using System;
using System.Text;

namespace Voorhees {
    /// Thrown when trying to rea invalid JSON data.
    /// The <c>message</c> field will contain more info about the specific
    /// json format error
    public class InvalidJsonException : Exception {
        public InvalidJsonException(string message) : base(message) {
        }
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
        public int Cursor = 0;
        public readonly string Json;
        
        /////////////////////////////////////////////////

        public JsonTokenizer(string jsonString) {
            if (jsonString == null) {
                throw new ArgumentException("Json string is null", nameof(jsonString));
            }
            Json = jsonString;
            Cursor = 0;
            
            SkipToNextToken();
        }

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
                default:
                    throw new ArgumentOutOfRangeException();
            }
            SkipToNextToken();
        }

        public string ConsumeNumber() {
            int start = Cursor;
            while (Cursor < Json.Length && isNumberChar(Json[Cursor])) {
                Cursor++;
            }
            string numberString = Json.Substring(start, Cursor - start);
            SkipToNextToken();
            return numberString;
        }

        public string ConsumeString() {
            Cursor++; // Skip the '"'

            // trivial string parsing short-circuit
            for (int readAheadIndex = Cursor; readAheadIndex < Json.Length; ++readAheadIndex) {
                char readAheadChar = Json[readAheadIndex];
                if (readAheadChar == '\\') {
                    // This string isn't trivial, so use the normal expensive parsing.
                    break;
                }

                if (readAheadChar <= 0x1F || readAheadChar == 0x7F ||
                    (readAheadChar >= 0x80 && readAheadChar <= 0x9F)) {
                    throw new InvalidJsonException(
                        $"Disallowed control character in string at column {readAheadIndex}!");
                }

                if (readAheadChar == '"') {
                    int start = Cursor;
                    int length = readAheadIndex - start;
                    Cursor = readAheadIndex + 1; // skip to after the closing "
                    var stringVal = Json.Substring(start, length);
                    SkipToNextToken();
                    return stringVal;
                }
            }

            var stringData = new StringBuilder();
            bool backslash = false;
            for (bool done = false; !done; ++Cursor) {
                if (backslash) {
                    backslash = false;
                    switch (Json[Cursor]) {
                        case '\\': stringData.Append('\\'); break;
                        case '"': stringData.Append('"'); break;
                        case '/': stringData.Append('/'); break;
                        case 'b': stringData.Append('\b'); break;
                        case 'f': stringData.Append('\f'); break;
                        case 'n': stringData.Append('\n'); break;
                        case 'r': stringData.Append('\r'); break;
                        case 't': stringData.Append('\t'); break;
                        case 'u': {
                            // Read 4 hex digits
                            var codePoint = Convert.ToInt16(Json.Substring(Cursor + 1, 4), 16);
                            Cursor += 4;
                            stringData.Append(char.ConvertFromUtf32(codePoint));
                        } break;
                        default:
                            throw new InvalidJsonException(
                                $"Unknown escape character sequence: \\{Json[Cursor]} at column {Cursor}!");
                    }
                } else {
                    switch (Json[Cursor]) {
                        case '\\':
                            backslash = true;
                            break;
                        case '"':
                            done = true;
                            break;
                        default:
                            if (Json[Cursor] <= 0x1F || Json[Cursor] == 0x7F ||
                                (Json[Cursor] >= 0x80 && Json[Cursor] <= 0x9F)) {
                                throw new InvalidJsonException(
                                    $"Disallowed control character in string at column {Cursor}!");
                            }

                            stringData.Append(Json[Cursor]);
                            break;
                    }
                }
            }

            string strinvVal = stringData.ToString();
            SkipToNextToken();
            return strinvVal;
        }
        
        /////////////////////////////////////////////////
        
        void SkipToNextToken() {
            while (Cursor < Json.Length && char.IsWhiteSpace(Json[Cursor])) {
                Cursor++;
            }
            
            int charsLeft = Json.Length - Cursor;

            if (charsLeft <= 0) {
                NextToken = JsonToken.EOF;
                return;
            }

            switch (Json[Cursor]) {
                case '[': NextToken = JsonToken.ArrayStart; return;
                case ']': NextToken = JsonToken.ArrayEnd; return;
                case '{': NextToken = JsonToken.ObjectStart; return;
                case '}': NextToken = JsonToken.ObjectEnd; return;
                case ',': NextToken = JsonToken.Separator; return;
                case '"': NextToken = JsonToken.String; return;
                case ':': NextToken = JsonToken.KeyValueSeparator; return;
            }

            // number
            if (Json[Cursor] == '-' || (Json[Cursor] <= '9' && Json[Cursor] >= '0')) {
                NextToken = JsonToken.Number;
                return;
            }
            
            // true
            const string trueToken = "true";
            if (charsLeft >= 4 && string.CompareOrdinal(Json, Cursor, trueToken, 0, trueToken.Length) == 0) {
                NextToken = JsonToken.True;
                return;
            }

            // false
            const string falseToken = "false";
            if (charsLeft >= 5 && string.CompareOrdinal(Json, Cursor, falseToken, 0, falseToken.Length) == 0) {
                NextToken = JsonToken.False;
                return;
            }

            // null
            const string nullToken = "null";
            if (charsLeft >= 4 && string.CompareOrdinal(Json, Cursor, nullToken, 0, nullToken.Length) == 0) {
                NextToken = JsonToken.Null;
                return;
            }

            throw new InvalidJsonException($"Unexpected character '{Json[Cursor]}' at character {Cursor}!");
        }

        bool isNumberChar(char c) {
            return (c >= '0' && c <= '9') ||
                c == '.' ||
                c == 'e' ||
                c == 'E' ||
                c == '-' ||
                c == '+';
        }
    }
}