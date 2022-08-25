using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using TypeInfo = Voorhees.Internal.TypeInfo;

namespace Voorhees {
    public static partial class JsonMapper {
        static void WriteValueAsJson(object obj, Type referenceType, Type valueType, JsonWriter writer) {
            if (obj == null) {
                writer.WriteNull();
                return;
            }

            // See if there's a custom exporter for the object
            if (Voorhees.Instance.CustomExporters.TryGetValue(referenceType, out var customExporter)) {
                customExporter(obj, writer);
                return;
            }

            // If not, maybe there's a built-in serializer
            if (Voorhees.BuiltInExporters.TryGetValue(referenceType, out var builtInExporter)) {
                builtInExporter(obj, writer);
                return;
            }
            
            switch (obj) { 
                case JsonValue jsonValue: WriteJsonValueAsJson(jsonValue, writer); return;

                // JSON String
                case string stringVal: writer.Write(stringVal); return;
                case char charVal: writer.Write(charVal); return;

                // JSON Number
                case byte byteVal: writer.Write(byteVal); return;
                case sbyte sbyteVal: writer.Write(sbyteVal); return;
                case short shortVal: writer.Write(shortVal); return;
                case ushort ushortVal: writer.Write(ushortVal); return;
                case int intVal: writer.Write(intVal); return;
                case uint uintVal: writer.Write(uintVal); return;
                case long longVal: writer.Write(longVal); return;
                case ulong ulongVal: writer.Write(ulongVal); return;
                case float floatVal: writer.Write(floatVal); return;
                case double doubleVal: writer.Write(doubleVal); return;
                case decimal decimalVal: writer.Write(decimalVal); return;

                // JSON Boolean
                case bool boolVal: writer.Write(boolVal); return;

                // JSON Array
                case Array arrayVal: {
                    // Faster code for the common case.
                    if (arrayVal.Rank == 1) {
                        Write1DArrayAsJson(arrayVal, writer);
                        return;
                    }
                    
                    // Handles arbitrary dimension arrays.
                    int[] index = new int[arrayVal.Rank];

                    void JsonifyArray(Array arr, int currentDimension) {
                        writer.WriteArrayStart();

                        int length = arr.GetLength(currentDimension);
                        for (int i = 0; i < length; ++i) {
                            index[currentDimension] = i;

                            if (currentDimension == arr.Rank - 1) {
                                object arrayObject = arr.GetValue(index);
                                WriteValueAsJson(arrayObject, arr.GetType().GetElementType(), arrayObject.GetType(), writer);
                            } else {
                                JsonifyArray(arr, currentDimension + 1);
                            }
                            
                            if (i < length - 1) {
                                writer.WriteArraySeparator();
                            } else {
                                writer.WriteArrayOrObjectBodyTerminator();
                            }
                        }

                        writer.WriteArrayEnd();
                    }
                    JsonifyArray(arrayVal, 0);
                    return;
                }
                case IList listVal: Write1DArrayAsJson(listVal, writer); return;

                // JSON Object
                case IDictionary dictionary: {
                    writer.WriteObjectStart();

                    int entryIndex = 0;
                    int length = dictionary.Count;
                    
                    foreach (DictionaryEntry entry in dictionary) {
                        string propertyName = entry.Key as string ?? Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                        writer.Write(propertyName);
                        writer.WriteObjectKeyValueSeparator();
                        object value = entry.Value;
                        WriteValueAsJson(value, value.GetType(), value.GetType(), writer);
                        
                        if (entryIndex < length - 1) {
                            writer.WriteArraySeparator();
                        } else {
                            writer.WriteArrayOrObjectBodyTerminator();
                        }
                        entryIndex++;
                    }

                    writer.WriteObjectEnd();
                    return;
                }
            }

            if (obj is Enum) {
                var enumType = Enum.GetUnderlyingType(valueType);
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(byte)) { writer.Write((byte) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(sbyte)) { writer.Write((sbyte) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(short)) { writer.Write((short) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(ushort)) { writer.Write((ushort) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(int)) { writer.Write((int) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(uint)) { writer.Write((uint) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(long)) { writer.Write((long) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(ulong)) { writer.Write((ulong) obj); return; }
            }

            writer.WriteObjectStart();

            if (referenceType != valueType) {
                writer.Write("$t");
                writer.WriteObjectKeyValueSeparator();
                writer.Write(valueType.AssemblyQualifiedName);
                writer.WriteArraySeparator();
            }
            
            var fieldsAndProperties = TypeInfo.GetTypePropertyMetadata(valueType);

            // Write the object's field and property values
            for (int fieldIndex = 0; fieldIndex < fieldsAndProperties.Count; fieldIndex++) {
                var propertyMetadata = fieldsAndProperties[fieldIndex];
                
                if (propertyMetadata.IsField) {
                    var fieldInfo = (FieldInfo) propertyMetadata.Info;
                    object value = fieldInfo.GetValue(obj);
                    writer.Write(fieldInfo.Name);
                    writer.WriteObjectKeyValueSeparator();
                    WriteValueAsJson(value, fieldInfo.FieldType, value != null ? value.GetType() : fieldInfo.FieldType, writer);
                } else {
                    var propertyInfo = (PropertyInfo) propertyMetadata.Info;
                    object value = propertyInfo.GetValue(obj);
                    writer.Write(propertyInfo.Name);
                    writer.WriteObjectKeyValueSeparator();
                    WriteValueAsJson(value, propertyInfo.PropertyType, value.GetType(), writer);
                }

                if (fieldIndex < fieldsAndProperties.Count - 1) {
                    writer.WriteArraySeparator();
                } else {
                    writer.WriteArrayOrObjectBodyTerminator();
                }
            }

            writer.WriteObjectEnd();
        }

        static void Write1DArrayAsJson(IList list, JsonWriter writer) {
            writer.WriteArrayStart();
            for (int i = 0; i < list.Count; i++) {
                object listVal = list[i];
                WriteValueAsJson(listVal, listVal.GetType(), listVal.GetType(), writer);

                if (i < list.Count - 1) {
                    writer.WriteArraySeparator();
                } else {
                    writer.WriteArrayOrObjectBodyTerminator();
                }
            }
            writer.WriteArrayEnd();
        }

        static void WriteJsonValueAsJson(JsonValue val, JsonWriter writer) {
            if (val == null) {
                writer.WriteNull();
                return;
            }

            switch (val.Type) {
                case JsonType.Int:     writer.Write((int) val); break;
                case JsonType.Float:   writer.Write((float) val); break;
                case JsonType.Boolean: writer.Write((bool) val); break;
                case JsonType.String:  writer.Write((string) val); break;
                case JsonType.Null:    writer.WriteNull(); break;
                case JsonType.Array: {
                    writer.WriteArrayStart();

                    for (int i = 0; i < val.Count; ++i) {
                        WriteJsonValueAsJson(val[i], writer);

                        if (i < val.Count - 1) {
                            writer.WriteArraySeparator();
                        } else {
                            writer.WriteArrayOrObjectBodyTerminator();
                        }
                    }

                    writer.WriteArrayEnd();
                } break;
                case JsonType.Object: {
                    writer.WriteObjectStart();

                    bool first = true;
                    foreach (var objectPair in val as IEnumerable<KeyValuePair<string, JsonValue>>) {
                        if (!first) {
                            writer.WriteArraySeparator();
                        }
                        first = false;

                        writer.Write(objectPair.Key);
                        writer.WriteObjectKeyValueSeparator();
                        WriteJsonValueAsJson(objectPair.Value, writer);
                    }

                    if (val.Count > 0) {
                        writer.WriteArrayOrObjectBodyTerminator();
                    }

                    writer.WriteObjectEnd();
                } break;
                case JsonType.Unspecified: 
                default:
                    throw new InvalidOperationException("Can't write JsonValue instance because it is of unspecified type");
            }
        }
    }
}
