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
        public int Cursor;
        public readonly string JsonData;
        
        /////////////////////////////////////////////////

        /// <summary>
        /// Construct a new tokenizer from the start of the given json string.
        /// </summary>
        /// <param name="jsonData">JSON to parse</param>
        /// <exception cref="ArgumentException">If <see cref="jsonData"/> is null.</exception>
        public JsonTokenizer(string jsonData) {
            JsonData = jsonData ?? throw new ArgumentException("Json string is null", nameof(jsonData));
            Cursor = 0;

            AdvanceToNextToken();
        }
        
        /// Advance <see cref="Cursor"/> to the next token in the JSON string.
        public void ConsumeToken() {
            switch (NextToken) {
                case JsonToken.ArrayStart:
                case JsonToken.ArrayEnd:
                case JsonToken.ObjectStart:
                case JsonToken.KeyValueSeparator:
                case JsonToken.ObjectEnd:
                case JsonToken.Separator:
                    Cursor++;
                    break;
                case JsonToken.String: ConsumeString(); break;
                case JsonToken.Number: ConsumeNumber(); break;
                case JsonToken.True: Cursor += 4; break;
                case JsonToken.False: Cursor += 5; break;
                case JsonToken.Null: Cursor += 4; break;
                case JsonToken.EOF:
                    break;
            }
            AdvanceToNextToken();
        }

        /// <summary>
        /// Return a copy the current number token and advance <see cref="Cursor"/> to the next token. 
        /// </summary>
        /// <returns>A copy of the number token as a parseable string.</returns>
        /// <exception cref="InvalidOperationException">If the next token in the string is not a number</exception>
        [return: NotNull]
        public string ConsumeNumber() {
            if (NextToken != JsonToken.Number) {
                throw new InvalidOperationException("Trying to consume a number, but the next JSON token is not a number.");
            }
            
            int start = Cursor;
            
            // optional leading -
            if (Cursor < JsonData.Length && JsonData[Cursor] == '-') {
                Cursor++;
            }

            // a leading zero needs to be followed by a decimal or exponent marker
            if (Cursor < JsonData.Length && JsonData[Cursor] == '0') {
                if (Cursor + 1 >= JsonData.Length || (JsonData[Cursor + 1] != '.' && JsonData[Cursor + 1] != 'e' && JsonData[Cursor + 1] != 'E')) {
                    throw new InvalidJsonException($"Leading zero in a number must be immediately followed by a decimal point or exponent at character {start}");
                }
            }

            // whole part digits
            while (Cursor < JsonData.Length && (JsonData[Cursor] >= '0' && JsonData[Cursor] <= '9')) {
                Cursor++;
            }
            
            // decimal
            if (Cursor < JsonData.Length && JsonData[Cursor] == '.') {
                Cursor++;
                
                // fractional part digits
                while (Cursor < JsonData.Length && (JsonData[Cursor] >= '0' && JsonData[Cursor] <= '9')) {
                    Cursor++;
                }
            }

            // Optional exponent
            if (Cursor < JsonData.Length && (JsonData[Cursor] == 'e' || JsonData[Cursor] == 'E')) {
                Cursor++;
                
                // optional + or -
                if (Cursor < JsonData.Length && (JsonData[Cursor] == '+' || JsonData[Cursor] == '-')) {
                    Cursor++;
                }

                // exponent digits
                while (Cursor < JsonData.Length && (JsonData[Cursor] >= '0' && JsonData[Cursor] <= '9')) {
                    Cursor++;
                }
            }
            
            string numberString = JsonData.Substring(start, Cursor - start);
            AdvanceToNextToken();
            return numberString;
        }

        /// <summary>
        /// Return a copy of the current string token and advance <see cref="Cursor"/> to the next token.
        /// </summary>
        /// <returns>A copy of the current string token</returns>
        /// <exception cref="InvalidJsonException">
        /// If the string contains a
        /// <a href="https://www.crockford.com/mckeeman.html">disallowed control character</a>
        /// or a malformed escape character sequence.
        /// </exception>
        [return: NotNull]
        public string ConsumeString() {
            Cursor++; // Skip the '"'
            
            // Read the string length
            int len = 0;
            int endCursor = Cursor; // Where the ending " is in JsonData
            bool trivial = true;
            for (int readAheadIndex = Cursor; readAheadIndex < JsonData.Length; ++readAheadIndex) {
                char readAheadChar = JsonData[readAheadIndex];
                if (readAheadChar <= 0x1F || readAheadChar == 0x7F || (readAheadChar >= 0x80 && readAheadChar <= 0x9F)) {
                    throw new InvalidJsonException($"Disallowed control character in string at column {readAheadIndex}!");
                }
                
                if (readAheadChar == '\\') {
                    trivial = false; // This string isn't trivial, so use the normal expensive parsing.
                    readAheadIndex++;
                    if (JsonData[readAheadIndex] == 'u') {
                        readAheadIndex += 4;
                    }
                } else if (readAheadChar == '"') {
                    endCursor = readAheadIndex;
                    break;
                }
                len++;
            }

            if (trivial) {
                Span<char> s = stackalloc char[len];
                JsonData.AsSpan().Slice(Cursor, len).CopyTo(s);
                
                Cursor += len + 1; // skip to after the closing "
                AdvanceToNextToken();
                
                return new string(s);
            }
            
            Span<char> result = stackalloc char[len];
            int resultIndex = 0;
            
            for (; Cursor < endCursor; ++Cursor) {
                switch (JsonData[Cursor]) {
                    case '\\': {
                        Cursor++;
                        switch (JsonData[Cursor]) {
                            case '\\': result[resultIndex++] = '\\'; break;
                            case '"':  result[resultIndex++] = '"';  break;
                            case '/':  result[resultIndex++] = '/';  break;
                            case 'b':  result[resultIndex++] = '\b'; break;
                            case 'f':  result[resultIndex++] = '\f'; break;
                            case 'n':  result[resultIndex++] = '\n'; break;
                            case 'r':  result[resultIndex++] = '\r'; break;
                            case 't':  result[resultIndex++] = '\t'; break;
                            case 'u': {
                                // Read 4 hex digits
                                short codePoint = Convert.ToInt16(JsonData.Substring(Cursor + 1, 4), 16);
                                Cursor += 4;
                                result[resultIndex++] = char.ConvertFromUtf32(codePoint)[0];
                            } break;
                            default:
                                throw new InvalidJsonException($"Unknown escape character sequence: \\{JsonData[Cursor]} at column {Cursor}!");
                        }
                    }
                    break;
                default:
                    result[resultIndex++] = JsonData[Cursor];
                    break;
                }
            }
            Cursor++; // Skip closing "
            
            AdvanceToNextToken();
            return new string(result);
        }

        /////////////////////////////////////////////////

        /// <summary>
        /// Moves <see cref="Cursor"/> to the next non-whitespace token.
        /// Identifies the type of token that starts at the cursor position and sets <see cref="NextToken"/>.
        /// </summary>
        /// <exception cref="InvalidJsonException">
        /// If the next non-whitespace character does not begin a valid JSON token.
        /// </exception>
        void AdvanceToNextToken() {
            // Skip whitespace
            while (Cursor < JsonData.Length && char.IsWhiteSpace(JsonData[Cursor])) {
                Cursor++;
            }
            
            // Detect token type
            
            int charsLeft = JsonData.Length - Cursor;

            if (charsLeft <= 0) {
                NextToken = JsonToken.EOF;
                return;
            }

            switch (JsonData[Cursor]) {
                case '[': NextToken = JsonToken.ArrayStart; return;
                case ']': NextToken = JsonToken.ArrayEnd; return;
                case '{': NextToken = JsonToken.ObjectStart; return;
                case '}': NextToken = JsonToken.ObjectEnd; return;
                case ',': NextToken = JsonToken.Separator; return;
                case '"': NextToken = JsonToken.String; return;
                case ':': NextToken = JsonToken.KeyValueSeparator; return;
            }

            // number
            if (JsonData[Cursor] == '-' || (JsonData[Cursor] >= '0' && JsonData[Cursor] <= '9')) {
                NextToken = JsonToken.Number;
                return;
            }
            
            // true
            const string TOKEN_TRUE = "true";
            if (charsLeft >= 4 && string.CompareOrdinal(JsonData, Cursor, TOKEN_TRUE, 0, TOKEN_TRUE.Length) == 0) {
                NextToken = JsonToken.True;
                return;
            }

            // false
            const string TOKEN_FALSE = "false";
            if (charsLeft >= 5 && string.CompareOrdinal(JsonData, Cursor, TOKEN_FALSE, 0, TOKEN_FALSE.Length) == 0) {
                NextToken = JsonToken.False;
                return;
            }

            // null
            const string TOKEN_NULL = "null";
            if (charsLeft >= 4 && string.CompareOrdinal(JsonData, Cursor, TOKEN_NULL, 0, TOKEN_NULL.Length) == 0) {
                NextToken = JsonToken.Null;
                return;
            }

            throw new InvalidJsonException($"Unexpected character '{JsonData[Cursor]}' at character {Cursor}!");
        }
    }
}