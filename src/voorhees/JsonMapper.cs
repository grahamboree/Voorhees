using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using TypeInfo = Voorhees.Internal.TypeInfo;

namespace Voorhees {
    // Writing
    public static partial class JsonMapper {
        public static string ToJson<T>(T obj, bool prettyPrint = false) {
            var os = new JsonOutputStream(prettyPrint);
            WriteJsonToStream(obj, os, typeof(T));
            return os.ToString();
        }

        /////////////////////////////////////////////////

        static void WriteJsonToStream(object obj, JsonOutputStream os, Type objType) {
            if (obj == null) {
                os.WriteNull();
                return;
            }

            // See if there's a custom exporter for the object
            if (JsonConfig.CurrentConfig.CustomExporters.TryGetValue(objType, out var customExporter)) {
                customExporter(obj, os);
                return;
            }

            // If not, maybe there's a built-in serializer
            if (JsonConfig.BuiltInExporters.TryGetValue(objType, out var builtInExporter)) {
                builtInExporter(obj, os);
                return;
            }
            
            switch (obj) { 
                case JsonValue jsonValue: os.Write(jsonValue); return;

                // JSON String
                case string stringVal: os.Write(stringVal); return;
                case char charVal: os.Write(charVal); return;

                // JSON Number
                case byte byteVal: os.Write(byteVal); return;
                case sbyte sbyteVal: os.Write(sbyteVal); return;
                case short shortVal: os.Write(shortVal); return;
                case ushort ushortVal: os.Write(ushortVal); return;
                case int intVal: os.Write(intVal); return;
                case uint uintVal: os.Write(uintVal); return;
                case long longVal: os.Write(longVal); return;
                case ulong ulongVal: os.Write(ulongVal); return;
                case float floatVal: os.Write(floatVal); return;
                case double doubleVal: os.Write(doubleVal); return;
                case decimal decimalVal: os.Write(decimalVal); return;

                // JSON Boolean
                case bool boolVal: os.Write(boolVal); return;

                // JSON Array
                case Array arrayVal: {
                    // Faster code for the common case.
                    if (arrayVal.Rank == 1) {
                        Write1DArrayJsonToStream(arrayVal, os);
                        return;
                    }
                    
                    // Handles arbitrary dimension arrays.
                    int[] index = new int[arrayVal.Rank];

                    void jsonifyArray(Array arr, int currentDimension) {
                        os.WriteArrayStart();

                        int length = arr.GetLength(currentDimension);
                        for (int i = 0; i < length; ++i) {
                            index[currentDimension] = i;

                            if (currentDimension == arr.Rank - 1) {
                                var arrayObject = arr.GetValue(index);
                                WriteJsonToStream(arrayObject, os, arr.GetType().GetElementType());
                            } else {
                                jsonifyArray(arr, currentDimension + 1);
                            }
                            
                            if (i < length - 1) {
                                os.WriteArraySeparator();
                            } else {
                                os.WriteArrayListTerminator();
                            }
                        }

                        os.WriteArrayEnd();
                    }
                    jsonifyArray(arrayVal, 0);
                    return;
                }
                case IList listVal: Write1DArrayJsonToStream(listVal, os); return;

                // JSON Object
                case IDictionary dictionary: {
                    os.WriteObjectStart();

                    int entryIndex = 0;
                    int length = dictionary.Count;
                    
                    foreach (DictionaryEntry entry in dictionary) {
                        string propertyName = entry.Key is string key
                            ? key
                            : Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                        os.Write(propertyName);
                        os.WriteObjectKeyValueSeparator();
                        var value = entry.Value;
                        WriteJsonToStream(value, os, value.GetType());
                        
                        if (entryIndex < length - 1) {
                            os.WriteArraySeparator();
                        } else {
                            os.WriteArrayListTerminator();
                        }
                        entryIndex++;
                    }

                    os.WriteObjectEnd();
                    return;
                }
            }

            if (obj is Enum) {
                var enumType = Enum.GetUnderlyingType(objType);
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(byte)) { os.Write((byte) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(sbyte)) { os.Write((byte) obj);  return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(short)) { os.Write((byte) obj);  return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(ushort)) { os.Write((byte) obj);  return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(int)) { os.Write((byte) obj);  return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(uint)) { os.Write((byte) obj);  return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(long)) { os.Write((byte) obj);  return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(ulong)) { os.Write((byte) obj);  return; }

                throw new InvalidOperationException("Unknown underlying enum type: " + enumType);
            }

            os.WriteObjectStart();
            
            var fieldsAndProperties = TypeInfo.GetTypePropertyMetadata(objType);
            for (var fieldIndex = 0; fieldIndex < fieldsAndProperties.Count; fieldIndex++) {
                var propertyMetadata = fieldsAndProperties[fieldIndex];

                if (!propertyMetadata.IsField) {
                    var propertyInfo = propertyMetadata.Info as PropertyInfo;
                    if (propertyInfo != null && !propertyInfo.CanRead) {
                        fieldsAndProperties.RemoveAt(fieldIndex);
                        fieldIndex--;                        
                    }
                }
            }

            for (var fieldIndex = 0; fieldIndex < fieldsAndProperties.Count; fieldIndex++) {
                var propertyMetadata = fieldsAndProperties[fieldIndex];
                
                string key;
                object value;

                if (propertyMetadata.IsField) {
                    var fieldInfo = (FieldInfo) propertyMetadata.Info;
                    key = fieldInfo.Name;
                    value = fieldInfo.GetValue(obj);
                } else {
                    var propertyInfo = (PropertyInfo) propertyMetadata.Info;
                    key = propertyInfo.Name;
                    value = propertyInfo.GetValue(obj, null);
                }

                os.Write(key);
                os.WriteObjectKeyValueSeparator();
                WriteJsonToStream(value, os, value.GetType());

                if (fieldIndex < fieldsAndProperties.Count - 1) {
                    os.WriteArraySeparator();
                } else {
                    os.WriteArrayListTerminator();
                }
            }

            os.WriteObjectEnd();
        }

