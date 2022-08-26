using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TypeInfo = Voorhees.Internal.TypeInfo;

namespace Voorhees {
    public static partial class JsonMapper {
        static object FromJson(JsonTokenReader tokenReader, Type destinationType) {
            var underlyingType = Nullable.GetUnderlyingType(destinationType);
            var valueType = underlyingType ?? destinationType;

            if (tokenReader.NextToken == JsonToken.Null) {
                if (destinationType.IsClass || underlyingType != null) {
                    return null;
                }
                throw new Exception($"Can't assign null to an instance of type {destinationType}");
            }

            // If there's a custom importer that fits, use it
            var config = Voorhees.Instance;
            if (config.CustomImporters.TryGetValue(destinationType, out var customImporter)) {
                return customImporter(tokenReader);
            }

            // Maybe there's a base importer that works
            if (destinationType == typeof(byte)) { return byte.Parse(tokenReader.ConsumeNumber()); }
            if (destinationType == typeof(sbyte)) { return sbyte.Parse(tokenReader.ConsumeNumber()); }
            if (destinationType == typeof(short)) { return short.Parse(tokenReader.ConsumeNumber()); }
            if (destinationType == typeof(ushort)) { return ushort.Parse(tokenReader.ConsumeNumber()); }
            if (destinationType == typeof(int)) { return int.Parse(tokenReader.ConsumeNumber()); }
            if (destinationType == typeof(uint)) { return uint.Parse(tokenReader.ConsumeNumber()); }
            if (destinationType == typeof(long)) { return long.Parse(tokenReader.ConsumeNumber()); }
            if (destinationType == typeof(ulong)) { return ulong.Parse(tokenReader.ConsumeNumber()); }
            
            if (destinationType == typeof(float)) { return float.Parse(tokenReader.ConsumeNumber()); }
            if (destinationType == typeof(double)) { return double.Parse(tokenReader.ConsumeNumber()); }
            if (destinationType == typeof(decimal)) { return decimal.Parse(tokenReader.ConsumeNumber()); }

            if (destinationType == typeof(char)) {
                string stringVal = tokenReader.ConsumeString();
                if (stringVal.Length != 1) {
                    throw new FormatException($"{tokenReader.LineColString} Trying to map a string of length != 1 to a char: \"{stringVal}\"");
                }
                return stringVal[0];
            }
            
            if (Voorhees.BuiltInImporters.TryGetValue(destinationType, out var builtInImporter)) {
                return builtInImporter(tokenReader);
            }

            Type jsonType;
            object jsonValue;

            switch (tokenReader.NextToken) {
                case JsonToken.Null:
                case JsonToken.ObjectStart: return MapObject(tokenReader, destinationType);
                case JsonToken.ArrayStart: return MapArray(tokenReader, destinationType);
                case JsonToken.String:
                    jsonType = typeof(string);
                    jsonValue = tokenReader.ConsumeString();
                    break;
                case JsonToken.Number: {
                    var numberString = tokenReader.ConsumeNumber();
                    if (int.TryParse(numberString, out int intVal)) {
                        jsonType = typeof(int);
                        jsonValue = intVal; // TODO Boxing
                    } else {
                        jsonType = typeof(float);
                        jsonValue = float.Parse(numberString); // TODO Boxing
                    }
                } break;
                case JsonToken.True:
                    jsonType = typeof(bool);
                    jsonValue = true; // TODO Boxing
                    tokenReader.SkipToken(JsonToken.True);
                    break;
                case JsonToken.False:
                    jsonType = typeof(bool);
                    jsonValue = false; // TODO Boxing
                    tokenReader.SkipToken(JsonToken.False);
                    break;
                case JsonToken.EOF:
                case JsonToken.None:
                case JsonToken.ArrayEnd:
                case JsonToken.KeyValueSeparator:
                case JsonToken.ObjectEnd:
                case JsonToken.Separator:
                default: throw new InvalidJsonException($"{tokenReader.LineColString} Unexpected token {tokenReader.NextToken}");
            }

            return MapValueToType(jsonValue, jsonType, valueType, destinationType);
        }

        static void ReadList(JsonTokenReader tokenReader, IList list, Type elementType) {
            tokenReader.SkipToken(JsonToken.ArrayStart);

            bool expectingValue = false;
            while (tokenReader.NextToken != JsonToken.ArrayEnd) {
                expectingValue = false;
                list.Add(FromJson(tokenReader, elementType));
                if (tokenReader.NextToken == JsonToken.Separator) {
                    expectingValue = true;
                    tokenReader.SkipToken(JsonToken.Separator);
                } else if (tokenReader.NextToken != JsonToken.ArrayEnd) {
                    throw new InvalidJsonException($"{tokenReader.LineColString} Expected end array token or separator");
                }
            }

            if (expectingValue) {
                throw new InvalidJsonException($"{tokenReader.LineColString} Unexpected end array token");
            }

            tokenReader.SkipToken(JsonToken.ArrayEnd);
        }

