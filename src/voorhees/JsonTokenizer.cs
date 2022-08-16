using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
        public readonly string JsonData;
        public JsonToken NextToken = JsonToken.None;
        
        /// Index of the current character in the entire json document. 0-indexed
        public int Cursor;
        /// Index of the current line in the json document. 1-indexed
        public int Line;
        /// Index of the current character on the current line in the json document.  1-indexed
        public int Column;
        
        /////////////////////////////////////////////////

        /// <summary>
        /// Construct a new tokenizer from the start of the given json string.
        /// </summary>
        /// <param name="jsonData">JSON to parse</param>
        /// <exception cref="ArgumentException">If <see cref="jsonData"/> is null.</exception>
        public JsonTokenizer(string jsonData) {
            JsonData = jsonData ?? throw new ArgumentException("Json string is null", nameof(jsonData));
            
            Cursor = 0;
            Line = 1;
            Column = 1;
            
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
                case JsonToken.KeyValueSeparator:
                case JsonToken.ObjectEnd:
                case JsonToken.Separator: AdvanceCursorBy(1); break;
                case JsonToken.True: AdvanceCursorBy(4); break;
                case JsonToken.False: AdvanceCursorBy(5); break;
                case JsonToken.Null: AdvanceCursorBy(4); break;
                case JsonToken.None:
                case JsonToken.String:
                case JsonToken.Number:
                case JsonToken.EOF:
                default:
                    throw new InvalidOperationException($"Can't skip token of type {token}");
            }
            AdvanceToNextToken();
        }

        /// <summary>
        /// Return a copy the current number token and advance <see cref="Cursor"/> to the next token. 
        /// </summary>
        /// <returns>A ReadOnlySpan mapping to the number character span in the original JsonData string</returns>
        /// <exception cref="InvalidOperationException">If the next token is not a properly formatted number</exception>
        public ReadOnlySpan<char> ConsumeNumber() {
            if (NextToken != JsonToken.Number) {
                throw new InvalidOperationException($"{LineColString} Trying to consume a number, but the next JSON token is not a number");
            }
            
            int start = Cursor;
            int end = start;
            
            // optional leading -
            if (end < JsonData.Length && JsonData[end] == '-') {
                end++;
            }

            // a leading zero needs to be followed by a decimal or exponent marker
            if (end < JsonData.Length && JsonData[end] == '0') {
                if (end + 1 >= JsonData.Length || (JsonData[end + 1] != '.' && JsonData[end + 1] != 'e' && JsonData[end + 1] != 'E')) {
                    throw new InvalidJsonException($"{LineColString} Leading zero in a number must be immediately followed by a decimal point or exponent");
                }
            }

            // whole part digits
            while (end < JsonData.Length && (JsonData[end] >= '0' && JsonData[end] <= '9')) {
                end++;
            }
            
            // decimal
            if (end < JsonData.Length && JsonData[end] == '.') {
                end++;
                
                // fractional part digits
                while (end < JsonData.Length && (JsonData[end] >= '0' && JsonData[end] <= '9')) {
                    end++;
                }
            }

            // Optional exponent
            if (end < JsonData.Length && (JsonData[end] == 'e' || JsonData[end] == 'E')) {
                end++;
                
                // optional + or -
                if (end < JsonData.Length && (JsonData[end] == '+' || JsonData[end] == '-')) {
                    end++;
                }

                // exponent digits
                while (end < JsonData.Length && (JsonData[end] >= '0' && JsonData[end] <= '9')) {
                    end++;
                }
            }
            int length = end - start;

            AdvanceCursorBy(length);
            AdvanceToNextToken();
            
            return JsonData.AsSpan(start, length);
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
            int start = Cursor + 1; // Skip the '"'
            int end = start; // Where the ending " is
            int resultLength = 0; // Number of characters in the resulting string.

            // Read the string length
            bool trivial = true;
            for (int readAheadIndex = start; readAheadIndex < JsonData.Length; ++readAheadIndex) {
                char readAheadChar = JsonData[readAheadIndex];
                if (readAheadChar <= 0x1F || readAheadChar == 0x7F || (readAheadChar >= 0x80 && readAheadChar <= 0x9F)) {
                    // TODO: This should indicate the line and column number for the offending character, not the start of the string.
                    throw new InvalidJsonException($"{LineColString} Disallowed control character in string");
                }
                
                if (readAheadChar == '\\') {
                    // This string isn't trivial, so use the normal slower parsing.
                    trivial = false;
                    readAheadIndex++;
                    if (JsonData[readAheadIndex] == 'u') {
                        readAheadIndex += 4;
                    }
                } else if (readAheadChar == '"') {
                    end = readAheadIndex;
                    break;
                }
                resultLength++;
            }

            if (trivial) {
                // TODO Use string.Create
                Span<char> s = stackalloc char[resultLength];
                JsonData.AsSpan().Slice(start, resultLength).CopyTo(s);
                
                AdvanceCursorBy(2 + (end - start)); // skip to after the closing "
                AdvanceToNextToken();
                
                return new string(s); // TODO to string.Create
            }
            
            Span<char> result = stackalloc char[resultLength];
            int resultIndex = 0;
            
            for (int current = start; current < end; ++current) {
                switch (JsonData[current]) {
                    case '\\': {
                        current++;
                        switch (JsonData[current]) {
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
                                short codePoint = Convert.ToInt16(JsonData.Substring(current + 1, 4), 16);
                                current += 4;
                                result[resultIndex++] = (char)codePoint;
                            } break;
                            default:
                                throw new InvalidJsonException($"{LineColString} Unknown escape character sequence");
                        }
                    }
                    break;
                default:
                    result[resultIndex++] = JsonData[current];
                    break;
                }
            }
            
            AdvanceCursorBy(2 + (end - start)); // skip to after the closing "
            AdvanceToNextToken();
            return new string(result); // TODO To String.Create
        }

        /// <summary>
        /// Generates a string containing info about the current line and column numbers that's useful for
        /// prepending to error messages and exceptions.
        /// </summary>
        /// <returns>string containing line and column info for the current cursor position</returns>
        public string LineColString => $"line: {Line} col: {Column}";

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
                AdvanceCursorBy(1);
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

            throw new InvalidJsonException($"{LineColString} Unexpected character '{JsonData[Cursor]}'");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AdvanceCursorBy(int numCharacters) {
            for (int i = 0; i < numCharacters; ++i) {
                if (JsonData[Cursor] == '\n') {
                    Line++;
                    Column = 0;
                }
                Cursor++;
                Column++;
            }
        }
    }
}