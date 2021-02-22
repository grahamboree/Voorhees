using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using Voorhees.Internal;

namespace Voorhees {
    public static class JsonMapper {
        public static string ToJson(object obj) {
            var sb = new StringBuilder();
            WriteJson(obj, 0, sb);
            return sb.ToString();
        }

        public static T FromJson<T>(string jsonString) {
            return (T) FromJson(JsonReader.Read(jsonString), typeof(T));
        }

        /////////////////////////////////////////////////

        static void WriteJson(object obj, int indentLevel, StringBuilder sb) {
            string tabs = "";
            if (JsonConfig.CurrentConfig.PrettyPrint) {
                for (int i = 0; i < indentLevel; ++i) {
                    tabs += "\t";
                }
            }

            void Write1DArray(IList arrayVal) {
                if (JsonConfig.CurrentConfig.PrettyPrint) {
                    sb.Append(tabs + "[\n");
                    for (var i = 0; i < arrayVal.Count; i++) {
                        WriteJson(arrayVal[i], indentLevel + 1, sb);
                        if (i < arrayVal.Count - 1) {
                            sb.Append(",");
                        }
                        sb.Append("\n");
                    }
                    sb.Append(tabs + "]");
                } else {
                    sb.Append("[");
                    for (int i = 0; i < arrayVal.Count; ++i) {
                        WriteJson(arrayVal[i], indentLevel + 1, sb);
                        if (i < arrayVal.Count - 1) {
                            sb.Append(",");
                        }
                    }
                    sb.Append("]");
                }
            }

            switch (obj) {
                case null: sb.Append("null"); return;
                case JsonValue jsonValue: sb.Append(JsonWriter.ToJson(jsonValue)); return;

                // JSON String
                case string stringVal: sb.Append(tabs); sb.Append(JsonOutputStream.StringToJsonString(stringVal)); return;
                case char charVal: sb.Append(tabs); sb.Append(JsonOutputStream.StringToJsonString("" + charVal)); return;

                // JSON Number
                case float floatVal: sb.Append(tabs); sb.Append(floatVal.ToString(CultureInfo.InvariantCulture)); return;
                case double doubleVal: sb.Append(tabs); sb.Append(doubleVal.ToString(CultureInfo.InvariantCulture)); return;
                case decimal decimalVal: sb.Append(tabs); sb.Append(decimalVal.ToString(CultureInfo.InvariantCulture)); return;
                case byte byteVal: sb.Append(tabs); sb.Append(byteVal); return;
                case sbyte sbyteVal: sb.Append(tabs); sb.Append(sbyteVal); return;
                case int intVal: sb.Append(tabs); sb.Append(intVal); return;
                case uint uintVal: sb.Append(tabs); sb.Append(uintVal); return;
                case long longVal: sb.Append(tabs); sb.Append(longVal); return;
                case ulong ulongVal: sb.Append(tabs); sb.Append(ulongVal); return;
                case short shortVal: sb.Append(tabs); sb.Append(shortVal); return;
                case ushort ushortVal: sb.Append(tabs); sb.Append(ushortVal); return;

                // JSON Boolean
                case bool boolVal: sb.Append(tabs); sb.Append(boolVal ? "true" : "false"); return;

                // JSON Array
                case Array arrayVal: {
                    // Faster code for the common case.
                    if (arrayVal.Rank == 1) {
                        Write1DArray(arrayVal);
                        return;
                    }
                    
                    // Handles arbitrary dimension arrays.
                    int[] index = new int[arrayVal.Rank];
                    void jsonifyArray(Array arr, int currentDimension, int indent) {
                        string arrayTabs = "";
                        if (JsonConfig.CurrentConfig.PrettyPrint) {
                            for (int i = 0; i < indent; ++i) {
                                arrayTabs += "\t";
                            }

                            sb.Append(arrayTabs);
                        }

                        sb.Append("[");
                        if (JsonConfig.CurrentConfig.PrettyPrint) {
                            sb.Append("\n");
                        }

                        int length = arr.GetLength(currentDimension);
                        for (int i = 0; i < length; ++i) {
                            index[currentDimension] = i;

                            if (currentDimension == arr.Rank - 1) {
                                WriteJson(arr.GetValue(index), indent + 1, sb);
                            } else {
                                jsonifyArray(arr, currentDimension + 1, indent + 1);
                            }
                            
                            if (i < length - 1) {
                                sb.Append(",");
                            }
                            
                            if (JsonConfig.CurrentConfig.PrettyPrint) {
                                sb.Append("\n");
                            }
                        }

                        if (JsonConfig.CurrentConfig.PrettyPrint) {
                            sb.Append(arrayTabs);
                        }

                        sb.Append("]");
                    }
                    jsonifyArray(arrayVal, 0, indentLevel);
                    return;
                }
                case IList listVal: Write1DArray(listVal); return;

                // JSON Object
                case IDictionary dictionary: {
                    if (JsonConfig.CurrentConfig.PrettyPrint) {
                        sb.Append(tabs + "{\n");
                        bool first = true;
                        var valueBuilder = new StringBuilder();
                        foreach (DictionaryEntry entry in dictionary) {
                            if (!first) {
                                sb.Append(",\n");
                            }

                            first = false;
                            
                            valueBuilder.Length = 0;
                            WriteJson(entry.Value, indentLevel + 1, valueBuilder);
                            
                            string propertyName = entry.Key is string key
                                ? key
                                : Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                            sb.Append(tabs + "\t\"" + propertyName + "\": " + valueBuilder.ToString().Substring(indentLevel + 1));
                        }
                        if (dictionary.Count > 0) {
                            sb.Append("\n");
                        }
                        sb.Append(tabs + "}");
                        return;
                    } else {
                        sb.Append("{");
                        bool first = true;
                        foreach (DictionaryEntry entry in dictionary) {
                            if (!first) {
                                sb.Append(",");
                            }

                            first = false;

                            string propertyName = entry.Key is string key
                                ? key
                                : Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                            sb.Append("\"" + propertyName + "\":");
                            WriteJson(entry.Value, indentLevel + 1, sb);
                        }

                        sb.Append("}");
                        return;
                    }
                }
            }

            var obj_type = obj.GetType();

            // See if there's a custom exporter for the object
            if (JsonConfig.CurrentConfig.customExporters.TryGetValue(obj_type, out var customExporter)) {
                sb.Append(tabs);
                sb.Append(customExporter(obj));
                return;
            }

            // If not, maybe there's a built-in serializer
            if (JsonConfig.builtInExporters.TryGetValue(obj_type, out var builtInExporter)) {
                sb.Append(tabs);
                sb.Append(builtInExporter(obj));
                return;
            }

            if (obj is Enum) {
                var enumType = Enum.GetUnderlyingType(obj_type);

                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(byte)) { sb.Append(tabs); sb.Append(((byte) obj).ToString()); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(sbyte)) { sb.Append(tabs); sb.Append(((sbyte) obj).ToString()); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(short)) { sb.Append(tabs); sb.Append(((short) obj).ToString()); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(ushort)) { sb.Append(tabs); sb.Append(((ushort) obj).ToString()); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(int)) { sb.Append(tabs); sb.Append(((int) obj).ToString()); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(uint)) { sb.Append(tabs); sb.Append(((uint) obj).ToString()); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(long)) { sb.Append(tabs); sb.Append(((long) obj).ToString()); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(ulong)) { sb.Append(tabs); sb.Append(((ulong) obj).ToString()); return; }

                throw new InvalidOperationException("Unknown underlying enum type: " + enumType);
            }

