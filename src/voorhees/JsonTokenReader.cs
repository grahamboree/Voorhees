using System;
using System.Diagnostics.CodeAnalysis;

namespace Voorhees {
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
        public JsonTokenReader(string json) {
            cursor = new Internal.DocumentCursor(json);
            AdvanceToNextToken();
        }

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
                case JsonToken.Null: cursor.AdvanceBy(4); break;
                case JsonToken.False: cursor.AdvanceBy(5); break;
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
        public ReadOnlySpan<char> ConsumeNumber() {
            if (NextToken != JsonToken.Number) {
                throw new InvalidOperationException($"{cursor} Trying to consume a number, but the next JSON token is not a number");
            }
            
            int start = cursor.Index;

            // optional leading -
            if (!cursor.AtEOF && cursor.CurrentChar == '-') {
                cursor.Advance();
            }

            bool leadingZero = !cursor.AtEOF && cursor.CurrentChar == '0';
            if (leadingZero) {
                cursor.Advance();
            }

            // whole part digits
            while (!cursor.AtEOF && (cursor.CurrentChar >= '0' && cursor.CurrentChar <= '9')) {
                if (leadingZero) {
                    throw new InvalidJsonException($"{cursor} Leading zero in a number must be immediately followed by a decimal point or exponent");
                }
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
        /// <exception cref="InvalidOperationException">If the next token is not a string</exception>
        [return: NotNull] // If we're consuming a string, it's always a valid string token, so it'll never be null.
        public string ConsumeString() {
            if (NextToken != JsonToken.String) {
                throw new InvalidOperationException($"{cursor} Trying to consume a string, but the next JSON token is not a string");
            }
            
            cursor.Advance(); // Skip the "
            int resultLength = 0; // Number of characters in the resulting string.
            bool hasEscapeChars = false; // False if the string contains escape codes that need to be parsed

            int lookaheadIndex = cursor.Index;
            for (; lookaheadIndex < cursor.Document.Length; ++lookaheadIndex) {
                char readAheadChar = cursor.Document[lookaheadIndex];
                if (readAheadChar <= 0x1F || readAheadChar == 0x7F || (readAheadChar >= 0x80 && readAheadChar <= 0x9F)) {
                    cursor.AdvanceBy(lookaheadIndex - cursor.Index);
                    throw new InvalidJsonException($"{cursor} Disallowed control character in string");
                }
                
                if (readAheadChar == '\\') {
                    // This string isn't trivial, so use the normal slower parsing.
                    hasEscapeChars = true;
                    lookaheadIndex++;
                    if (cursor.Document[lookaheadIndex] == 'u') {
                        lookaheadIndex += 4;
                    }
                } else if (readAheadChar == '"') {
                    break;
                }
                resultLength++;
            }

            if (!hasEscapeChars) {
                var data = new StringGeneratorContextData {
                    Cursor = cursor,
                    EndIndex = lookaheadIndex,
                    NumChars = resultLength
                };
                string result = string.Create(resultLength, data, (chars, genData) => {
                    genData.Cursor.Document.AsSpan().Slice(genData.Cursor.Index, genData.NumChars).CopyTo(chars);
                });
                cursor.AdvanceBy(1 + resultLength); // skip to after the closing "
                AdvanceToNextToken();
                return result;
            } else {
                var data = new StringGeneratorContextData {
                    Cursor = cursor,
                    EndIndex = lookaheadIndex,
                    NumChars = resultLength
                };

                string result = string.Create(resultLength, data, (chars, genData) => {
                    int resultIndex = 0;
                    while (genData.Cursor.Index < genData.EndIndex) {
                        switch (genData.Cursor.CurrentChar) {
                            case '\\': {
                                genData.Cursor.Advance();
                                switch (genData.Cursor.CurrentChar) {
                                    case '\\':
                                        chars[resultIndex++] = '\\';
                                        break;
                                    case '"':
                                        chars[resultIndex++] = '"';
                                        break;
                                    case '/':
                                        chars[resultIndex++] = '/';
                                        break;
                                    case 'b':
                                        chars[resultIndex++] = '\b';
                                        break;
                                    case 'f':
                                        chars[resultIndex++] = '\f';
                                        break;
                                    case 'n':
                                        chars[resultIndex++] = '\n';
                                        break;
                                    case 'r':
                                        chars[resultIndex++] = '\r';
                                        break;
                                    case 't':
                                        chars[resultIndex++] = '\t';
                                        break;
                                    case 'u': {
                                        // Read 4 hex digits
                                        chars[resultIndex++] = (char)Convert.ToInt16(genData.Cursor.Document.Substring(genData.Cursor.Index + 1, 4), 16);
                                        genData.Cursor.AdvanceBy(4);
                                    }
                                        break;
                                    default: throw new InvalidJsonException($"{genData.Cursor} Unknown escape character sequence");
                                }
                            }
                                break;
                            default:
                                chars[resultIndex++] = genData.Cursor.CurrentChar;
                                break;
                        }
                        genData.Cursor.Advance();
                    }
                    genData.Cursor.Advance();
                });

                AdvanceToNextToken();
                return result;
            }
        }

        /// <summary>
        /// Generates a string containing info about the current line and column numbers that's useful for
        /// prepending to error messages and exceptions.
        /// </summary>
        /// <returns>string containing line and column info for the current cursor position</returns>
        public string LineColString => cursor.ToString();

        /////////////////////////////////////////////////
        
        readonly Internal.DocumentCursor cursor;

        struct StringGeneratorContextData {
            public Internal.DocumentCursor Cursor;
            public int EndIndex;
            public int NumChars;
        }
        
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