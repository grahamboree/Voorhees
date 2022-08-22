using System;
using System.Diagnostics.CodeAnalysis;

namespace Voorhees {
    /// Thrown when trying to read invalid JSON data.
    public class InvalidJsonException : Exception {
        public InvalidJsonException(string message) : base(message) { }
    }
    
    public enum JsonToken {
        None,
        
        ArrayStart,
        ArrayEnd,
        
        ObjectStart,
        KeyValueSeparator, // : 
        ObjectEnd,
        
        Separator, // ,
        
        String,
        Number,
        True,
        False,
        Null,
        
        EOF
    }
    
    public class JsonTokenizer {
        public JsonToken NextToken = JsonToken.None;

        /////////////////////////////////////////////////

        /// <summary>
        /// Construct a new tokenizer from the start of the json document
        /// </summary>
        /// <param name="cursor">DocumentCursor into a JSON document string</param>
        public JsonTokenizer(Internal.DocumentCursor cursor) {
            this.cursor = cursor;
            AdvanceToNextToken();
        }
        
        /// <summary>
        /// Construct a new tokenizer from the start of the json document.
        /// Constructs a DocumentCursor from the given json string
        /// </summary>
        /// <param name="json">The JSON document</param>
        public JsonTokenizer(string json) {
            cursor = new Internal.DocumentCursor(json);
            AdvanceToNextToken();
        }

        public void SkipToken(JsonToken token) {
            if (token != NextToken) {
                throw new InvalidOperationException($"Attempting to skip a token of type {token} but the next token is {NextToken}");
            }

            switch (token) {
                case JsonToken.ArrayStart:
                case JsonToken.ArrayEnd:
                case JsonToken.ObjectStart:
                case JsonToken.ObjectEnd:
                case JsonToken.KeyValueSeparator:
                case JsonToken.Separator: cursor.Advance(); break;
                case JsonToken.True:
                case JsonToken.Null: cursor.AdvanceBy(4); break;
                case JsonToken.False: cursor.AdvanceBy(5); break;
                case JsonToken.String: SkipString(); break;
                case JsonToken.Number: ConsumeNumber(); break; // OK to consume here because it only just computes the bounds of the token
                case JsonToken.None:
                case JsonToken.EOF:
                default: throw new InvalidOperationException($"Can't skip token of type {token}");
            }
            AdvanceToNextToken();
        }

