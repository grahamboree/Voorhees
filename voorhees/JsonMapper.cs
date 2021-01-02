using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Voorhees {
    public static class JsonMapper {
        public delegate string ExporterFunc<in T>(T objectToSerialize);
        public delegate T ImporterFunc<in TJson, out T>(TJson jsonData);

        /////////////////////////////////////////////////

        public static string ToJson(object obj) {
            switch (obj) {
                case null: return "null";
                case JsonValue jsonValue: return JsonWriter.ToJson(jsonValue);

                // JSON String
                case string stringVal: return "\"" + stringVal + "\"";
                case char charVal: return "\"" + charVal + "\"";

                // JSON Number
                case float floatVal: return floatVal.ToString(CultureInfo.InvariantCulture);
                case double doubleVal: return doubleVal.ToString(CultureInfo.InvariantCulture);
                case decimal decimalVal: return decimalVal.ToString(CultureInfo.InvariantCulture);
                case byte byteVal: return byteVal.ToString();
                case sbyte sbyteVal: return sbyteVal.ToString();
                case int intVal: return intVal.ToString();
                case uint uintVal: return uintVal.ToString();
                case long longVal: return longVal.ToString();
                case ulong ulongVal: return ulongVal.ToString();
                case short shortVal: return shortVal.ToString();
                case ushort ushortVal: return ushortVal.ToString();

                // JSON Boolean
                case bool boolVal: return boolVal ? "true" : "false";

                // JSON Array
                case Array arrayVal: {
                    // Faster code for the common case.
                    if (arrayVal.Rank == 1) {
                        var stringVals = new string[arrayVal.Length];
                        for (int i = 0; i < arrayVal.Length; ++i) {
                            stringVals[i] = ToJson(arrayVal.GetValue(i));
                        }
                        return "[" + string.Join(",", stringVals) + "]";
                    }
                    
                    // Handles arbitrary dimension arrays.
                    int[] index = new int[arrayVal.Rank];
                    string jsonifyArray(Array arr, int currentDimension) {
                        var stringVals = new string[arr.GetLength(currentDimension)];
                        for (int i = 0; i < arr.GetLength(currentDimension); ++i) {
                            index[currentDimension] = i;
                            if (currentDimension == arr.Rank - 1) {
                                stringVals[i] = ToJson(arr.GetValue(index));
                            } else {
                                stringVals[i] = jsonifyArray(arr, currentDimension + 1);
                            }
                        }

                        return "[" + string.Join(",", stringVals) + "]";
                    }

                    return jsonifyArray(arrayVal, 0);
                }
                case IList listVal: {
                    var stringVals = new string[listVal.Count];
                    for (var i = 0; i < listVal.Count; i++) {
                        stringVals[i] = ToJson(listVal[i]);
                    }
                    return "[" + string.Join(",", stringVals) + "]";
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
                        dictBuilder.Append(ToJson(entry.Value));
                    }
                    dictBuilder.Append("}");
                    return dictBuilder.ToString();
                }
            }

            var obj_type = obj.GetType();

            // See if there's a custom exporter for the object
            if (customExporters.TryGetValue(obj_type, out var customExporter)) {
                return customExporter(obj);
            }

            // If not, maybe there's a built-in serializer
            if (builtInExporters.TryGetValue(obj_type, out var builtInExporter)) {
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
                    objectBuilder.Append(ToJson(((FieldInfo) propertyMetadata.Info).GetValue(obj)));
                } else {
                    var propertyInfo = (PropertyInfo) propertyMetadata.Info;

                    if (propertyInfo.CanRead) {
                        objectBuilder.Append("\"" + propertyMetadata.Info.Name + "\":");
                        objectBuilder.Append(ToJson(propertyInfo.GetValue (obj, null)));
                    }
                }
            }
            objectBuilder.Append("}");
            return objectBuilder.ToString();
        }

        public static void RegisterJsonExporter<T>(ExporterFunc<T> exporter) {
            customExporters[typeof(T)] = obj => exporter((T) obj);
        }

        public static void UnRegisterJsonExporter<T>() {
            customExporters.Remove(typeof(T));
        }

        public static void UnRegisterAllJsonExporters() {
            customExporters.Clear();
        }

        public static T FromJson<T>(string jsonString) {
            return (T) FromJson(JsonReader.Read(jsonString), typeof(T));
        }

        public static void RegisterJsonImporter<TJson, TValue>(ImporterFunc<TJson, TValue> importer) {
            RegisterJsonImporter(customImporters, typeof(TJson), typeof(TValue), input => importer((TJson) input));
        }

        /////////////////////////////////////////////////

        struct ObjectMetadata {
            public bool IsDictionary;
            public Dictionary<string, PropertyMetadata> Properties;

            public Type ElementType {
                get => (element_type == null) ? typeof(JsonValue) : element_type;
                set => element_type = value;
            }
            Type element_type;
        }
        static readonly Dictionary<Type, ObjectMetadata> cachedObjectMetadata = new Dictionary<Type, ObjectMetadata>();
        
        struct PropertyMetadata {
            public MemberInfo Info;
            public bool IsField;
            public Type Type;
        }
        static readonly Dictionary<Type, List<PropertyMetadata>> typeProperties = new Dictionary<Type, List<PropertyMetadata>>();

        struct ArrayMetadata {
            public bool IsArray { get; set; }
            public bool IsList { get; set; }

            public Type ElementType {
                get => element_type ?? typeof(JsonValue);
                set => element_type = value;
            }
            Type element_type;
        }
        static readonly Dictionary<Type, ArrayMetadata> cachedArrayMetadata = new Dictionary<Type, ArrayMetadata>();

        static readonly Dictionary<Type, Dictionary<Type, MethodInfo>> implicitConversionOperatorCache = new Dictionary<Type, Dictionary<Type, MethodInfo>>();

        delegate string ExporterFunc(object obj);
        static readonly Dictionary<Type, ExporterFunc> builtInExporters = new Dictionary<Type, ExporterFunc>();
        static readonly Dictionary<Type, ExporterFunc> customExporters = new Dictionary<Type, ExporterFunc>();

        delegate object ImporterFunc(object input);
        static readonly Dictionary<Type, Dictionary<Type, ImporterFunc>> builtInImporters = new Dictionary<Type, Dictionary<Type, ImporterFunc>>();
        static readonly Dictionary<Type, Dictionary<Type, ImporterFunc>> customImporters = new Dictionary<Type, Dictionary<Type, ImporterFunc>>();

        /////////////////////////////////////////////////

        static JsonMapper() {
            builtInExporters[typeof(DateTime)] = obj =>
                "\"" + ((DateTime) obj).ToString("o") + "\"";
            builtInExporters[typeof(DateTimeOffset)] = obj =>
                "\"" + ((DateTimeOffset) obj).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", DateTimeFormatInfo.InvariantInfo) + "\"";
            
            RegisterBaseImporter<int, byte>(Convert.ToByte);
            RegisterBaseImporter<int, sbyte>(Convert.ToSByte);
            RegisterBaseImporter<int, short>(Convert.ToInt16);
            RegisterBaseImporter<int, ushort>(Convert.ToUInt16);
            RegisterBaseImporter<int, uint>(Convert.ToUInt32);
            RegisterBaseImporter<int, long>(Convert.ToInt64);
            RegisterBaseImporter<int, ulong>(Convert.ToUInt64);
            RegisterBaseImporter<int, float>(Convert.ToSingle);
            RegisterBaseImporter<int, double>(Convert.ToDouble);
            RegisterBaseImporter<int, decimal>(Convert.ToDecimal);
            
            RegisterBaseImporter<float, double>(Convert.ToDouble);
            RegisterBaseImporter<float, decimal>(Convert.ToDecimal);
            
            RegisterBaseImporter<string, char>(Convert.ToChar);
            RegisterBaseImporter<string, DateTime>(input => Convert.ToDateTime(input, DateTimeFormatInfo.InvariantInfo));
            RegisterBaseImporter<string, DateTimeOffset>(DateTimeOffset.Parse);
        }

        /// Gather property and field info about the type
        /// Cache it so we don't have to get this info every
        /// time we come across an instance of this type
        static List<PropertyMetadata> GetTypePropertyMetadata(Type type) {
            if (!typeProperties.ContainsKey(type)) {
                var props = new List<PropertyMetadata>();

                foreach (var propertyInfo in type.GetProperties()) {
                    if (propertyInfo.Name != "Item") {
                        props.Add(new PropertyMetadata {
                            Info = propertyInfo,
                            IsField = false
                        });
                    }
                }

                foreach (var fieldInfo in type.GetFields()) {
                    props.Add (new PropertyMetadata {
                        Info = fieldInfo,
                        IsField = true
                    });
                }

                try {
                    typeProperties.Add(type, props);
                } catch (ArgumentException) {
                }
            }

            return typeProperties[type];
        }

        static ArrayMetadata GetCachedArrayMetadata(Type type) {
            if (cachedArrayMetadata.ContainsKey(type)) {
                return cachedArrayMetadata[type];
            }

            var data = new ArrayMetadata {
                IsArray = type.IsArray,
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

        static void RegisterJsonImporter(Dictionary<Type, Dictionary<Type, ImporterFunc>> table, Type json_type, Type value_type, ImporterFunc importer) {
            if (!table.ContainsKey(json_type)) {
                table.Add(json_type, new Dictionary<Type, ImporterFunc>());
            }

            table[json_type][value_type] = importer;
        }
        
        static void RegisterBaseImporter<TJson, TValue>(ImporterFunc<TJson, TValue> importer) {
            RegisterJsonImporter(builtInImporters, typeof(TJson), typeof(TValue), input => importer((TJson) input));
        }
        
        static object FromJson(JsonValue jsonValue, Type destinationType) {
            var underlyingType = Nullable.GetUnderlyingType(destinationType);
            var valueType = underlyingType ?? destinationType;

            switch (jsonValue.Type) {
                case JsonType.Null:
                    if (destinationType.IsClass || underlyingType != null) {
                        return null;
                    }
                    throw new Exception($"Can't assign null to an instance of type {destinationType}");
                case JsonType.Int: return MapValueToType(jsonValue, typeof(int), valueType, destinationType);
                case JsonType.Float: return MapValueToType(jsonValue, typeof(float), valueType, destinationType);
                case JsonType.Boolean: return MapValueToType(jsonValue, typeof(bool), valueType, destinationType);
                case JsonType.String: return MapValueToType(jsonValue, typeof(string), valueType, destinationType);
                case JsonType.Array: {
                    var arrayMetadata = GetCachedArrayMetadata(destinationType);

                    if (!arrayMetadata.IsArray && !arrayMetadata.IsList) {
                        throw new Exception($"Type {destinationType} can't act as an array");
                    }

                    var list = arrayMetadata.IsArray ? new ArrayList() : (IList)Activator.CreateInstance(destinationType);
                    var elementType = arrayMetadata.IsArray ? destinationType.GetElementType() : arrayMetadata.ElementType;

                    list.Clear();
                    foreach (var element in jsonValue) {
                        list.Add(FromJson(element, elementType));
                    }

                    if (arrayMetadata.IsArray) {
                        int n = list.Count;
                        if (elementType == null) {
                            throw new InvalidOperationException("Attempting to map an array but the array element type is null");
                        }
                        var result = Array.CreateInstance(elementType, n);

                        for (int i = 0; i < n; i++) {
                            result.SetValue(list[i], i);
                        }
                        return result;
                    }

                    return list;
                }
                case JsonType.Object: {
                    var objectMetadata = GetObjectMetadata(valueType);

                    var instance = Activator.CreateInstance(valueType);

                    foreach (string property in jsonValue.Keys) {
                        var val = jsonValue[property];

                        if (objectMetadata.Properties.ContainsKey(property)) {
                            var propertyMetadata = objectMetadata.Properties[property];

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

            // If there's a custom importer that fits, use it
            if (customImporters.ContainsKey(jsonType) && customImporters[jsonType].ContainsKey(valueType)) {
                return customImporters[jsonType][valueType](json.Value);
            }

            // Maybe there's a base importer that works
            if (builtInImporters.ContainsKey(jsonType) && builtInImporters[jsonType].ContainsKey(valueType)) {
                return builtInImporters[jsonType][valueType](json.Value);
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

                var objectMetadata = new ObjectMetadata();
                objectMetadata.IsDictionary = isDictionary;
                objectMetadata.Properties = new Dictionary<string, PropertyMetadata>();
                objectMetadata.ElementType = null;

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
                        Type = propertyInfo.PropertyType
                    });
                }

                foreach (var fieldInfo in type.GetFields()) {
                    objectMetadata.Properties.Add(fieldInfo.Name, new PropertyMetadata {
                        Info = fieldInfo,
                        IsField = true,
                        Type = fieldInfo.FieldType
                    });
                }

                cachedObjectMetadata.Add(type, objectMetadata);
            }

            return cachedObjectMetadata[type];
        }
    }
}
