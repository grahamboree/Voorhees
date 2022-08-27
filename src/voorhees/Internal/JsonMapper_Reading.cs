using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TypeInfo = Voorhees.Internal.TypeInfo;

namespace Voorhees {
    // Value type parser instances.  This is necessary to trick the type system into not boxing the value type results.
    public static partial class JsonMapper {
        static IValueParser<T> GetParser<T>() {
            var destinationType = typeof(T);
            if (destinationType == typeof(byte)) { return (IValueParser<T>)ByteValueParser.Instance; }
            if (destinationType == typeof(sbyte)) { return (IValueParser<T>)SByteValueParser.Instance; }
            if (destinationType == typeof(short)) { return (IValueParser<T>)ShortValueParser.Instance; }
            if (destinationType == typeof(ushort)) { return (IValueParser<T>)UShortValueParser.Instance; }
            if (destinationType == typeof(int)) { return (IValueParser<T>)IntValueParser.Instance; }
            if (destinationType == typeof(uint)) { return (IValueParser<T>)UIntValueParser.Instance; }
            if (destinationType == typeof(long)) { return (IValueParser<T>)LongValueParser.Instance; }
            if (destinationType == typeof(ulong)) { return (IValueParser<T>)ULongValueParser.Instance; }
            
            if (destinationType == typeof(float)) { return (IValueParser<T>)FloatValueParser.Instance; }
            if (destinationType == typeof(double)) { return (IValueParser<T>)DoubleValueParser.Instance; }
            if (destinationType == typeof(decimal)) { return (IValueParser<T>)DecimalValueParser.Instance; }
            
            if (destinationType == typeof(char)) { return (IValueParser<T>)CharValueParser.Instance; }
            
            if (destinationType == typeof(DateTime)) { return (IValueParser<T>)DateTimeValueParser.Instance; }
            if (destinationType == typeof(DateTimeOffset)) { return (IValueParser<T>)DateTimeOffsetValueParser.Instance; }
            return null;
        }
        
        interface IValueParser<out T> {
            T Parse(JsonTokenReader tokenReader);
        }
        
        class ByteValueParser : IValueParser<byte> {
            public static readonly ByteValueParser Instance = new();
            
            public byte Parse(JsonTokenReader tokenReader) {
                return byte.Parse(tokenReader.ConsumeNumber());
            }
        }
        
        class SByteValueParser : IValueParser<sbyte> {
            public static readonly SByteValueParser Instance = new();
            
            public sbyte Parse(JsonTokenReader tokenReader) {
                return sbyte.Parse(tokenReader.ConsumeNumber());
            }
        }
        
        class ShortValueParser : IValueParser<short> {
            public static readonly ShortValueParser Instance = new();
            
            public short Parse(JsonTokenReader tokenReader) {
                return short.Parse(tokenReader.ConsumeNumber());
            }
        }
        
        class UShortValueParser : IValueParser<ushort> {
            public static readonly UShortValueParser Instance = new();
            
            public ushort Parse(JsonTokenReader tokenReader) {
                return ushort.Parse(tokenReader.ConsumeNumber());
            }
        }
        
        class IntValueParser : IValueParser<int> {
            public static readonly IntValueParser Instance = new();
            
            public int Parse(JsonTokenReader tokenReader) {
                return int.Parse(tokenReader.ConsumeNumber());
            }
        }
        
        class UIntValueParser : IValueParser<uint> {
            public static readonly UIntValueParser Instance = new();
            
            public uint Parse(JsonTokenReader tokenReader) {
                return uint.Parse(tokenReader.ConsumeNumber());
            }
        }
        
        class LongValueParser : IValueParser<long> {
            public static readonly LongValueParser Instance = new();
            
            public long Parse(JsonTokenReader tokenReader) {
                return long.Parse(tokenReader.ConsumeNumber());
            }
        }
        
        class ULongValueParser : IValueParser<ulong> {
            public static readonly ULongValueParser Instance = new();
            
            public ulong Parse(JsonTokenReader tokenReader) {
                return ulong.Parse(tokenReader.ConsumeNumber());
            }
        }
        
        class FloatValueParser : IValueParser<float> {
            public static readonly FloatValueParser Instance = new();
            
            public float Parse(JsonTokenReader tokenReader) {
                return float.Parse(tokenReader.ConsumeNumber());
            }
        }
        
        class DoubleValueParser : IValueParser<double> {
            public static readonly DoubleValueParser Instance = new();
            