        /// <summary>
        /// Return a span of the current number token and advance to the next token. 
        /// </summary>
        /// <returns>The number's character span in the original json string</returns>
        /// <exception cref="InvalidOperationException">If the next token is not a properly formatted number</exception>
        public ReadOnlySpan<char> ConsumeNumber() {
            if (NextToken != JsonToken.Number) {
                throw new InvalidOperationException($"{cursor} Trying to consume a number, but the next JSON token is not a number");
            }
            
            int start = cursor.Index;

            // optional leading -
            if (!cursor.AtEOF && cursor.CurrentChar == '-') {
                cursor.Advance();
            }

            // a leading zero needs to be followed by a decimal or exponent marker
            if (!cursor.AtEOF && cursor.CurrentChar == '0') {
                cursor.Advance();
                if (cursor.AtEOF || (cursor.CurrentChar != '.' && cursor.CurrentChar != 'e' && cursor.CurrentChar != 'E')) {
                    throw new InvalidJsonException($"{cursor} Leading zero in a number must be immediately followed by a decimal point or exponent");
                }
            }

            // whole part digits
            while (!cursor.AtEOF && (cursor.CurrentChar >= '0' && cursor.CurrentChar <= '9')) {
                cursor.Advance();
            }
            
            // decimal
            if (!cursor.AtEOF && cursor.CurrentChar == '.') {
                cursor.Advance();

                // fractional part digits
                while (!cursor.AtEOF && (cursor.CurrentChar >= '0' && cursor.CurrentChar <= '9')) {
                    cursor.Advance();
                }
            }

            // Optional exponent
            if (!cursor.AtEOF && (cursor.CurrentChar == 'e' || cursor.CurrentChar == 'E')) {
                cursor.Advance();
                
                // optional + or -
                if (!cursor.AtEOF && (cursor.CurrentChar == '+' || cursor.CurrentChar == '-')) {
                    cursor.Advance();
                }

                // exponent digits
                while (!cursor.AtEOF && (cursor.CurrentChar >= '0' && cursor.CurrentChar <= '9')) {
                    cursor.Advance();
                }
            }
            int length = cursor.Index - start;
            AdvanceToNextToken();
            return cursor.Document.AsSpan(start, length);
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
        [return: NotNull]
        public string ConsumeString() {
            cursor.Advance(); // Skip the "
            int resultLength = 0; // Number of characters in the resulting string.
            bool hasEscapeChars = false; // False if the string contains escape codes that need to be parsed 
            var lookahead = cursor.Clone(); // We read ahead and determine the string length
            
            while (lookahead.NumCharsLeft > 0) {
                char readAheadChar = lookahead.CurrentChar;
                if (readAheadChar <= 0x1F || readAheadChar == 0x7F || (readAheadChar >= 0x80 && readAheadChar <= 0x9F)) {
                    throw new InvalidJsonException($"{lookahead} Disallowed control character in string");
                }
                
                if (readAheadChar == '\\') {
                    // This string isn't trivial, so use the normal slower parsing.
                    hasEscapeChars = true;
                    lookahead.Advance();
                    if (lookahead.CurrentChar == 'u') {
                        lookahead.AdvanceBy(4);
                    }
                } else if (readAheadChar == '"') {
                    break;
                }
                resultLength++;
                lookahead.Advance();
            }
            
            if (!hasEscapeChars) {
                return string.Create(resultLength, cursor.Document, (chars, doc) => {
                    doc.AsSpan().Slice(cursor.Index, resultLength).CopyTo(chars);
                    cursor.AdvanceBy(1 + resultLength); // skip to after the closing "
                    AdvanceToNextToken();
                });
            }

            return string.Create(resultLength, cursor.Document, (chars, doc) => {
                int resultIndex = 0;
                while (cursor.Index < lookahead.Index) {
                    switch (cursor.CurrentChar) {
                        case '\\': {
                            cursor.Advance();
                            switch (cursor.CurrentChar) {
                                case '\\': chars[resultIndex++] = '\\'; break;
                                case '"':  chars[resultIndex++] = '"';  break;
                                case '/':  chars[resultIndex++] = '/';  break;
                                case 'b':  chars[resultIndex++] = '\b'; break;
                                case 'f':  chars[resultIndex++] = '\f'; break;
                                case 'n':  chars[resultIndex++] = '\n'; break;
                                case 'r':  chars[resultIndex++] = '\r'; break;
                                case 't':  chars[resultIndex++] = '\t'; break;
                                case 'u': {
                                    // Read 4 hex digits
                                    chars[resultIndex++] = (char)Convert.ToInt16(cursor.Document.Substring(cursor.Index + 1, 4), 16);
                                    cursor.AdvanceBy(4);
                                } break;
                                default: throw new InvalidJsonException($"{cursor} Unknown escape character sequence");
                            }
                        }
                            break;
                        default:
                            chars[resultIndex++] = cursor.CurrentChar;
                            break;
                    }
                    cursor.Advance();
                }
                AdvanceToNextToken();
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
            
            if (cursor.NumCharsLeft <= 0) {
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
            }

            // number
            if (cursor.CurrentChar == '-' || (cursor.CurrentChar >= '0' && cursor.CurrentChar <= '9')) {
                NextToken = JsonToken.Number;
                return;
            }
            
            // true
            const string TOKEN_TRUE = "true";
            if (cursor.NumCharsLeft >= 4 && string.CompareOrdinal(cursor.Document, cursor.Index, TOKEN_TRUE, 0, TOKEN_TRUE.Length) == 0) {
                NextToken = JsonToken.True;
                return;
            }

            // false
            const string TOKEN_FALSE = "false";
            if (cursor.NumCharsLeft >= 5 && string.CompareOrdinal(cursor.Document, cursor.Index, TOKEN_FALSE, 0, TOKEN_FALSE.Length) == 0) {
                NextToken = JsonToken.False;
                return;
            }

            // null
            const string TOKEN_NULL = "null";
            if (cursor.NumCharsLeft >= 4 && string.CompareOrdinal(cursor.Document, cursor.Index, TOKEN_NULL, 0, TOKEN_NULL.Length) == 0) {
                NextToken = JsonToken.Null;
                return;
            }

            throw new InvalidJsonException($"{cursor} Unexpected character '{cursor.CurrentChar}'");
        }

        /// Same as ConsumeString but doesn't bother parsing the result.
        void SkipString() {
            // Skip the leading '"', then continue skipping until we hit EOF or the closing "
            for (cursor.Advance(); !cursor.AtEOF; cursor.Advance()) {
                if (cursor.CurrentChar <= 0x1F || cursor.CurrentChar == 0x7F || (cursor.CurrentChar >= 0x80 && cursor.CurrentChar <= 0x9F)) {
                    throw new InvalidJsonException($"{cursor} Disallowed control character in string");
                }

                if (cursor.CurrentChar == '\\') {
                    cursor.Advance();
                    if (cursor.CurrentChar == 'u') {
                        cursor.AdvanceBy(4);
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