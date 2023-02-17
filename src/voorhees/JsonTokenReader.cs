using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Voorhees {
    /// Reads json tokens from a json document stream.
    public class JsonTokenReader {
        /// The next Json token in the document.
        public JsonToken NextToken = JsonToken.None;

        /////////////////////////////////////////////////

        /// <summary>
        /// Read tokens using the given document cursor position 
        /// </summary>
        /// <param name="cursor">DocumentCursor into a JSON document</param>
        public JsonTokenReader(Internal.DocumentCursor cursor) {
            this.cursor = cursor;
            AdvanceToNextToken();
        }
        
        /// <summary>
        /// Read tokens from the start of the json string.
        /// </summary>
        /// <param name="json">The JSON document to read</param>
        public JsonTokenReader(TextReader json) : this (new Internal.DocumentCursor(json)) { }

        /// <summary>
        /// Skip over a token of the given type.  Ensures that a valid token of that type was actually skipped over.
        /// </summary>
        /// <param name="tokenType">The type of token to skip over</param>
        /// <exception cref="InvalidOperationException">
        /// If the next token in the document is not of type <paramref name="tokenType"/>
        /// or if a token of type <paramref name="tokenType"/> is unskippable. e.g. EOF or None. 
        /// </exception>
        public void SkipToken(JsonToken tokenType) {
            if (tokenType != NextToken) {
                throw new InvalidOperationException($"Attempting to skip a token of type {tokenType} but the next token is {NextToken}");
            }

            switch (tokenType) {
                case JsonToken.ArrayStart:
                case JsonToken.ArrayEnd:
                case JsonToken.ObjectStart:
                case JsonToken.ObjectEnd:
                case JsonToken.KeyValueSeparator:
                case JsonToken.Separator: cursor.Advance(); break;
                case JsonToken.True:
                case JsonToken.False:
                case JsonToken.Null: break; // We've already advanced the cursor for true, false and null.
                case JsonToken.String: SkipString(); break;
                case JsonToken.Number: ConsumeNumber(); break; // OK to consume because it just computes the bounds of the token and returns a ReadOnlySpan of the bounds
                
                // Can't skip these tokens
                case JsonToken.None:
                case JsonToken.EOF:
                default: throw new InvalidOperationException($"Can't skip token of type {tokenType}");
            }
            AdvanceToNextToken();
        }

        /// <summary>
        /// Return a span of the current number token and advance to the next token. 
        /// </summary>
        /// <returns>The number's character span in the original json string</returns>
        /// <exception cref="InvalidOperationException">If the next token is not a properly formatted number</exception>
        public string ConsumeNumber() {
            if (NextToken != JsonToken.Number) {
                throw new InvalidOperationException($"{cursor} Trying to consume a number, but the next JSON token is not a number");
            }

            int length = 0;
            Span<char> numberChars = stackalloc char[256];

            // optional leading -
            if (!cursor.AtEOF && cursor.CurrentChar == '-') {
                numberChars[length++] = cursor.CurrentChar;
                cursor.Advance();
            }

            bool leadingZero = !cursor.AtEOF && cursor.CurrentChar == '0';
            if (leadingZero) {
                numberChars[length++] = cursor.CurrentChar;
                cursor.Advance();
            }

            // whole part digits
            while (!cursor.AtEOF && (cursor.CurrentChar >= '0' && cursor.CurrentChar <= '9')) {
                if (leadingZero) {
                    throw new InvalidJsonException($"{cursor} Leading zero in a number must be immediately followed by a decimal point or exponent");
                }
                numberChars[length++] = cursor.CurrentChar;
                cursor.Advance();
            }
            
            // decimal
            if (!cursor.AtEOF && cursor.CurrentChar == '.') {
                numberChars[length++] = cursor.CurrentChar;
                cursor.Advance();

                // fractional part digits
                while (!cursor.AtEOF && (cursor.CurrentChar >= '0' && cursor.CurrentChar <= '9')) {
                    numberChars[length++] = cursor.CurrentChar;
                    cursor.Advance();
                }
            }

            // Optional exponent
            if (!cursor.AtEOF && (cursor.CurrentChar == 'e' || cursor.CurrentChar == 'E')) {
                numberChars[length++] = cursor.CurrentChar;
                cursor.Advance();
                
                // optional + or -
                if (!cursor.AtEOF && (cursor.CurrentChar == '+' || cursor.CurrentChar == '-')) {
                    numberChars[length++] = cursor.CurrentChar;
                    cursor.Advance();
                }

                // exponent digits
                while (!cursor.AtEOF && (cursor.CurrentChar >= '0' && cursor.CurrentChar <= '9')) {
                    numberChars[length++] = cursor.CurrentChar;
                    cursor.Advance();
                }
            }
            
            AdvanceToNextToken();
            return new string(numberChars[..length]);
        }

        /// <summary>
        /// Return a copy of the current string token and advance to the next token.
        /// </summary>
        /// <returns>A copy of the current string token</returns>
        /// <exception cref="InvalidJsonException">
        /// If the string contains a
        /// <a href="https://www.crockford.com/mckeeman.html">disallowed control character</a>
        /// or a malformed escape character sequence.
        /// </exception>
        /// <exception cref="InvalidOperationException">If the next token is not a string</exception>
        [return: NotNull] // If we're consuming a string, it's always a valid string token, so it'll never be null.
        public string ConsumeString() {
            if (NextToken != JsonToken.String) {
                throw new InvalidOperationException($"{cursor} Trying to consume a string, but the next JSON token is not a string");
            }
            
            cursor.Advance(); // Skip the "
            
            Span<char> hexChars = stackalloc char[4];
            readChars.Clear();
            
            while (!cursor.AtEOF) {
                if (cursor.CurrentChar == '"') {
                    cursor.Advance();
                    break;
                }
                
                if (cursor.CurrentChar == '\\') {
                    cursor.Advance();
                    switch (cursor.CurrentChar) {
                        case '\\':
                            readChars.Add('\\');
                            break;
                        case '"':
                            readChars.Add('"');
                            break;
                        case '/':
                            readChars.Add('/');
                            break;
                        case 'b':
                            readChars.Add('\b');
                            break;
                        case 'f':
                            readChars.Add('\f');
                            break;
                        case 'n':
                            readChars.Add('\n');
                            break;
                        case 'r':
                            readChars.Add('\r');
                            break;
                        case 't':
                            readChars.Add('\t');
                            break;
                        case 'u': {
                            cursor.Advance();
                            // Read 4 hex digits
                            hexChars[0] = cursor.CurrentChar;
                            cursor.Advance();
                            hexChars[1] = cursor.CurrentChar;
                            cursor.Advance();
                            hexChars[2] = cursor.CurrentChar;
                            cursor.Advance();
                            hexChars[3] = cursor.CurrentChar;
                            readChars.Add((char)Convert.ToInt16(new string(hexChars), 16));
                        } break;
                        default: throw new InvalidJsonException($"{cursor} Unknown escape character sequence");
                    }
                } else if (cursor.CurrentChar <= 0x1F || cursor.CurrentChar == 0x7F || (cursor.CurrentChar >= 0x80 && cursor.CurrentChar <= 0x9F)) {
                    throw new InvalidJsonException($"{cursor} Disallowed control character in string");
                } else {
                    readChars.Add(cursor.CurrentChar);
                }

                cursor.Advance();
            }

            AdvanceToNextToken();
            return string.Create(readChars.Count, readChars, (chars, read) => {
                for (int i = 0; i < read.Count; ++i) {
                    chars[i] = read[i];
                }
            });
        }

        /// <summary>
        /// Generates a string containing info about the current line and column numbers that's useful for
        /// prepending to error messages and exceptions.
        /// </summary>
        /// <returns>string containing line and column info for the current cursor position</returns>
        public string LineColString => cursor.ToString();

        /////////////////////////////////////////////////
        
        readonly Internal.DocumentCursor cursor;
        readonly List<char> readChars = new(512); //128 

        /////////////////////////////////////////////////

        /// <summary>
        /// Moves to the next non-whitespace token.
        /// Identifies the type of token that starts at the cursor position and sets <see cref="NextToken"/>.
        /// </summary>
        /// <exception cref="InvalidJsonException">
        /// If the next non-whitespace character does not begin a valid JSON token.
        /// </exception>
        void AdvanceToNextToken() {
            // Skip whitespace
            cursor.AdvanceToNextNonWhitespaceChar();
            
            // Detect next token type
            
            if (cursor.AtEOF) {
                NextToken = JsonToken.EOF;
                return;
            }

            switch (cursor.CurrentChar) {
                case '[': NextToken = JsonToken.ArrayStart; return;
                case ']': NextToken = JsonToken.ArrayEnd; return;
                case '{': NextToken = JsonToken.ObjectStart; return;
                case '}': NextToken = JsonToken.ObjectEnd; return;
                case ',': NextToken = JsonToken.Separator; return;
                case '"': NextToken = JsonToken.String; return;
                case ':': NextToken = JsonToken.KeyValueSeparator; return;
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9': NextToken = JsonToken.Number; return;
                case 't': { // true
                    cursor.Advance(); if (cursor.CurrentChar != 'r') { break; }
                    cursor.Advance(); if (cursor.CurrentChar != 'u') { break; }
                    cursor.Advance(); if (cursor.CurrentChar != 'e') { break; }
                    cursor.Advance();
                    NextToken = JsonToken.True;
                    return;
                }
                case 'f': { // false
                    cursor.Advance(); if (cursor.CurrentChar != 'a') { break; }
                    cursor.Advance(); if (cursor.CurrentChar != 'l') { break; }
                    cursor.Advance(); if (cursor.CurrentChar != 's') { break; }
                    cursor.Advance(); if (cursor.CurrentChar != 'e') { break; }
                    cursor.Advance();
                    NextToken = JsonToken.False;
                    return;
                }
                case 'n': { // null
                    cursor.Advance(); if (cursor.CurrentChar != 'u') { break; }
                    cursor.Advance(); if (cursor.CurrentChar != 'l') { break; }
                    cursor.Advance(); if (cursor.CurrentChar != 'l') { break; }
                    cursor.Advance();
                    NextToken = JsonToken.Null;
                    return;
                }
            }

            throw new InvalidJsonException($"{cursor} Unexpected character '{cursor.CurrentChar}'");
        }

        /// <summary>
        /// Same as ConsumeString but doesn't bother parsing the result.
        /// </summary>
        /// <exception cref="InvalidJsonException">If the string is never terminated or contains a disallowed control character</exception>
        void SkipString() {
            // Skip the leading '"', then continue skipping until we hit the closing "
            for (cursor.Advance(); ; cursor.Advance()) {
                if (cursor.AtEOF) {
                    throw new InvalidJsonException($"{cursor} Unexpected EOF while reading a string value");
                }
                
                if (cursor.CurrentChar <= 0x1F || cursor.CurrentChar == 0x7F || (cursor.CurrentChar >= 0x80 && cursor.CurrentChar <= 0x9F)) {
                    throw new InvalidJsonException($"{cursor} Disallowed control character in string");
                }

                if (cursor.CurrentChar == '\\') {
                    cursor.Advance();
                    if (cursor.CurrentChar == 'u') {
                        cursor.Advance(4);
                    }
                } else if (cursor.CurrentChar == '"') {
                    cursor.Advance();
                    break;
                }
            }
            
            AdvanceToNextToken();
        }
    }
}