using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Voorhees {
    public static class JsonMapper {
        public static string ToJson(object obj) {
            return ToJsonImpl(obj, 0);
        }

        public static T FromJson<T>(string jsonString) {
            return (T) FromJson(JsonReader.Read(jsonString), typeof(T));
        }

        /////////////////////////////////////////////////

        struct ObjectMetadata {
            public bool IsDictionary;
            public Dictionary<string, PropertyMetadata> Properties;

            public Type ElementType {
                get => element_type ?? typeof(JsonValue);
                set => element_type = value;
            }
            Type element_type;
        }
        static readonly Dictionary<Type, ObjectMetadata> cachedObjectMetadata = new Dictionary<Type, ObjectMetadata>();
        
        struct PropertyMetadata {
            public MemberInfo Info;
            public bool IsField;
            public Type Type;
            public bool Ignored;
        }
        static readonly Dictionary<Type, List<PropertyMetadata>> typeProperties = new Dictionary<Type, List<PropertyMetadata>>();

        struct ArrayMetadata {
            public bool IsArray;
            public int ArrayRank;
            public bool IsList;

            public Type ElementType {
                get => element_type ?? typeof(JsonValue);
                set => element_type = value;
            }
            Type element_type;
        }
        static readonly Dictionary<Type, ArrayMetadata> cachedArrayMetadata = new Dictionary<Type, ArrayMetadata>();

        static readonly Dictionary<Type, Dictionary<Type, MethodInfo>> implicitConversionOperatorCache = new Dictionary<Type, Dictionary<Type, MethodInfo>>();

        /////////////////////////////////////////////////

        /// Gather property and field info about the type
        /// Cache it so we don't have to get this info every
        /// time we come across an instance of this type
        static List<PropertyMetadata> GetTypePropertyMetadata(Type type) {
            if (!typeProperties.ContainsKey(type)) {
                var props = new List<PropertyMetadata>();

                foreach (var propertyInfo in type.GetProperties()) {
                    if (propertyInfo.Name != "Item" && !Attribute.IsDefined(propertyInfo, typeof(JsonIgnoreAttribute))) {
                        props.Add(new PropertyMetadata {
                            Info = propertyInfo,
                            IsField = false
                        });
                    }
                }

                foreach (var fieldInfo in type.GetFields()) {
                    if (!Attribute.IsDefined(fieldInfo, typeof(JsonIgnoreAttribute))) {
                        props.Add(new PropertyMetadata {
                            Info = fieldInfo,
                            IsField = true
                        });
                    }
                }

                typeProperties.Add(type, props);
            }

            return typeProperties[type];
        }

        static ArrayMetadata GetCachedArrayMetadata(Type type) {
            if (cachedArrayMetadata.ContainsKey(type)) {
                return cachedArrayMetadata[type];
            }

            var data = new ArrayMetadata {
                IsArray = type.IsArray,
                ArrayRank = type.IsArray ? type.GetArrayRank() : 1,
                IsList = type.GetInterface ("System.Collections.IList") != null
            };

            foreach (var propertyInfo in type.GetProperties()) {
                if (propertyInfo.Name == "Item") {
                    var parameters = propertyInfo.GetIndexParameters();

                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int)) {
                        data.ElementType = propertyInfo.PropertyType;
                    }
                }
            }
            
            cachedArrayMetadata.Add(type, data);
            
            return cachedArrayMetadata[type];
        }

        static MethodInfo GetImplicitConversionOperator(Type t1, Type t2) {
            if (!implicitConversionOperatorCache.ContainsKey(t1)) {
                implicitConversionOperatorCache.Add(t1, new Dictionary<Type, MethodInfo>());
            }

            if (implicitConversionOperatorCache[t1].ContainsKey(t2)) {
                return implicitConversionOperatorCache[t1][t2];
            }

            var op = t1.GetMethod("op_Implicit", new[] {t2});

            try {
                implicitConversionOperatorCache[t1].Add(t2, op);
            } catch (ArgumentException) {
                return implicitConversionOperatorCache[t1][t2];
            }

            return op;
        }

        static string ToJsonImpl(object obj, int indentLevel) {
            string tabs = "";
            if (JsonConfig.CurrentConfig.PrettyPrint) {
                for (int i = 0; i < indentLevel; ++i) {
                    tabs += "\t";
                }
            }

            switch (obj) {
                case null: return "null";
                case JsonValue jsonValue: return JsonWriter.ToJson(jsonValue);

                // JSON String
                case string stringVal: return "\"" + stringVal + "\"";
                case char charVal: return "\"" + charVal + "\"";

                // JSON Number
                case float floatVal: return tabs + floatVal.ToString(CultureInfo.InvariantCulture);
                case double doubleVal: return tabs + doubleVal.ToString(CultureInfo.InvariantCulture);
                case decimal decimalVal: return tabs + decimalVal.ToString(CultureInfo.InvariantCulture);
                case byte byteVal: return tabs + byteVal;
                case sbyte sbyteVal: return tabs + sbyteVal;
                case int intVal: return tabs + intVal;
                case uint uintVal: return tabs + uintVal;
                case long longVal: return tabs + longVal;
                case ulong ulongVal: return tabs + ulongVal;
                case short shortVal: return tabs + shortVal;
                case ushort ushortVal: return tabs + ushortVal;

                // JSON Boolean
                case bool boolVal: return tabs + (boolVal ? "true" : "false");

                // JSON Array
                case Array arrayVal: {
                    var sb = new StringBuilder();
                    
                    // Faster code for the common case.
                    if (arrayVal.Rank == 1) {
                        string result;
                        
                        if (JsonConfig.CurrentConfig.PrettyPrint) {
                            result = tabs + "[\n";
                            for (var i = 0; i < arrayVal.Length; i++) {
                                result += ToJsonImpl(arrayVal.GetValue(i), indentLevel + 1);
                                if (i < arrayVal.Length - 1) {
                                    result += ",";
                                }
                                result += "\n";
                            }
                            result += tabs + "]";
                        } else {
                            result = "[";
                            for (int i = 0; i < arrayVal.Length; ++i) {
                                result += ToJsonImpl(arrayVal.GetValue(i), indentLevel + 1);
                                if (i < arrayVal.Length - 1) {
                                    result += ",";
                                }
                            }
                            result += "]";
                        }
                        return result;
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
                                sb.Append(ToJsonImpl(arr.GetValue(index), indent + 1));
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
                    return sb.ToString();
                }
                case IList listVal: {
                    string result;
                        
                    if (JsonConfig.CurrentConfig.PrettyPrint) {
                        result = tabs + "[\n";
                        for (var i = 0; i < listVal.Count; i++) {
                            result += ToJsonImpl(listVal[i], indentLevel + 1);
                            if (i < listVal.Count - 1) {
                                result += ",";
                            }
                            result += "\n";
                        }
                        result += tabs + "]";
                    } else {
                        result = "[";
                        for (int i = 0; i < listVal.Count; ++i) {
                            result += ToJsonImpl(listVal[i], indentLevel + 1);
                            if (i < listVal.Count - 1) {
                                result += ",";
                            }
                        }
                        result += "]";
                    }
                    return result;
                }

                // JSON Object
                case IDictionary dictionary: {
                    var dictBuilder = new StringBuilder();
                    dictBuilder.Append("{");
                    bool first = true;
                    foreach (DictionaryEntry entry in dictionary) {
                        if (!first) {
                            dictBuilder.Append(",");
                        }
                        first = false;

                        string propertyName = entry.Key is string key ? key
                            : Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                        dictBuilder.Append("\"" + propertyName + "\":");
                        dictBuilder.Append(ToJsonImpl(entry.Value, indentLevel + 1));
                    }
                    dictBuilder.Append("}");
                    return dictBuilder.ToString();
                }
            }

            var obj_type = obj.GetType();

            // See if there's a custom exporter for the object
            if (JsonConfig.CurrentConfig.customExporters.TryGetValue(obj_type, out var customExporter)) {
                return customExporter(obj);
            }

            // If not, maybe there's a built-in serializer
            if (JsonConfig.builtInExporters.TryGetValue(obj_type, out var builtInExporter)) {
                return builtInExporter(obj);
            }

            if (obj is Enum) {
                var enumType = Enum.GetUnderlyingType(obj_type);

                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(byte)) { return ((byte) obj).ToString(); }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(sbyte)) { return ((sbyte) obj).ToString(); }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(short)) { return ((short) obj).ToString(); }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(ushort)) { return ((ushort) obj).ToString(); }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(int)) { return ((int) obj).ToString(); }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(uint)) { return ((uint) obj).ToString(); }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(long)) { return ((long) obj).ToString(); }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(ulong)) { return ((ulong) obj).ToString(); }

                throw new InvalidOperationException("Unknown underlying enum type: " + enumType);
            }

            var props = GetTypePropertyMetadata(obj_type);

            var objectBuilder = new StringBuilder();
            objectBuilder.Append("{");
            foreach (var propertyMetadata in props) {
                if (propertyMetadata.IsField) {
                    objectBuilder.Append("\"" + propertyMetadata.Info.Name + "\":");
                    objectBuilder.Append(ToJsonImpl(((FieldInfo) propertyMetadata.Info).GetValue(obj), indentLevel + 1));
                } else {
                    var propertyInfo = (PropertyInfo) propertyMetadata.Info;

                    if (propertyInfo.CanRead) {
                        objectBuilder.Append("\"" + propertyMetadata.Info.Name + "\":");
                        objectBuilder.Append(ToJsonImpl(propertyInfo.GetValue (obj, null), indentLevel + 1));
                    }
                }
            }
            objectBuilder.Append("}");
            return objectBuilder.ToString();
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
                    var arrayMetadata = GetCachedArrayMetadata(destinationType);
                    
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
                    var objectMetadata = GetObjectMetadata(valueType);

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
            var implicitConversionOperator = GetImplicitConversionOperator(valueType, jsonType);
            if (implicitConversionOperator != null) {
                return implicitConversionOperator.Invoke(null, new[] {json.Value});
            }

            // No luck
            throw new Exception($"Can't assign value '{JsonWriter.ToJson(json)}' ({jsonType}) to type {destinationType}");
        }

        static ObjectMetadata GetObjectMetadata(Type type) {
            if (!cachedObjectMetadata.ContainsKey(type)) {
                bool isDictionary = type.GetInterface("System.Collections.IDictionary") != null;

                var objectMetadata = new ObjectMetadata {
                    IsDictionary = isDictionary,
                    Properties = new Dictionary<string, PropertyMetadata>(),
                    ElementType = null
                };

                foreach (var propertyInfo in type.GetProperties()) {
                    if (propertyInfo.Name == "Item") {
                        var parameters = propertyInfo.GetIndexParameters();

                        if (parameters.Length != 1) {
                            continue;
                        }

                        if (parameters[0].ParameterType == typeof(string)) {
                            objectMetadata.ElementType = propertyInfo.PropertyType;
                        }

                        continue;
                    }

                    objectMetadata.Properties.Add(propertyInfo.Name, new PropertyMetadata {
                        Info = propertyInfo,
                        Type = propertyInfo.PropertyType,
                        Ignored = Attribute.IsDefined(propertyInfo, typeof(JsonIgnoreAttribute))
                    });
                }

                foreach (var fieldInfo in type.GetFields()) {
                    objectMetadata.Properties.Add(fieldInfo.Name, new PropertyMetadata {
                        Info = fieldInfo,
                        IsField = true,
                        Type = fieldInfo.FieldType,
                        Ignored = Attribute.IsDefined(fieldInfo, typeof(JsonIgnoreAttribute))
                    });
                }

                cachedObjectMetadata.Add(type, objectMetadata);
            }

            return cachedObjectMetadata[type];
        }
    }
}
