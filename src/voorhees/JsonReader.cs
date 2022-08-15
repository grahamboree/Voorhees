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
               throw new InvalidJsonException($"Expected end of file at character {tokenizer.Cursor}!");
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
               tokenizer.ConsumeToken();
               return new JsonValue(true);
            case JsonToken.False:
               tokenizer.ConsumeToken();
               return new JsonValue(false);
            case JsonToken.Null:
               tokenizer.ConsumeToken();
               return new JsonValue(null);
            case JsonToken.None:
            case JsonToken.EOF:
               throw new InvalidJsonException($"Unexpected end of file at character {tokenizer.Cursor}");
            default:
               throw new ArgumentOutOfRangeException($"Unknown json token {tokenizer.NextToken}");
         }

         throw new InvalidJsonException($"Unexpected character '{tokenizer.JsonData[tokenizer.Cursor]}' at column {tokenizer.Cursor}!");
      }

      static JsonValue ReadArray(JsonTokenizer tokenizer) {
         var arrayValue = new JsonValue {Type = JsonType.Array};
         
         tokenizer.ConsumeToken(); // [

         bool expectingValue = false;
         
         while (tokenizer.NextToken != JsonToken.ArrayEnd) {
            expectingValue = false;
            arrayValue.Add(ReadJsonValue(tokenizer));
            if (tokenizer.NextToken == JsonToken.Separator) {
               expectingValue = true;
               tokenizer.ConsumeToken(); // ,
            } else if (tokenizer.NextToken != JsonToken.ArrayEnd) {
               throw new InvalidJsonException($"Expected end array token or separator at column {tokenizer.Cursor}!");
            }
         }

         if (expectingValue) {
            throw new InvalidJsonException($"Unexpected end array token at column {tokenizer.Cursor}!");
         }

         tokenizer.ConsumeToken(); // ]
         
         return arrayValue;
      }

      static JsonValue ReadObject(JsonTokenizer tokenizer) {
         var result = new JsonValue { Type = JsonType.Object };
         
         tokenizer.ConsumeToken(); // {
      
         bool expectingValue = false;
         while (tokenizer.NextToken != JsonToken.ObjectEnd) {
            expectingValue = false;
            string key = tokenizer.ConsumeString();
            
            if (tokenizer.NextToken != JsonToken.KeyValueSeparator) {
               throw new InvalidJsonException($"Expected ':' at character {tokenizer.Cursor}!");
            }
            tokenizer.ConsumeToken(); // :
            
            result.Add(key, ReadJsonValue(tokenizer));

            if (tokenizer.NextToken == JsonToken.Separator) {
               expectingValue = true;
               tokenizer.ConsumeToken(); // ,
            } else if (tokenizer.NextToken != JsonToken.ObjectEnd) {
               throw new InvalidJsonException($"Unexpected token {tokenizer.NextToken} at character {tokenizer.Cursor}!");
            }
         }

         if (expectingValue) {
            throw new InvalidJsonException($"Unexpected end object token at column {tokenizer.Cursor}!");
         }
         
         tokenizer.ConsumeToken(); // }
         
         return result;
      }
   }
}
