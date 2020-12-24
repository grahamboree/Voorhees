using System;
using System.Text;

namespace Voorhees {
   public class InvalidJsonException : Exception {
      public InvalidJsonException(string message) : base(message) {
      }
   }

   public static class JsonReader {
      public static JsonValue Read(string json) {
         JsonValue result;
         try {
            // Read the json.
            int readIndex = 0;
            result = ReadValue(json, ref readIndex);

            // Make sure there's no additional json in the buffer.
            SkipWhitespace(json, ref readIndex);
            if (readIndex <= json.Length - 1) {
               throw new InvalidJsonException($"Expected end of file at column {readIndex}!");
            }
         } catch (IndexOutOfRangeException) {
            throw new InvalidJsonException("Unexpected end of file!");
         }
         return result;
      }

      static JsonValue ReadNumber(string json, ref int readIndex) {
         int startIndex = readIndex;
         readIndex++;
         while (readIndex < json.Length &&
               ((json[readIndex] >= '0' && json[readIndex] <= '9') ||
                json[readIndex] == '.' || json[readIndex] == 'e' ||
                json[readIndex] == 'E' || json[readIndex] == '-' ||
                json[readIndex] == '+')) {
            readIndex++;
         }
         string numberString = json.Substring(startIndex, readIndex - startIndex);

         if (int.TryParse(numberString, out int intVal)) {
            return intVal;
         }

         if (float.TryParse(numberString, out float floatVal)) {
            return floatVal;
         }

         throw new InvalidJsonException($"'{numberString}' is not a number");
      }

      static JsonValue ReadString(string json, ref int readIndex) {
         readIndex++; // Skip the '"'

         // trivial string parsing short-circuit
         /*
         for (int readAheadIndex = readIndex; readAheadIndex < json.Length; ++readAheadIndex) {
            if (json[readAheadIndex] == '\\') {
               // This string isn't trivial, so use the normal expensive parsing.
               break;
            }

            if (json[readAheadIndex] == '"') {
               int start = readIndex;
               int length = readAheadIndex - start;
               readIndex = readAheadIndex + 1; // skip to after the "
               return json.Substring(start, length);
            }
         }
         */

         var stringData = new StringBuilder();
         bool backslash = false;
         for (bool done = false; !done; ++readIndex) {
            if (backslash) {
               backslash = false;
               switch (json[readIndex]) {
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
                     var codePoint = Convert.ToInt16(json.Substring(readIndex + 1, 4), 16);
                     readIndex += 4;
                     stringData.Append(char.ConvertFromUtf32(codePoint));
                  } break;
                  default:
                     throw new InvalidJsonException($"Unknown escape character sequence: \\{json[readIndex]} at column {readIndex}!");
               }
            } else {
               switch (json[readIndex]) {
                  case '\\':
                     backslash = true;
                     break;
                  case '"':
                     done = true;
                     break;
                  default:
                     if (json[readIndex] <= 0x1F || json[readIndex] == 0x7F || (json[readIndex] >= 0x80 && json[readIndex] <= 0x9F)) {
                        throw new InvalidJsonException($"Disallowed control character in string at column {readIndex}!");
                     }
                     stringData.Append(json[readIndex]);
                     break;
               }
            }

         }
         return stringData.ToString();
      }

      static JsonValue ReadArray(string json, ref int readIndex) {
         readIndex++; // Skip the '['
         SkipWhitespace(json, ref readIndex);

         var arrayValue = new JsonValue {Type = JsonType.Array};

         bool expectingValue = false;
         while (json[readIndex] != ']') {
            expectingValue = false;
            arrayValue.Add(ReadValue(json, ref readIndex));
            SkipWhitespace(json, ref readIndex);
            if (json[readIndex] == ',') {
               expectingValue = true;
               readIndex++;
               SkipWhitespace(json, ref readIndex);
            } else if (json[readIndex] != ']') {
               throw new InvalidJsonException($"Expected end array token at column {readIndex}!");
            }
         }

         if (expectingValue) {
            throw new InvalidJsonException($"Unexpected end array token at column {readIndex}!");
         }

         readIndex++; // Skip the ']'
         return arrayValue;
      }

      static JsonValue ReadValue(string json, ref int readIndex) {
         SkipWhitespace(json, ref readIndex);

         // array
         if (json[readIndex] == '[') {
            return ReadArray(json, ref readIndex);
         }

         // object
         if (json[readIndex] == '{') {
            return ReadObject(json, ref readIndex);
         }

         // string
         if (json[readIndex] == '"') {
            return ReadString(json, ref readIndex);
         }

         // number
         if (json[readIndex] == '-' || (json[readIndex] <= '9' && json[readIndex] >= '0')) {
            return ReadNumber(json, ref readIndex);
         }

            int charsLeft = json.Length - readIndex;

         // true
         if (charsLeft >= 4 && json.Substring(readIndex, 4) == "true") {
            readIndex += 4;
            return true;
         }

         // false
         if (charsLeft >= 5 && json.Substring(readIndex, 5) == "false") {
            readIndex += 5;
            return false;
         }

         // null
         if (charsLeft >= 4 && json.Substring(readIndex, 4) == "null") {
            readIndex += 4;
            return new JsonValue();
         }

         throw new InvalidJsonException($"Unexpected character '{json[readIndex]}' at column {readIndex}!");
      }

      static JsonValue ReadObject(string json, ref int readIndex) {
         var result = new JsonValue { Type = JsonType.Object };

         readIndex++; // Skip the '{'
         SkipWhitespace(json, ref readIndex);
         if (json[readIndex] != '}') {
            while (true) {
               SkipWhitespace(json, ref readIndex);
               var key = ReadString(json, ref readIndex);
               SkipWhitespace(json, ref readIndex);
               if (json[readIndex] != ':') {
                  throw new InvalidJsonException($"Expected ':' at column {readIndex }!");
               }
               ++readIndex; // Skip the ':'
               SkipWhitespace(json, ref readIndex);
               var value = ReadValue(json, ref readIndex);
               result.Add((string)key, value);

               SkipWhitespace(json, ref readIndex);

               if (json[readIndex] == ',') {
                  readIndex++; // Skip the ','
               } else {
                  break;
               }
            }
         }

         if (json[readIndex] != '}') {
            throw new InvalidJsonException($"Expected closing object token at column {readIndex}!");
         }

         readIndex++; // Skip the '}'

         return result;
      }

      static void SkipWhitespace(string json, ref int readIndex) {
         while (readIndex < json.Length && char.IsWhiteSpace(json[readIndex])) {
            readIndex++;
         }
      }
   }
}
