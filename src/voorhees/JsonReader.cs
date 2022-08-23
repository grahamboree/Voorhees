namespace Voorhees {
   /// Reads JSON into JsonValue's
   public static class JsonReader {
      /// <summary>
      /// Reads a JSON string and generates a matching JsonValue structure
      /// </summary>
      /// <param name="json">The json string</param>
      /// <returns>A JsonValue object that matches the json data</returns>
      /// <exception cref="InvalidJsonException">If the input JSON has invalid JSON syntax or characters.</exception>
      public static JsonValue Read(string json) {
         var tokenizer = new JsonTokenizer(json);
         return Read(tokenizer);
      }

      public static JsonValue Read(JsonTokenizer tokenizer) {
         var result = ReadJsonValue(tokenizer);

         // Make sure there's no additional json in the buffer.
         if (tokenizer.NextToken != JsonToken.EOF) {
            throw new InvalidJsonException($"{tokenizer.LineColString} Expected end of file");
         }
         return result;
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
               try {
                  return int.TryParse(numberString, out int intVal) ? new JsonValue(intVal)
                     : new JsonValue(float.Parse(numberString));
               } catch (System.FormatException) {
                  // TODO this line/col number is wrong.  It points to after the number token that we failed to parse.
                  throw new InvalidJsonException($"{tokenizer.LineColString} Can't parse text \"{new string(numberString)}\" as a number.");
               }
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
            case JsonToken.EOF:
               throw new InvalidJsonException($"{tokenizer.LineColString} Unexpected end of file");
            case JsonToken.KeyValueSeparator:
            case JsonToken.None:
            default: break;
         }
         throw new InvalidJsonException($"{tokenizer.LineColString} Unexpected token {tokenizer.NextToken}");
      }

      static JsonValue ReadArray(JsonTokenizer tokenizer) {
         var arrayValue = new JsonValue(JsonType.Array);
         
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
         var result = new JsonValue(JsonType.Object);
         
         tokenizer.SkipToken(JsonToken.ObjectStart);

         bool expectingValue = false;
         while (tokenizer.NextToken != JsonToken.ObjectEnd) {
            expectingValue = false;
            string key = tokenizer.ConsumeString();

            // Edge case: If the dictionary already contains the key, for example in the case where
            // the json we're reading has duplicate keys in an object, arbitrarily prefer the later 
            // key value pair that appears in the file.
            if (result.ContainsKey(key)) {
               result.Remove(key);
            }

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
