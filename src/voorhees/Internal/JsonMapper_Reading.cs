using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using TypeInfo = Voorhees.Internal.TypeInfo;

namespace Voorhees {
    public partial class JsonMapper {
        T ReadValueOfType<T>(JsonTokenReader tokenReader) {
            var destinationType = typeof(T);

            if (destinationType == typeof(JsonValue)) {
                // All this casting does nothing but is necessary to make the type checker happy.
                return (T)Convert.ChangeType(ReadJsonValue(tokenReader), destinationType);
            }
            
            // If there's a custom importer that fits, use it
            if (importers.TryGetValue(destinationType, out var customImporter)) {
                return (T)customImporter(tokenReader);
            }

            try {
                var numericParser = Internal.NumericValueParsers.Get<T>();
                if (numericParser != null) {
                    return numericParser.ConvertFrom(tokenReader.ConsumeNumber());
                }
                
                var stringParser = Internal.StringValueParsers.Get<T>();
                if (stringParser != null) {
                    return stringParser.Parse(tokenReader.ConsumeString());
                }
            } catch (InvalidJsonException inner) {
                // Need to re-throw these because the value parsers don't have access to the line & column info
                throw new InvalidJsonException(tokenReader.LineColString + " " + inner.Message);
            }
            
            return (T)ReadValueOfType(tokenReader, destinationType);
        }
        
        object ReadValueOfType(JsonTokenReader tokenReader, Type destinationType) {
            var underlyingType = Nullable.GetUnderlyingType(destinationType);
            var valueType = underlyingType ?? destinationType;
            Type jsonType;
            object jsonValue;
            
            switch (tokenReader.NextToken) {
                case JsonToken.Null: {
                    if (destinationType.IsClass || underlyingType != null) {
                        tokenReader.SkipToken(JsonToken.Null);
                        return null;
                    }
                    throw new InvalidCastException($"{tokenReader} Can't assign null to an instance of type {destinationType}");
                }
                case JsonToken.ObjectStart: return MapObject(tokenReader, destinationType);
                case JsonToken.ArrayStart: return MapArray(tokenReader, destinationType);
                case JsonToken.String: {
                    jsonType = typeof(string);
                    jsonValue = tokenReader.ConsumeString();
                } break;
                case JsonToken.Number: {
                    // All numbers in json are represented as doubles.
                    double numberValue = tokenReader.ConsumeNumber();
                    if (numberValue == (int)numberValue) {
                        jsonType = typeof(int);
                        jsonValue = (int)numberValue; // TODO Boxing

                        // Integral value can be converted to enum values
                        if (valueType.IsEnum) {
                            return Convert.ChangeType(Enum.ToObject(valueType, jsonValue), destinationType);
                        }
                    } else {
                        jsonType = typeof(double);
                        jsonValue = numberValue; // TODO Boxing

                        // Double values can be converted to floats
                        if (valueType == typeof(float)) {
                            return Convert.ToSingle(jsonValue); // TODO boxing
                        }
                    }
                } break;
                case JsonToken.True: {
                    jsonType = typeof(bool);
                    jsonValue = true; // TODO Boxing
                    tokenReader.SkipToken(JsonToken.True);
                } break;
                case JsonToken.False: {
                    jsonType = typeof(bool);
                    jsonValue = false; // TODO Boxing
                    tokenReader.SkipToken(JsonToken.False);
                } break;
                case JsonToken.EOF:
                case JsonToken.None:
                case JsonToken.ArrayEnd:
                case JsonToken.KeyValueSeparator:
                case JsonToken.ObjectEnd:
                case JsonToken.Separator:
                default: throw new InvalidJsonException($"{tokenReader.LineColString} Unexpected token {tokenReader.NextToken}");
            }

            if (valueType.IsAssignableFrom(jsonType)) {
                return Convert.ChangeType(Convert.ChangeType(jsonValue, valueType), destinationType);
            }

            // Try using an implicit conversion operator
            var implicitConversionOperator = TypeInfo.GetImplicitConversionOperator(valueType, jsonType);
            if (implicitConversionOperator != null) {
                return implicitConversionOperator.Invoke(null, new[] { jsonValue });
            }

            // No luck
            throw new InvalidCastException($"Can't assign value of type '{jsonType}' to value type {valueType} and destination type {destinationType}");
        }

