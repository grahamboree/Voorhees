using System;

namespace Voorhees {
   /// Static class that handles reading and parsing JSON
   public static class JsonReader {
      /// <summary>
      /// Reads a JSON string and generates a matching JsonValue structure
      /// </summary>
      /// <param name="json">The json string</param>
      /// <returns>A JsonValue object that matches the json data</returns>
      /// <exception cref="InvalidJsonException">If the input JSON has invalid JSON syntax or characters.</exception>
      public static JsonValue Read(string json) {
         try {
            var tokenizer = new JsonTokenizer(json);
            var result = ReadJsonValue(tokenizer);

            // Make sure there's no additional json in the buffer.
            if (tokenizer.NextToken != JsonToken.EOF) {
               throw new InvalidJsonException($"{tokenizer.LineColString} Expected end of file");
            }
            
            return result;
         } catch (IndexOutOfRangeException) {
            throw new InvalidJsonException("Unexpected end of file!");
         }
      }
      
      /////////////////////////////////////////////////
      
      internal static JsonValue ReadJsonValue(JsonTokenizer tokenizer) {
         switch (tokenizer.NextToken) {
            case JsonToken.ArrayStart: return ReadArray(tokenizer);
            case JsonToken.ArrayEnd: break;
            case JsonToken.ObjectStart: return ReadObject(tokenizer);
            case JsonToken.ObjectEnd: break;
            case JsonToken.Separator: break;
            case JsonToken.String: return new JsonValue(tokenizer.ConsumeString());
            case JsonToken.Number: {
               var numberString = tokenizer.ConsumeNumber();
               return int.TryParse(numberString, out int intVal) ? new JsonValue(intVal)
                  : new JsonValue(float.Parse(numberString));
            }
            case JsonToken.True:
               tokenizer.SkipToken(JsonToken.True);
               return new JsonValue(true);
            case JsonToken.False:
               tokenizer.SkipToken(JsonToken.False);
               return new JsonValue(false);
            case JsonToken.Null:
               tokenizer.SkipToken(JsonToken.Null);
               return new JsonValue(null);
            case JsonToken.None:
            case JsonToken.EOF:
               throw new InvalidJsonException($"{tokenizer.LineColString} Unexpected end of file");
            default:
               throw new ArgumentOutOfRangeException($"Unknown json token {tokenizer.NextToken}");
         }

         throw new InvalidJsonException($"{tokenizer.LineColString} Unexpected character '{tokenizer.JsonData[tokenizer.Cursor]}'");
      }

      static JsonValue ReadArray(JsonTokenizer tokenizer) {
         var arrayValue = new JsonValue {Type = JsonType.Array};
         
         tokenizer.SkipToken(JsonToken.ArrayStart);

         bool expectingValue = false;
         
         while (tokenizer.NextToken != JsonToken.ArrayEnd) {
            expectingValue = false;
            arrayValue.Add(ReadJsonValue(tokenizer));
            if (tokenizer.NextToken == JsonToken.Separator) {
               expectingValue = true;
               tokenizer.SkipToken(JsonToken.Separator);
            } else if (tokenizer.NextToken != JsonToken.ArrayEnd) {
               throw new InvalidJsonException($"{tokenizer.LineColString} Expected end array token or separator");
            }
         }

         if (expectingValue) {
            throw new InvalidJsonException($"{tokenizer.LineColString} Unexpected end array token");
         }

         tokenizer.SkipToken(JsonToken.ArrayEnd);

         return arrayValue;
      }

      static JsonValue ReadObject(JsonTokenizer tokenizer) {
         var result = new JsonValue { Type = JsonType.Object };
         
         tokenizer.SkipToken(JsonToken.ObjectStart);

         bool expectingValue = false;
         while (tokenizer.NextToken != JsonToken.ObjectEnd) {
            expectingValue = false;
            string key = tokenizer.ConsumeString();
            
            if (tokenizer.NextToken != JsonToken.KeyValueSeparator) {
               throw new InvalidJsonException($"{tokenizer.LineColString} Expected ':'");
            }
            tokenizer.SkipToken(JsonToken.KeyValueSeparator);

            result.Add(key, ReadJsonValue(tokenizer));

            if (tokenizer.NextToken == JsonToken.Separator) {
               expectingValue = true;
               tokenizer.SkipToken(JsonToken.Separator);
            } else if (tokenizer.NextToken != JsonToken.ObjectEnd) {
               throw new InvalidJsonException($"{tokenizer.LineColString} Unexpected token {tokenizer.NextToken}");
            }
         }

         if (expectingValue) {
            throw new InvalidJsonException($"{tokenizer.LineColString} Unexpected object end token");
         }
         
         tokenizer.SkipToken(JsonToken.ObjectEnd);

         return result;
      }
   }
}