            if (JsonConfig.CurrentConfig.PrettyPrint) {
                sb.Append(tabs + "{\n");
                
                bool first = true;
                var valueBuilder = new StringBuilder();
                var metadata = TypeInfo.GetTypePropertyMetadata(obj_type);
                foreach (var propertyMetadata in metadata) {
                    if (!first) {
                        sb.Append(",\n");
                    }
                    first = false;
                    
                    valueBuilder.Length = 0;
                    if (propertyMetadata.IsField) {
                        WriteJson(((FieldInfo) propertyMetadata.Info).GetValue(obj), indentLevel + 1, valueBuilder);
                    } else {
                        var propertyInfo = (PropertyInfo) propertyMetadata.Info;
                        if (propertyInfo.CanRead) {
                            WriteJson(propertyInfo.GetValue(obj, null), indentLevel + 1, valueBuilder);
                        }
                    }
                    sb.Append(tabs + "\t\"" + propertyMetadata.Info.Name + "\": " + valueBuilder.ToString().Substring(indentLevel + 1));
                }
                if (metadata.Count > 0) {
                    sb.Append("\n");
                }
                sb.Append(tabs + "}");
            } else {
                sb.Append("{");
                bool first = true;
                foreach (var propertyMetadata in TypeInfo.GetTypePropertyMetadata(obj_type)) {
                    if (!first) {
                        sb.Append(",");
                    }
                    first = false;
                    
                    if (propertyMetadata.IsField) {
                        sb.Append("\"" + propertyMetadata.Info.Name + "\":");
                        WriteJson(((FieldInfo) propertyMetadata.Info).GetValue(obj), indentLevel + 1, sb);
                    } else {
                        var propertyInfo = (PropertyInfo) propertyMetadata.Info;

                        if (propertyInfo.CanRead) {
                            sb.Append("\"" + propertyMetadata.Info.Name + "\":");
                            WriteJson(propertyInfo.GetValue(obj, null), indentLevel + 1, sb);
                        }
                    }
                }
                sb.Append("}");
            }
        }
        
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
