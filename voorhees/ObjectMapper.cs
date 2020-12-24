using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Voorhees {
	public static class JsonMapper {
        public delegate string ExporterFunc<in T>(T obj);
        
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
        
        /////////////////////////////////////////////////

        struct PropertyMetadata {
            public MemberInfo Info;
            public bool IsField;
        }
        static readonly Dictionary<Type, List<PropertyMetadata>> typeProperties = new Dictionary<Type, List<PropertyMetadata>>();
        
        delegate string ExporterFunc(object obj);
        static readonly IDictionary<Type, ExporterFunc> customSerializers = new Dictionary<Type, ExporterFunc>();
        static readonly IDictionary<Type, ExporterFunc> builtInSerializers = new Dictionary<Type, ExporterFunc>();

        /////////////////////////////////////////////////
        
        static JsonMapper() {
            builtInSerializers[typeof(DateTime)] = obj => 
                "\"" + ((DateTime) obj).ToString("s", DateTimeFormatInfo.InvariantInfo) + "\"";
            builtInSerializers[typeof(DateTimeOffset)] = obj =>
                "\"" + ((DateTimeOffset) obj).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", DateTimeFormatInfo.InvariantInfo) + "\"";
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
    }
}
