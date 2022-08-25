using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TypeInfo = Voorhees.Internal.TypeInfo;

namespace Voorhees {
    public static partial class JsonMapper {
        static object FromJson(JsonTokenizer tokenizer, Type destinationType) {
            var underlyingType = Nullable.GetUnderlyingType(destinationType);
            var valueType = underlyingType ?? destinationType;

            if (tokenizer.NextToken == JsonToken.Null) {
                if (destinationType.IsClass || underlyingType != null) {
                    return null;
                }
                throw new Exception($"Can't assign null to an instance of type {destinationType}");
            }

            // If there's a custom importer that fits, use it
            var config = Voorhees.Instance;
            if (config.CustomImporters.TryGetValue(destinationType, out var customImporter)) {
                return customImporter(tokenizer);
            }

            // Maybe there's a base importer that works
            if (Voorhees.BuiltInImporters.TryGetValue(destinationType, out var builtInImporter)) {
                return builtInImporter(tokenizer);
            }

            Type jsonType;
            object jsonValue;

            switch (tokenizer.NextToken) {
                case JsonToken.Null:
                case JsonToken.ObjectStart: return MapObject(tokenizer, destinationType);
                case JsonToken.ArrayStart: return MapArray(tokenizer, destinationType);
                case JsonToken.String:
                    jsonType = typeof(string);
                    jsonValue = tokenizer.ConsumeString();
                    break;
                case JsonToken.Number: {
                    var numberString = tokenizer.ConsumeNumber();
                    if (int.TryParse(numberString, out int intVal)) {
                        jsonType = typeof(int);
                        jsonValue = intVal;
                    } else {
                        jsonType = typeof(float);
                        jsonValue = float.Parse(numberString);
                    }
                }
                    break;
                case JsonToken.True:
                    jsonType = typeof(bool);
                    jsonValue = true;
                    tokenizer.SkipToken(JsonToken.True);
                    break;
                case JsonToken.False:
                    jsonType = typeof(bool);
                    jsonValue = false;
                    tokenizer.SkipToken(JsonToken.False);
                    break;
                case JsonToken.EOF:
                case JsonToken.None:
                case JsonToken.ArrayEnd:
                case JsonToken.KeyValueSeparator:
                case JsonToken.ObjectEnd:
                case JsonToken.Separator:
                default: throw new InvalidJsonException($"{tokenizer.LineColString} Unexpected token {tokenizer.NextToken}");
            }

            return MapValueToType(jsonValue, jsonType, valueType, destinationType);
        }