            public double Parse(JsonTokenReader tokenReader) {
                return double.Parse(tokenReader.ConsumeNumber());
            }
        }
        
        class DecimalValueParser : IValueParser<decimal> {
            public static readonly DecimalValueParser Instance = new();
            
            public decimal Parse(JsonTokenReader tokenReader) {
                return decimal.Parse(tokenReader.ConsumeNumber());
            }
        }
        
        class CharValueParser : IValueParser<char> {
            public static readonly CharValueParser Instance = new();
            
            public char Parse(JsonTokenReader tokenReader) {
                string stringVal = tokenReader.ConsumeString();
                if (stringVal.Length != 1) {
                    throw new FormatException($"{tokenReader.LineColString} Trying to map a string of length != 1 to a char: \"{stringVal}\"");
                }
                return stringVal[0];
            }
        }
        
        class DateTimeValueParser : IValueParser<DateTime> {
            public static readonly DateTimeValueParser Instance = new();
            
            public DateTime Parse(JsonTokenReader tokenReader) {
                return DateTime.Parse(tokenReader.ConsumeString());
            }
        }
        
        class DateTimeOffsetValueParser : IValueParser<DateTimeOffset> {
            public static readonly DateTimeOffsetValueParser Instance = new();
            
            public DateTimeOffset Parse(JsonTokenReader tokenReader) {
                return DateTimeOffset.Parse(tokenReader.ConsumeString());
            }
        }
    }
    
    public static partial class JsonMapper {
        static T ReadValueOfType<T>(JsonTokenReader tokenReader) {
            var destinationType = typeof(T);
            // If there's a custom importer that fits, use it
            var config = Voorhees.Instance;
            if (config.CustomImporters.TryGetValue(destinationType, out var customImporter)) {
                return (T)customImporter(tokenReader);
            }
            
            var parser = GetParser<T>();
            if (parser != null) {
                return parser.Parse(tokenReader);
            }
            return (T)ReadValueOfType(tokenReader, destinationType);
        }
        
        static object ReadValueOfType(JsonTokenReader tokenReader, Type destinationType) {
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
                    throw new Exception($"{tokenReader} Can't assign null to an instance of type {destinationType}");
                }
                case JsonToken.ObjectStart: return MapObject(tokenReader, destinationType);
                case JsonToken.ArrayStart: return MapArray(tokenReader, destinationType);
                case JsonToken.String: {
                    jsonType = typeof(string);
                    jsonValue = tokenReader.ConsumeString();
                } break;
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

        static void ReadList(JsonTokenReader tokenReader, IList list, Type elementType) {
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

        static IList ReadMultiList(JsonTokenReader tokenReader, Type elementType, int rank) {
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

        static object MapArray(JsonTokenReader tokenReader, Type destinationType) {
            var arrayMetadata = TypeInfo.GetCachedArrayMetadata(destinationType);

            if (arrayMetadata.IsArray) {
                int rank = arrayMetadata.ArrayRank;
                var elementType = destinationType.GetElementType();

                if (elementType == null) {
                    throw new InvalidOperationException("Attempting to map an array but the array element type is null");
                }

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
                        ReadValueOfType(tokenReader, propertyMetadata.Type);
                    } else {
                        if (propertyMetadata.IsField) {
                            ((FieldInfo)propertyMetadata.Info).SetValue(instance, ReadValueOfType(tokenReader, propertyMetadata.Type));
                        } else {
                            var propertyInfo = (PropertyInfo)propertyMetadata.Info;
                            if (propertyInfo.CanWrite) {
                                propertyInfo.SetValue(instance, ReadValueOfType(tokenReader, propertyMetadata.Type), null);
                            } else {
                                throw new Exception("Read property value from json but the property " +
                                                    $"{propertyInfo.Name} in type {destinationType} is read-only.");
                            }
                        }
                    }
                } else if (objectMetadata.IsDictionary) {
                    ((IDictionary)instance).Add(propertyName, ReadValueOfType(tokenReader, objectMetadata.ElementType));
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

        static JsonValue ReadJsonValue(JsonTokenReader tokenReader) {
            switch (tokenReader.NextToken) {
                case JsonToken.Null: tokenReader.SkipToken(JsonToken.Null); return new JsonValue(null);
                case JsonToken.True: tokenReader.SkipToken(JsonToken.True); return new JsonValue(true);
                case JsonToken.False: tokenReader.SkipToken(JsonToken.False); return new JsonValue(false);
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
                case JsonToken.ArrayStart: {
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
                case JsonToken.ObjectStart: {
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