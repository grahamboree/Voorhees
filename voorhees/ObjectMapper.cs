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

        public static string Serialize(object obj) {
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
                    var stringVals = new string[arrayVal.Length];
                    int valueIndex = 0;
                    foreach (var elem in arrayVal) {
                        stringVals[valueIndex] = Serialize(elem);
                        valueIndex++;
                    }
                    return "[" + string.Join(",", stringVals) + "]";
                }
                case IList listVal: {
                    var stringVals = new string[listVal.Count];
                    int valueIndex = 0;
                    foreach (var elem in listVal) {
                        stringVals[valueIndex] = Serialize(elem);
                        valueIndex++;
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
                        dictBuilder.Append(Serialize(entry.Value));
                    }
                    dictBuilder.Append("}");
                    return dictBuilder.ToString();
                }
            }

            var obj_type = obj.GetType();

            // See if there's a custom exporter for the object
            if (customSerializers.TryGetValue(obj_type, out var customExporter)) {
                return customExporter(obj);
            }

            // If not, maybe there's a built-in serializer
            if (builtInSerializers.TryGetValue(obj_type, out var builtInExporter)) {
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
                    objectBuilder.Append(Serialize(((FieldInfo) propertyMetadata.Info).GetValue(obj)));
                } else {
                    var propertyInfo = (PropertyInfo) propertyMetadata.Info;

                    if (propertyInfo.CanRead) {
                        objectBuilder.Append("\"" + propertyMetadata.Info.Name + "\":");
                        objectBuilder.Append(Serialize(propertyInfo.GetValue (obj, null)));
                    }
                }
            }
            objectBuilder.Append("}");
            return objectBuilder.ToString();
        }

        public static void RegisterSerializer<T>(ExporterFunc<T> exporter) {
            customSerializers[typeof(T)] = obj => exporter((T) obj);
        }

        public static void UnRegisterSerializer<T>() {
            customSerializers.Remove(typeof(T));
        }

        public static void UnRegisterAllSerializers() {
            customSerializers.Clear();
        }

        public static T UnSerialize<T>(string jsonString) {
            var destinationType = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(destinationType);
            var valueType = underlyingType ?? destinationType;
            
            var jsonValue = JsonReader.Read(jsonString);

            switch (jsonValue.Type) {
                case JsonType.Null:
                    if (destinationType.IsClass || underlyingType != null) {
                        return (T)(object)null;
                    }
                    throw new Exception($"Can't assign null to an instance of type {destinationType}");
                case JsonType.Int: return MapValueToType<T, int>(jsonValue, valueType);
                case JsonType.Float: return MapValueToType<T, float>(jsonValue, valueType);
                case JsonType.Boolean: return MapValueToType<T, bool>(jsonValue, valueType);
                case JsonType.String: return MapValueToType<T, string>(jsonValue, valueType);
                case JsonType.Array:
                    break;
                case JsonType.Object:
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            throw new NotImplementedException();
        }

        public static void RegisterImporter<TJson, TValue>(ImporterFunc<TJson, TValue> importer) {
            RegisterImporter(custom_importers_table, typeof(TJson), typeof(TValue), input => importer((TJson) input));
        }

        /////////////////////////////////////////////////

        struct PropertyMetadata {
            public MemberInfo Info;
            public bool IsField;
        }
        static readonly Dictionary<Type, List<PropertyMetadata>> typeProperties = new Dictionary<Type, List<PropertyMetadata>>();

        static readonly Dictionary<Type, Dictionary<Type, MethodInfo>> implicitConversionOperatorCache = new Dictionary<Type, Dictionary<Type, MethodInfo>>();

        delegate string ExporterFunc(object obj);
        static readonly Dictionary<Type, ExporterFunc> customSerializers = new Dictionary<Type, ExporterFunc>();
        static readonly Dictionary<Type, ExporterFunc> builtInSerializers = new Dictionary<Type, ExporterFunc>();

        delegate object ImporterFunc(object input);
        static readonly Dictionary<Type, Dictionary<Type, ImporterFunc>> base_importers_table = new Dictionary<Type, Dictionary<Type, ImporterFunc>>();
        static readonly Dictionary<Type, Dictionary<Type, ImporterFunc>> custom_importers_table = new Dictionary<Type, Dictionary<Type, ImporterFunc>>();

        /////////////////////////////////////////////////

        static JsonMapper() {
            builtInSerializers[typeof(DateTime)] = obj =>
                "\"" + ((DateTime) obj).ToString("o") + "\"";
            builtInSerializers[typeof(DateTimeOffset)] = obj =>
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

        static void RegisterImporter(Dictionary<Type, Dictionary<Type, ImporterFunc>> table, Type json_type, Type value_type, ImporterFunc importer) {
            if (!table.ContainsKey(json_type)) {
                table.Add(json_type, new Dictionary<Type, ImporterFunc>());
            }

            table[json_type][value_type] = importer;
        }
        
        static void RegisterBaseImporter<TJson, TValue>(ImporterFunc<TJson, TValue> importer) {
            RegisterImporter(base_importers_table, typeof(TJson), typeof(TValue), input => importer((TJson) input));
        }
        
        /// <summary>
        /// Converts a basic json value to an object of the specified type.
        /// </summary>
        /// <param name="json">The json value</param>
        /// <param name="valueType">The underlying storage value type of <typeparamref name="T"/></param>
        /// <typeparam name="T">Type we're converting to</typeparam>
        /// <typeparam name="U">Type of the json data value</typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        static T MapValueToType<T, U>(JsonValue json, Type valueType) {
            var jsonType = typeof(U);
                    
            if (valueType.IsAssignableFrom(jsonType)) {
                return (T)json.Value;
            }

            // If there's a custom importer that fits, use it
            if (custom_importers_table.ContainsKey(jsonType) && custom_importers_table[jsonType].ContainsKey(valueType)) {
                return (T) custom_importers_table[jsonType][valueType](json.Value);
            }

            // Maybe there's a base importer that works
            if (base_importers_table.ContainsKey(jsonType) && base_importers_table[jsonType].ContainsKey(valueType)) {
                return (T) base_importers_table[jsonType][valueType](json.Value);
            }
                    
            // Integral value can be converted to enum values
            if (jsonType == typeof(int) && valueType.IsEnum) {
                return (T)Enum.ToObject(valueType, json.Value);
            }
            
            // Try using an implicit conversion operator
            var implicitConversionOperator = GetImplicitConversionOperator(valueType, jsonType);
            if (implicitConversionOperator != null) {
                return (T)implicitConversionOperator.Invoke(null, new[] {json.Value});
            }

            // No luck
            throw new Exception($"Can't assign value '{JsonWriter.ToJson(json)}' ({jsonType}) to type {typeof(T)}");
        }
    }
}
