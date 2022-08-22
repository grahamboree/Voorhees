using System;
using System.Collections.Generic;
using System.Reflection;

namespace Voorhees.Internal {
    public static class TypeInfo {
        /// Gather property and field info about the type
        /// Cache it so we don't have to get this info every
        /// time we come across an instance of this type
        internal static List<PropertyMetadata> GetTypePropertyMetadata(Type type) {
            if (!typeProperties.ContainsKey(type)) {
                var props = new List<PropertyMetadata>();

                foreach (var propertyInfo in type.GetProperties()) {
                    bool serializableProperty =
                        propertyInfo.CanRead && 
                        propertyInfo.Name != "Item" && // Indexer
                        !Attribute.IsDefined(propertyInfo, typeof(JsonIgnoreAttribute)); // Ignored
                    if (serializableProperty) {
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

        internal static ArrayMetadata GetCachedArrayMetadata(Type type) {
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

        internal static MethodInfo GetImplicitConversionOperator(Type t1, Type t2) {
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

        internal static ObjectMetadata GetObjectMetadata(Type type) {
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
        
        /////////////////////////////////////////////////

        internal struct ObjectMetadata {
            public bool IsDictionary;
            public Dictionary<string, PropertyMetadata> Properties;

            public Type ElementType {
                get => element_type ?? typeof(JsonValue);
                set => element_type = value;
            }
            Type element_type;
        }
        static readonly Dictionary<Type, ObjectMetadata> cachedObjectMetadata = new Dictionary<Type, ObjectMetadata>();
        
        internal struct PropertyMetadata {
            public MemberInfo Info;
            public bool IsField;
            public Type Type;
            public bool Ignored;
        }
        static readonly Dictionary<Type, List<PropertyMetadata>> typeProperties = new Dictionary<Type, List<PropertyMetadata>>();

        internal struct ArrayMetadata {
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
    }
}