        static void Write1DArrayJsonToStream(IList list, JsonOutputStream os) {
            os.WriteArrayStart();
            for (var i = 0; i < list.Count; i++) {
                var listVal = list[i];
                WriteJsonToStream(listVal, os, listVal.GetType());

                if (i < list.Count - 1) {
                    os.WriteArraySeparator();
                } else {
                    os.WriteArrayListTerminator();
                }
            }
            os.WriteArrayEnd();
        }
    }

    // Reading
    public static partial class JsonMapper {
        public static T FromJson<T>(string jsonString) {
            return (T) FromJson(new JsonTokenizer(jsonString), typeof(T));
        }

        /////////////////////////////////////////////////

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
            var config = JsonConfig.CurrentConfig;
            if (config.LowLevelCustomImporters.TryGetValue(destinationType, out var lowLevelImporter)) {
                return lowLevelImporter(tokenizer);
            }
            if (config.CustomImporters.TryGetValue(destinationType, out var customImporter)) {
                return customImporter(JsonReader.ReadJsonValue(tokenizer));
            }
            
            // Maybe there's a base importer that works
            if (JsonConfig.BuiltInImporters.TryGetValue(destinationType, out var builtInImporter)) {
                return builtInImporter(tokenizer);
            }
            
            Type jsonType;
            object jsonValue;
            
            switch (tokenizer.NextToken) {
                case JsonToken.Null: 
                case JsonToken.ObjectStart: return MapObject(tokenizer, destinationType);
                case JsonToken.ArrayStart: return MapArray(tokenizer, destinationType);
                case JsonToken.String: jsonType = typeof(string); jsonValue = tokenizer.ConsumeString(); break;
                case JsonToken.Number: {
                    string numberString = tokenizer.ConsumeNumber();
                    if (int.TryParse(numberString, out int intVal)) {
                        jsonType = typeof(int);
                        jsonValue = intVal;
                    } else if (float.TryParse(numberString, out float floatVal)) {
                        jsonType = typeof(float);
                        jsonValue = floatVal;
                    } else {
                        throw new InvalidJsonException($"Can't parse number value: \"{numberString}\" at character {tokenizer.Cursor}");
                    }
                } break;
                case JsonToken.True: jsonType = typeof(bool); jsonValue = true; break;
                case JsonToken.False: jsonType = typeof(bool); jsonValue = false; break;
                default: throw new ArgumentOutOfRangeException();
            }

            return MapValueToType(jsonValue, jsonType, valueType, destinationType);
        }
        
        static void ReadList(JsonTokenizer tokenizer, IList list, Type elementType) {
            tokenizer.ConsumeToken(); // [
            
            bool expectingValue = false;
            while (tokenizer.NextToken != JsonToken.ArrayEnd) {
                expectingValue = false;
                list.Add(FromJson(tokenizer, elementType));
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
        }

        static IList ReadMultiList(JsonTokenizer tokenizer, Type elementType, int rank) {
            tokenizer.ConsumeToken(); // [

            IList list = new ArrayList();
            
            bool expectingValue = false;
            while (tokenizer.NextToken != JsonToken.ArrayEnd) {
                expectingValue = false;

                if (rank > 1) {
                    list.Add(ReadMultiList(tokenizer, elementType, rank - 1));
                } else {
                    list.Add(FromJson(tokenizer, elementType));
                }
                
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
                var list = (IList) Activator.CreateInstance(destinationType);
                ReadList(tokenizer, list, arrayMetadata.ElementType);
                return list;
            }
            
            throw new Exception($"Type {destinationType} can't act as an array");
        }

        static object MapObject(JsonTokenizer tokenizer, Type destinationType) {
            var objectMetadata = TypeInfo.GetObjectMetadata(destinationType);
            tokenizer.ConsumeToken(); // {

            var instance = Activator.CreateInstance(destinationType);

            bool expectingValue = false;

            while (tokenizer.NextToken != JsonToken.ObjectEnd) {
                expectingValue = false;
                string propertyName = tokenizer.ConsumeString();
                tokenizer.ConsumeToken(); // : 
                
                if (objectMetadata.Properties.TryGetValue(propertyName, out var propertyMetadata)) {
                    if (propertyMetadata.Ignored) {
                        // Read the value so we advance the tokenizer, but don't do anything with it.
                        FromJson(tokenizer, propertyMetadata.Type);
                    } else {
                        if (propertyMetadata.IsField) {
                            ((FieldInfo) propertyMetadata.Info).SetValue(instance, FromJson(tokenizer, propertyMetadata.Type));
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
                    ((IDictionary) instance).Add(propertyName, FromJson(tokenizer, objectMetadata.ElementType));
                } else {
                    throw new Exception($"The type {destinationType} doesn't have the property '{propertyName}'");
                }

                if (tokenizer.NextToken == JsonToken.Separator) {
                    expectingValue = true;
                    tokenizer.ConsumeToken(); // ,
                }
            }

            if (expectingValue) {
                throw new InvalidJsonException($"Unexpected \'}}\' at character {tokenizer.Cursor}");
            }

            tokenizer.ConsumeToken(); // }
            
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
                return implicitConversionOperator.Invoke(null, new[] {jsonValue});
            }

            // No luck
            throw new Exception($"Can't assign value of type '{jsonType}' to value type {valueType} and destination type {destinationType}");
        }
    }
}
