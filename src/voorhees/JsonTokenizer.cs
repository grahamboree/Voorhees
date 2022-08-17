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
        Internal.DocumentCursor Doc;
        
        /////////////////////////////////////////////////

        /// <summary>
        /// Construct a new tokenizer from the start of the json document
        /// </summary>
        /// <param name="cursor">DocumentCursor into a JSON document string</param>
        public JsonTokenizer(Internal.DocumentCursor cursor) {
            Doc = cursor;
            AdvanceToNextToken();
        }
        
        /// <summary>
        /// Construct a new tokenizer from the start of the json document.
        /// Constructs a DocumentCursor from the given json string
        /// </summary>
        /// <param name="json">The JSON document</param>
        public JsonTokenizer(string json) {
            Doc = new Internal.DocumentCursor(json);
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
                case JsonToken.Separator: Doc.AdvanceCursorBy(1); break;
                case JsonToken.True:
                case JsonToken.Null: Doc.AdvanceCursorBy(4); break;
                case JsonToken.False: Doc.AdvanceCursorBy(5); break;
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
                throw new InvalidOperationException($"{Doc} Trying to consume a number, but the next JSON token is not a number");
            }
            
            int start = Doc.Cursor;
            int end = start;
            
            // optional leading -
            if (end < Doc.Document.Length && Doc.Document[end] == '-') {
                end++;
            }

            // a leading zero needs to be followed by a decimal or exponent marker
            if (end < Doc.Document.Length && Doc.Document[end] == '0') {
                if (end + 1 >= Doc.Document.Length || (Doc.Document[end + 1] != '.' && Doc.Document[end + 1] != 'e' && Doc.Document[end + 1] != 'E')) {
                    throw new InvalidJsonException($"{Doc} Leading zero in a number must be immediately followed by a decimal point or exponent");
                }
            }

            // whole part digits
            while (end < Doc.Document.Length && (Doc.Document[end] >= '0' && Doc.Document[end] <= '9')) {
                end++;
            }
            
            // decimal
            if (end < Doc.Document.Length && Doc.Document[end] == '.') {
                end++;
                
                // fractional part digits
                while (end < Doc.Document.Length && (Doc.Document[end] >= '0' && Doc.Document[end] <= '9')) {
                    end++;
                }
            }

            // Optional exponent
            if (end < Doc.Document.Length && (Doc.Document[end] == 'e' || Doc.Document[end] == 'E')) {
                end++;
                
                // optional + or -
                if (end < Doc.Document.Length && (Doc.Document[end] == '+' || Doc.Document[end] == '-')) {
                    end++;
                }

                // exponent digits
                while (end < Doc.Document.Length && (Doc.Document[end] >= '0' && Doc.Document[end] <= '9')) {
                    end++;
                }
            }
            int length = end - start;

            Doc.AdvanceCursorBy(length);
            AdvanceToNextToken();
            
            return Doc.Document.AsSpan(start, length);
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
            int start = Doc.Cursor + 1; // Skip the '"'
            int end = start; // Where the ending " is
            int resultLength = 0; // Number of characters in the resulting string.

            // Read the string length
            bool trivial = true;
            for (int readAheadIndex = start; readAheadIndex < Doc.Document.Length; ++readAheadIndex) {
                char readAheadChar = Doc.Document[readAheadIndex];
                if (readAheadChar <= 0x1F || readAheadChar == 0x7F || (readAheadChar >= 0x80 && readAheadChar <= 0x9F)) {
                    // TODO: This should indicate the line and column number for the offending character, not the start of the string.
                    throw new InvalidJsonException($"{LineColString} Disallowed control character in string");
                }
                
                if (readAheadChar == '\\') {
                    // This string isn't trivial, so use the normal slower parsing.
                    trivial = false;
                    readAheadIndex++;
                    if (Doc.Document[readAheadIndex] == 'u') {
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
                Doc.Document.AsSpan().Slice(start, resultLength).CopyTo(s);
                
                Doc.AdvanceCursorBy(2 + (end - start)); // skip to after the closing "
                AdvanceToNextToken();
                
                return new string(s); // TODO to string.Create
            }
            
            Span<char> result = stackalloc char[resultLength];
            int resultIndex = 0;
            
            for (int current = start; current < end; ++current) {
                switch (Doc.Document[current]) {
                    case '\\': {
                        current++;
                        switch (Doc.Document[current]) {
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
                                result[resultIndex++] = (char)Convert.ToInt16(Doc.Document.Substring(current + 1, 4), 16);
                                current += 4;
                            } break;
                            default: throw new InvalidJsonException($"{Doc} Unknown escape character sequence");
                        }
                    }
                    break;
                default:
                    result[resultIndex++] = Doc.Document[current];
                    break;
                }
            }
            
            Doc.AdvanceCursorBy(2 + (end - start)); // skip to after the closing "
            AdvanceToNextToken();
            return new string(result); // TODO To String.Create
        }

        /// <summary>
        /// Generates a string containing info about the current line and column numbers that's useful for
        /// prepending to error messages and exceptions.
        /// </summary>
        /// <returns>string containing line and column info for the current cursor position</returns>
        public string LineColString => Doc.ToString();

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
            Doc.AdvanceToNextNonWhitespaceChar();
            
            // Detect next token type
            
            if (Doc.CharsLeft <= 0) {
                NextToken = JsonToken.EOF;
                return;
            }

            switch (Doc.Document[Doc.Cursor]) {
                case '[': NextToken = JsonToken.ArrayStart; return;
                case ']': NextToken = JsonToken.ArrayEnd; return;
                case '{': NextToken = JsonToken.ObjectStart; return;
                case '}': NextToken = JsonToken.ObjectEnd; return;
                case ',': NextToken = JsonToken.Separator; return;
                case '"': NextToken = JsonToken.String; return;
                case ':': NextToken = JsonToken.KeyValueSeparator; return;
            }

            // number
            if (Doc.Document[Doc.Cursor] == '-' || (Doc.Document[Doc.Cursor] >= '0' && Doc.Document[Doc.Cursor] <= '9')) {
                NextToken = JsonToken.Number;
                return;
            }
            
            // true
            const string TOKEN_TRUE = "true";
            if (Doc.CharsLeft >= 4 && string.CompareOrdinal(Doc.Document, Doc.Cursor, TOKEN_TRUE, 0, TOKEN_TRUE.Length) == 0) {
                NextToken = JsonToken.True;
                return;
            }

            // false
            const string TOKEN_FALSE = "false";
            if (Doc.CharsLeft >= 5 && string.CompareOrdinal(Doc.Document, Doc.Cursor, TOKEN_FALSE, 0, TOKEN_FALSE.Length) == 0) {
                NextToken = JsonToken.False;
                return;
            }

            // null
            const string TOKEN_NULL = "null";
            if (Doc.CharsLeft >= 4 && string.CompareOrdinal(Doc.Document, Doc.Cursor, TOKEN_NULL, 0, TOKEN_NULL.Length) == 0) {
                NextToken = JsonToken.Null;
                return;
            }

            throw new InvalidJsonException($"{Doc} Unexpected character '{Doc.Document[Doc.Cursor]}'");
        }

        /// Same as ConsumeString but doesn't bother parsing the result.
        void SkipString() {
            int start = Doc.Cursor + 1; // Skip the '"'
            int end; // Where the ending " is

            // Read the string length
            for (end = start; end < Doc.Document.Length; ++end) {
                char readAheadChar = Doc.Document[end];
                if (readAheadChar <= 0x1F || readAheadChar == 0x7F || (readAheadChar >= 0x80 && readAheadChar <= 0x9F)) {
                    // TODO: This should indicate the line and column number for the offending character, not the start of the string.
                    throw new InvalidJsonException($"{Doc} Disallowed control character in string");
                }
                
                if (readAheadChar == '\\') {
                    end++;
                    if (Doc.Document[end] == 'u') {
                        end += 4;
                    }
                } else if (readAheadChar == '"') {
                    break;
                }
            }

            Doc.AdvanceCursorBy(2 + (end - start)); // skip to after the closing "
            AdvanceToNextToken();
        }
    }
}