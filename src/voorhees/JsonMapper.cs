using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using TypeInfo = Voorhees.Internal.TypeInfo;

namespace Voorhees {
    public static partial class JsonMapper {
        // Writing
        public static string ToJson<T>(T obj) {
            var os = JsonConfig.CurrentConfig.PrettyPrint ? new PrettyPrintJsonOutputStream()
                : new JsonOutputStream();
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
            if (JsonConfig.CurrentConfig.customExporters.TryGetValue(objType, out var customExporter)) {
                customExporter(obj, os);
                return;
            }

            // If not, maybe there's a built-in serializer
            if (JsonConfig.builtInExporters.TryGetValue(objType, out var builtInExporter)) {
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

    public static partial class JsonMapper {
        // Reading
        public static T FromJson<T>(string jsonString) {
            return (T) FromJson(JsonReader.Read(jsonString), typeof(T));
        }

        /////////////////////////////////////////////////

        static object FromJson(JsonValue jsonValue, Type destinationType) {
            var underlyingType = Nullable.GetUnderlyingType(destinationType);
            var valueType = underlyingType ?? destinationType;

            if (jsonValue.Type == JsonType.Null) {
                if (destinationType.IsClass || underlyingType != null) {
                    return null;
                }
                throw new Exception($"Can't assign null to an instance of type {destinationType}");
            }

            var jsonType = typeof(object);
            switch (jsonValue.Type) {
                case JsonType.Null:
                case JsonType.Object: jsonType = typeof(object); break;
                case JsonType.Array: jsonType = typeof(Array); break;
                case JsonType.String: jsonType = typeof(string); break;
                case JsonType.Boolean: jsonType = typeof(bool); break;
                case JsonType.Int: jsonType = typeof(int); break;
                case JsonType.Float: jsonType = typeof(float); break;
            }

            // If there's a custom importer that fits, use it
            var config = JsonConfig.CurrentConfig;
            if (config.customImporters.ContainsKey(jsonType) && config.customImporters[jsonType].ContainsKey(valueType)) {
                return config.customImporters[jsonType][valueType](jsonValue.Value);
            }

            // Maybe there's a base importer that works
            if (JsonConfig.builtInImporters.ContainsKey(jsonType) && JsonConfig.builtInImporters[jsonType].ContainsKey(valueType)) {
                return JsonConfig.builtInImporters[jsonType][valueType](jsonValue.Value);
            }

            switch (jsonValue.Type) {
                case JsonType.Null:
                case JsonType.Int: return MapValueToType(jsonValue, typeof(int), valueType, destinationType);
                case JsonType.Float: return MapValueToType(jsonValue, typeof(float), valueType, destinationType);
                case JsonType.Boolean: return MapValueToType(jsonValue, typeof(bool), valueType, destinationType);
                case JsonType.String: return MapValueToType(jsonValue, typeof(string), valueType, destinationType);
                case JsonType.Array: {
                    var arrayMetadata = TypeInfo.GetCachedArrayMetadata(destinationType);
                    
                    if (arrayMetadata.IsArray) {
                        int rank = arrayMetadata.ArrayRank;
                        var elementType = destinationType.GetElementType();

                        if (elementType == null) {
                            throw new InvalidOperationException("Attempting to map an array but the array element type is null");
                        }
                        
                        if (rank == 1) { // Simple array
                            var result = Array.CreateInstance(elementType, jsonValue.Count);
                            for (int i = 0; i < jsonValue.Count; i++) {
                                result.SetValue(FromJson(jsonValue[i], elementType), i);
                            }
                            return result;
                        } else {
                            if (jsonValue.Count == 0) {
                                return Array.CreateInstance(elementType, new int[rank]);
                            }

                            // Figure out the size of each dimension the array.
                            var lengths = new int[rank];
                            var currentArray = jsonValue;
                            for (int dimension = 0; dimension < rank; ++dimension) {
                                lengths[dimension] = currentArray.Count;
                                currentArray = currentArray[0];
                            }
                            
                            var result = Array.CreateInstance(elementType, lengths);
                            var currentIndex = new int[lengths.Length];
                            void ReadArray(JsonValue current, int currentDimension) {
                                for (int i = 0; i < current.Count; ++i) {
                                    currentIndex[currentDimension] = i;
                                    if (currentDimension == rank - 1) {
                                        result.SetValue(FromJson(current[i], elementType), currentIndex);
                                    } else {
                                        ReadArray(current[i], currentDimension + 1);
                                    }
                                }
                            }
                            ReadArray(jsonValue, 0);
                            return result;
                        }
                    }

                    if (arrayMetadata.IsList) {
                        var list = (IList) Activator.CreateInstance(destinationType);
                        list.Clear();
                        foreach (var element in jsonValue) {
                            list.Add(FromJson(element, arrayMetadata.ElementType));
                        }
                        return list;
                    }
                    
                    throw new Exception($"Type {destinationType} can't act as an array");
                }
                case JsonType.Object: {
                    var objectMetadata = TypeInfo.GetObjectMetadata(valueType);

                    var instance = Activator.CreateInstance(valueType);

                    foreach (string property in jsonValue.Keys) {
                        var val = jsonValue[property];

                        if (objectMetadata.Properties.TryGetValue(property, out var propertyMetadata)) {
                            if (propertyMetadata.Ignored) {
                                continue;
                            }
                            if (propertyMetadata.IsField) {
                                ((FieldInfo) propertyMetadata.Info).SetValue(instance, FromJson(val, propertyMetadata.Type));
                            } else {
                                var propertyInfo = (PropertyInfo)propertyMetadata.Info;
                                if (propertyInfo.CanWrite) {
                                    propertyInfo.SetValue(instance, FromJson(val, propertyMetadata.Type), null);
                                } else {
                                    throw new Exception("Read property value from json but the property " +
                                                        $"{propertyInfo.Name} in type {valueType} is read-only.");
                                }
                            }
                        } else if (objectMetadata.IsDictionary) {
                            ((IDictionary) instance).Add(property, FromJson(val, objectMetadata.ElementType));
                        } else {
                            throw new Exception($"The type {destinationType} doesn't have the property '{property}'");
                        }
                    }

                    return instance;
                }
                default: throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Converts a basic json value to an object of the specified type.
        /// </summary>
        /// <param name="json">The json value</param>
        /// <param name="jsonType">The type of the json value (int, float, string, etc.)</param>
        /// <param name="valueType"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        static object MapValueToType(JsonValue json, Type jsonType, Type valueType, Type destinationType) {
            if (valueType.IsAssignableFrom(jsonType)) {
                return json.Value;
            }

            // Integral value can be converted to enum values
            if (jsonType == typeof(int) && valueType.IsEnum) {
                return Enum.ToObject(valueType, json.Value);
            }
            
            // Try using an implicit conversion operator
            var implicitConversionOperator = TypeInfo.GetImplicitConversionOperator(valueType, jsonType);
            if (implicitConversionOperator != null) {
                return implicitConversionOperator.Invoke(null, new[] {json.Value});
            }

            // No luck
            throw new Exception($"Can't assign value '{JsonWriter.ToJson(json)}' ({jsonType}) to type {destinationType}");
        }
    }
}