        static IList ReadMultiList(JsonTokenReader tokenReader, Type elementType, int rank) {
            tokenReader.SkipToken(JsonToken.ArrayStart);

            IList list = new ArrayList();

            bool expectingValue = false;
            while (tokenReader.NextToken != JsonToken.ArrayEnd) {
                expectingValue = false;

                list.Add(rank > 1 ? ReadMultiList(tokenReader, elementType, rank - 1) : FromJson(tokenReader, elementType));

                if (tokenReader.NextToken == JsonToken.Separator) {
                    expectingValue = true;
                    tokenReader.SkipToken(JsonToken.Separator);
                } else if (tokenReader.NextToken != JsonToken.ArrayEnd) {
                    throw new InvalidJsonException($"{tokenReader.LineColString} Expected end array token or separator");
                }
            }

            if (expectingValue) {
                throw new InvalidJsonException($"{tokenReader.LineColString} Unexpected end array token");
            }

            tokenReader.SkipToken(JsonToken.ArrayEnd);

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

        static object MapArray(JsonTokenReader tokenReader, Type destinationType) {
            var arrayMetadata = TypeInfo.GetCachedArrayMetadata(destinationType);

            if (arrayMetadata.IsArray) {
                int rank = arrayMetadata.ArrayRank;
                var elementType = destinationType.GetElementType();

                if (elementType == null) {
                    throw new InvalidOperationException("Attempting to map an array but the array element type is null");
                }

                if (rank == 1) { // Simple array
                    var tempValues = new List<object>();
                    ReadList(tokenReader, tempValues, elementType);

                    var result = Array.CreateInstance(elementType, tempValues.Count);
                    for (int i = 0; i < tempValues.Count; i++) {
                        result.SetValue(tempValues[i], i);
                    }
                    return result;
                } else {
                    var list = ReadMultiList(tokenReader, elementType, rank);

                    // Compute rank
                    int[] lengths = new int[rank]; // TODO Temp alloc
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
                ReadList(tokenReader, list, arrayMetadata.ElementType);
                return list;
            }

            throw new Exception($"Type {destinationType} can't act as an array");
        }

        static object MapObject(JsonTokenReader tokenReader, Type destinationType) {
            var valueType = destinationType;
            tokenReader.SkipToken(JsonToken.ObjectStart);

            string firstKey = null;
            if (tokenReader.NextToken == JsonToken.String) {
                firstKey = tokenReader.ConsumeString();
                if (firstKey == "$t") { // Type specifier
                    firstKey = null;

                    tokenReader.SkipToken(JsonToken.KeyValueSeparator);

                    string valueAssemblyQualifiedName = tokenReader.ConsumeString();
                    valueType = Type.GetType(valueAssemblyQualifiedName);
                    if (valueType == null) {
                        throw new InvalidJsonException($"Can't deserialize value of type {valueAssemblyQualifiedName} because a type with that " +
                                                       "assembly qualified name name cannot be found.");
                    }

                    if (tokenReader.NextToken == JsonToken.Separator) {
                        tokenReader.SkipToken(JsonToken.Separator);
                    }
                }
            }

            var objectMetadata = TypeInfo.GetObjectMetadata(valueType);
            object instance = Activator.CreateInstance(valueType);

            bool expectingValue = false;

            while (tokenReader.NextToken != JsonToken.ObjectEnd) {
                expectingValue = false;
                string propertyName;
                if (firstKey != null) {
                    propertyName = firstKey;
                    firstKey = null;
                } else {
                    propertyName = tokenReader.ConsumeString();
                }

                tokenReader.SkipToken(JsonToken.KeyValueSeparator);

                if (objectMetadata.Properties.TryGetValue(propertyName, out var propertyMetadata)) {
                    if (propertyMetadata.Ignored) {
                        // Read the value so we advance the token reader, but don't do anything with it.
                        FromJson(tokenReader, propertyMetadata.Type);
                    } else {
                        if (propertyMetadata.IsField) {
                            ((FieldInfo)propertyMetadata.Info).SetValue(instance, FromJson(tokenReader, propertyMetadata.Type));
                        } else {
                            var propertyInfo = (PropertyInfo)propertyMetadata.Info;
                            if (propertyInfo.CanWrite) {
                                propertyInfo.SetValue(instance, FromJson(tokenReader, propertyMetadata.Type), null);
                            } else {
                                throw new Exception("Read property value from json but the property " +
                                                    $"{propertyInfo.Name} in type {destinationType} is read-only.");
                            }
                        }
                    }
                } else if (objectMetadata.IsDictionary) {
                    ((IDictionary)instance).Add(propertyName, FromJson(tokenReader, objectMetadata.ElementType));
                } else {
                    throw new Exception($"The type {destinationType} doesn't have the property '{propertyName}'");
                }

                if (tokenReader.NextToken == JsonToken.Separator) {
                    expectingValue = true;
                    tokenReader.SkipToken(JsonToken.Separator);
                }
            }

            if (expectingValue) {
                throw new InvalidJsonException($"{tokenReader.LineColString} Unexpected object end token");
            }

            tokenReader.SkipToken(JsonToken.ObjectEnd);

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

        static JsonValue ReadJsonValue(JsonTokenReader tokenReader) {
            switch (tokenReader.NextToken) {
                case JsonToken.ArrayStart: return ReadJsonValueArray(tokenReader);
                case JsonToken.ArrayEnd: break;
                case JsonToken.ObjectStart: return ReadJsonValueObject(tokenReader);
                case JsonToken.ObjectEnd: break;
                case JsonToken.Separator: break;
                case JsonToken.String: return new JsonValue(tokenReader.ConsumeString());
                case JsonToken.Number: {
                    var numberSpan = tokenReader.ConsumeNumber();
                    try {
                        return int.TryParse(numberSpan, out int intVal) ? new JsonValue(intVal)
                            : new JsonValue(float.Parse(numberSpan));
                    } catch (FormatException) {
                        // TODO this line/col number is wrong.  It points to after the number token that we failed to parse.
                        throw new InvalidJsonException($"{tokenReader.LineColString} Can't parse text \"{new string(numberSpan)}\" as a number.");
                    }
                }
                case JsonToken.True:
                    tokenReader.SkipToken(JsonToken.True);
                    return new JsonValue(true);
                case JsonToken.False:
                    tokenReader.SkipToken(JsonToken.False);
                    return new JsonValue(false);
                case JsonToken.Null:
                    tokenReader.SkipToken(JsonToken.Null);
                    return new JsonValue(null);
                case JsonToken.EOF:
                    throw new InvalidJsonException($"{tokenReader.LineColString} Unexpected end of file");
                case JsonToken.KeyValueSeparator:
                case JsonToken.None:
                default: break;
            }
            throw new InvalidJsonException($"{tokenReader.LineColString} Unexpected token {tokenReader.NextToken}");
        }

        static JsonValue ReadJsonValueArray(JsonTokenReader tokenReader) {
            var arrayValue = new JsonValue(JsonType.Array);

            tokenReader.SkipToken(JsonToken.ArrayStart);

            bool expectingValue = false;

            while (tokenReader.NextToken != JsonToken.ArrayEnd) {
                expectingValue = false;
                arrayValue.Add(ReadJsonValue(tokenReader));
                if (tokenReader.NextToken == JsonToken.Separator) {
                    expectingValue = true;
                    tokenReader.SkipToken(JsonToken.Separator);
                } else if (tokenReader.NextToken != JsonToken.ArrayEnd) {
                    throw new InvalidJsonException($"{tokenReader.LineColString} Expected end array token or separator");
                }
            }

            if (expectingValue) {
                throw new InvalidJsonException($"{tokenReader.LineColString} Unexpected end array token");
            }

            tokenReader.SkipToken(JsonToken.ArrayEnd);

            return arrayValue;
        }

        static JsonValue ReadJsonValueObject(JsonTokenReader tokenReader) {
            var result = new JsonValue(JsonType.Object);

            tokenReader.SkipToken(JsonToken.ObjectStart);

            bool expectingValue = false;
            while (tokenReader.NextToken != JsonToken.ObjectEnd) {
                expectingValue = false;
                string key = tokenReader.ConsumeString();

                // Edge case: If the dictionary already contains the key, for example in the case where
                // the json we're reading has duplicate keys in an object, arbitrarily prefer the later 
                // key value pair that appears in the file.
                if (result.ContainsKey(key)) {
                    result.Remove(key);
                }

                if (tokenReader.NextToken != JsonToken.KeyValueSeparator) {
                    throw new InvalidJsonException($"{tokenReader.LineColString} Expected ':'");
                }
                tokenReader.SkipToken(JsonToken.KeyValueSeparator);

                result.Add(key, ReadJsonValue(tokenReader));

                if (tokenReader.NextToken == JsonToken.Separator) {
                    expectingValue = true;
                    tokenReader.SkipToken(JsonToken.Separator);
                } else if (tokenReader.NextToken != JsonToken.ObjectEnd) {
                    throw new InvalidJsonException($"{tokenReader.LineColString} Unexpected token {tokenReader.NextToken}");
                }
            }

            if (expectingValue) {
                throw new InvalidJsonException($"{tokenReader.LineColString} Unexpected object end token");
            }

            tokenReader.SkipToken(JsonToken.ObjectEnd);

            return result;
        }
    }
}