        void ReadList(JsonTokenReader tokenReader, IList list, Type elementType) {
            tokenReader.SkipToken(JsonToken.ArrayStart);

            bool expectingValue = false;
            while (tokenReader.NextToken != JsonToken.ArrayEnd) {
                expectingValue = false;
                list.Add(ReadValueOfType(tokenReader, elementType));
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

        IList ReadMultiList(JsonTokenReader tokenReader, Type elementType, int rank) {
            tokenReader.SkipToken(JsonToken.ArrayStart);

            IList list = new ArrayList();

            bool expectingValue = false;
            while (tokenReader.NextToken != JsonToken.ArrayEnd) {
                expectingValue = false;

                list.Add(rank > 1 ? ReadMultiList(tokenReader, elementType, rank - 1) : ReadValueOfType(tokenReader, elementType));

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

        object MapArray(JsonTokenReader tokenReader, Type destinationType) {
            var arrayMetadata = TypeInfo.GetCachedArrayMetadata(destinationType);

            if (arrayMetadata.IsArray) {
                int rank = arrayMetadata.ArrayRank;
                var elementType = destinationType.GetElementType();

                // If we've parsed it as an array previously, GetElementType() should never return null.
                Debug.Assert(elementType != null);

                if (rank == 1) { // Simple array
                    var tempValues = new List<object>(); // TODO Temp alloc
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
                    CopyArray(new int[lengths.Length], 0, rank, result, list, elementType);  // TODO Temp alloc
                    return result;
                }
            }

            if (arrayMetadata.IsList) {
                var list = (IList)Activator.CreateInstance(destinationType);
                ReadList(tokenReader, list, arrayMetadata.ElementType);
                return list;
            }

            throw new InvalidCastException($"Type {destinationType} can't act as an array");
        }

        object MapObject(JsonTokenReader tokenReader, Type destinationType) {
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
                        ReadValueOfType(tokenReader, propertyMetadata.Type);
                    } else {
                        if (propertyMetadata.IsField) {
                            ((FieldInfo)propertyMetadata.Info).SetValue(instance, ReadValueOfType(tokenReader, propertyMetadata.Type));
                        } else {
                            var propertyInfo = (PropertyInfo)propertyMetadata.Info;
                            if (propertyInfo.CanWrite) {
                                propertyInfo.SetValue(instance, ReadValueOfType(tokenReader, propertyMetadata.Type), null);
                            } else {
                                throw new InvalidJsonException("Read property value from json but the property " +
                                                               $"{propertyInfo.Name} in type {destinationType} is read-only.");
                            }
                        }
                    }
                } else if (objectMetadata.IsDictionary) {
                    ((IDictionary)instance).Add(propertyName, ReadValueOfType(tokenReader, objectMetadata.ElementType));
                } else {
                    throw new InvalidJsonException($"The type {destinationType} doesn't have the property '{propertyName}'");
                }

                if (tokenReader.NextToken == JsonToken.Separator) {
                    expectingValue = true;
                    tokenReader.SkipToken(JsonToken.Separator);
                } else if (tokenReader.NextToken != JsonToken.ObjectEnd) {
                    throw new InvalidJsonException($"Expecting an comma or }} but found {tokenReader.NextToken}");
                }
            }

            if (expectingValue) {
                throw new InvalidJsonException($"{tokenReader.LineColString} Unexpected object end token");
            }

            tokenReader.SkipToken(JsonToken.ObjectEnd);

            return instance;
        }

        static JsonValue ReadJsonValue(JsonTokenReader tokenReader) {
            switch (tokenReader.NextToken) {
                case JsonToken.Null: tokenReader.SkipToken(JsonToken.Null); return new JsonValue(null);
                case JsonToken.True: tokenReader.SkipToken(JsonToken.True); return new JsonValue(true);
                case JsonToken.False: tokenReader.SkipToken(JsonToken.False); return new JsonValue(false);
                case JsonToken.String: return new JsonValue(tokenReader.ConsumeString());
                case JsonToken.Number: {
                    double number;
                    try {
                        number = tokenReader.ConsumeNumber();
                    } catch (InvalidJsonException inner) {
                        // Need to re-throw these because the value parsers don't have access to the line & column info
                        throw new InvalidJsonException(tokenReader.LineColString + " " + inner.Message);
                    }
                    
                    try {
                        return number == (int)number ? new JsonValue((int)number) : new JsonValue(number);
                    } catch (FormatException) {
                        // TODO this line/col number is wrong.  It points to after the number token that we failed to parse.
                        throw new InvalidJsonException($"{tokenReader.LineColString} Can't parse number.");
                    }
                }
                case JsonToken.ArrayStart: {
                    var arrayValue = new JsonValue(JsonValueType.Array);

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
                case JsonToken.ObjectStart: {
                    var result = new JsonValue(JsonValueType.Object);

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
                
                case JsonToken.EOF: throw new InvalidJsonException($"{tokenReader.LineColString} Unexpected end of file");
                case JsonToken.ArrayEnd:
                case JsonToken.ObjectEnd:
                case JsonToken.Separator:
                case JsonToken.KeyValueSeparator:
                case JsonToken.None:
                default: throw new InvalidJsonException($"{tokenReader.LineColString} Unexpected token {tokenReader.NextToken}");
            }
        }
    }
}