        static void ReadList(JsonTokenizer tokenizer, IList list, Type elementType) {
            tokenizer.SkipToken(JsonToken.ArrayStart);

            bool expectingValue = false;
            while (tokenizer.NextToken != JsonToken.ArrayEnd) {
                expectingValue = false;
                list.Add(FromJson(tokenizer, elementType));
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
        }

        static IList ReadMultiList(JsonTokenizer tokenizer, Type elementType, int rank) {
            tokenizer.SkipToken(JsonToken.ArrayStart);

            IList list = new ArrayList();

            bool expectingValue = false;
            while (tokenizer.NextToken != JsonToken.ArrayEnd) {
                expectingValue = false;

                list.Add(rank > 1 ? ReadMultiList(tokenizer, elementType, rank - 1) : FromJson(tokenizer, elementType));

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

            return list;
        }

        static void CopyArray(int[] currentIndex, int currentDimension, int rank, Array result, IList list, Type elementType) {
            for (int i = 0; i < list.Count; ++i) {
                currentIndex[currentDimension] = i;
                if (currentDimension == rank - 1) {
                    result.SetValue(Convert.ChangeType(list[i], elementType), currentIndex);
                } else {
                    CopyArray(currentIndex, currentDimension + 1, rank, result, (IList)list[i], elementType);
                }
            }
        }

        static object MapArray(JsonTokenizer tokenizer, Type destinationType) {
            var arrayMetadata = TypeInfo.GetCachedArrayMetadata(destinationType);

            if (arrayMetadata.IsArray) {
                int rank = arrayMetadata.ArrayRank;
                var elementType = destinationType.GetElementType();

                if (elementType == null) {
                    throw new InvalidOperationException("Attempting to map an array but the array element type is null");
                }

                if (rank == 1) { // Simple array
                    var tempValues = new List<object>();
                    ReadList(tokenizer, tempValues, elementType);

                    var result = Array.CreateInstance(elementType, tempValues.Count);
                    for (int i = 0; i < tempValues.Count; i++) {
                        result.SetValue(tempValues[i], i);
                    }
                    return result;
                } else {
                    var list = ReadMultiList(tokenizer, elementType, rank);

                    // Compute rank
                    var lengths = new int[rank];
                    var curList = list;
                    for (int i = 0; i < rank; ++i) {
                        if (curList == null) {
                            lengths[i] = 0;
                        } else {
                            lengths[i] = curList.Count;
                            curList = (curList.Count != 0 && i < rank - 1) ? (IList)curList[0] : null;
                        }
                    }

                    // Create the instance and copy the data.
                    var result = Array.CreateInstance(elementType, lengths);
                    CopyArray(new int[lengths.Length], 0, rank, result, list, elementType);
                    return result;
                }
            }

            if (arrayMetadata.IsList) {
                var list = (IList)Activator.CreateInstance(destinationType);
                ReadList(tokenizer, list, arrayMetadata.ElementType);
                return list;
            }

            throw new Exception($"Type {destinationType} can't act as an array");
        }

        static object MapObject(JsonTokenizer tokenizer, Type destinationType) {
            var valueType = destinationType;
            tokenizer.SkipToken(JsonToken.ObjectStart);

            string firstKey = null;
            if (tokenizer.NextToken == JsonToken.String) {
                firstKey = tokenizer.ConsumeString();
                if (firstKey == "$t") { // Type specifier
                    firstKey = null;

                    tokenizer.SkipToken(JsonToken.KeyValueSeparator);

                    string valueAssemblyQualifiedName = tokenizer.ConsumeString();
                    valueType = Type.GetType(valueAssemblyQualifiedName);
                    if (valueType == null) {
                        throw new InvalidJsonException($"Can't deserialize value of type {valueAssemblyQualifiedName} because a type with that " +
                                                       "assembly qualified name name cannot be found.");
                    }

                    if (tokenizer.NextToken == JsonToken.Separator) {
                        tokenizer.SkipToken(JsonToken.Separator);
                    }
                }
            }

            var objectMetadata = TypeInfo.GetObjectMetadata(valueType);
            object instance = Activator.CreateInstance(valueType);

            bool expectingValue = false;

            while (tokenizer.NextToken != JsonToken.ObjectEnd) {
                expectingValue = false;
                string propertyName;
                if (firstKey != null) {
                    propertyName = firstKey;
                    firstKey = null;
                } else {
                    propertyName = tokenizer.ConsumeString();
                }

                tokenizer.SkipToken(JsonToken.KeyValueSeparator);

                if (objectMetadata.Properties.TryGetValue(propertyName, out var propertyMetadata)) {
                    if (propertyMetadata.Ignored) {
                        // Read the value so we advance the tokenizer, but don't do anything with it.
                        FromJson(tokenizer, propertyMetadata.Type);
                    } else {
                        if (propertyMetadata.IsField) {
                            ((FieldInfo)propertyMetadata.Info).SetValue(instance, FromJson(tokenizer, propertyMetadata.Type));
                        } else {
                            var propertyInfo = (PropertyInfo)propertyMetadata.Info;
                            if (propertyInfo.CanWrite) {
                                propertyInfo.SetValue(instance, FromJson(tokenizer, propertyMetadata.Type), null);
                            } else {
                                throw new Exception("Read property value from json but the property " +
                                                    $"{propertyInfo.Name} in type {destinationType} is read-only.");
                            }
                        }
                    }
                } else if (objectMetadata.IsDictionary) {
                    ((IDictionary)instance).Add(propertyName, FromJson(tokenizer, objectMetadata.ElementType));
                } else {
                    throw new Exception($"The type {destinationType} doesn't have the property '{propertyName}'");
                }

                if (tokenizer.NextToken == JsonToken.Separator) {
                    expectingValue = true;
                    tokenizer.SkipToken(JsonToken.Separator);
                }
            }

            if (expectingValue) {
                throw new InvalidJsonException($"{tokenizer.LineColString} Unexpected object end token");
            }

            tokenizer.SkipToken(JsonToken.ObjectEnd);

            return instance;
        }

        /// <summary>
        /// Converts a basic json value to an object of the specified type.
        /// </summary>
        /// <param name="jsonValue">The json value</param>
        /// <param name="jsonType">The type of the json value (int, float, string, etc.)</param>
        /// <param name="valueType">The underlying value's type. e.g. an instance of a derived class.</param>
        /// <param name="destinationType">The destination type.  e.g. a reference to a base class.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        static object MapValueToType(object jsonValue, Type jsonType, Type valueType, Type destinationType) {
            if (valueType.IsAssignableFrom(jsonType)) {
                return Convert.ChangeType(Convert.ChangeType(jsonValue, valueType), destinationType);
            }

            // Integral value can be converted to enum values
            if (jsonType == typeof(int) && valueType.IsEnum) {
                return Convert.ChangeType(Enum.ToObject(valueType, jsonValue), destinationType);
            }

            // Try using an implicit conversion operator
            var implicitConversionOperator = TypeInfo.GetImplicitConversionOperator(valueType, jsonType);
            if (implicitConversionOperator != null) {
                return implicitConversionOperator.Invoke(null, new[] { jsonValue });
            }

            // No luck
            throw new Exception($"Can't assign value of type '{jsonType}' to value type {valueType} and destination type {destinationType}");
        }
        
      static JsonValue ReadJsonValue(JsonTokenizer tokenizer) {
         switch (tokenizer.NextToken) {
            case JsonToken.ArrayStart: return ReadJsonValueArray(tokenizer);
            case JsonToken.ArrayEnd: break;
            case JsonToken.ObjectStart: return ReadJsonValueObject(tokenizer);
            case JsonToken.ObjectEnd: break;
            case JsonToken.Separator: break;
            case JsonToken.String: return new JsonValue(tokenizer.ConsumeString());
            case JsonToken.Number: {
               var numberString = tokenizer.ConsumeNumber();
               try {
                  return int.TryParse(numberString, out int intVal) ? new JsonValue(intVal)
                     : new JsonValue(float.Parse(numberString));
               } catch (FormatException) {
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

      static JsonValue ReadJsonValueArray(JsonTokenizer tokenizer) {
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

      static JsonValue ReadJsonValueObject(JsonTokenizer tokenizer) {
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