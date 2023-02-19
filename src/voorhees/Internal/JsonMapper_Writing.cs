using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using TypeInfo = Voorhees.Internal.TypeInfo;

namespace Voorhees {
    public partial class JsonMapper {
        void WriteValue(object obj, Type referenceType, Type valueType, JsonTokenWriter tokenWriter) {
            if (obj == null) {
                tokenWriter.WriteNull();
                return;
            }

            // See if there's a custom exporter for the object
            if (exporters.TryGetValue(referenceType, out var customExporter)) {
                customExporter(obj, tokenWriter);
                return;
            }
            
            // Special case built-in serializer for DateTime
            if (referenceType == typeof(DateTime)) {
                tokenWriter.Write(((DateTime) obj).ToString("o"));
                return;
            }
            
            // Special case built-in serializer for DateTimeOffset
            if (referenceType == typeof(DateTimeOffset)) {
                tokenWriter.Write(((DateTimeOffset) obj).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", DateTimeFormatInfo.InvariantInfo));
                return;
            }

            switch (obj) { 
                case JsonValue jsonValue: WriteJsonValue(jsonValue, tokenWriter); return;

                // JSON String
                case string stringVal: tokenWriter.Write(stringVal); return;
                case char charVal: tokenWriter.Write(charVal); return;

                // JSON Number
                case byte byteVal: tokenWriter.Write(byteVal); return;
                case sbyte sbyteVal: tokenWriter.Write(sbyteVal); return;
                case short shortVal: tokenWriter.Write(shortVal); return;
                case ushort ushortVal: tokenWriter.Write(ushortVal); return;
                case int intVal: tokenWriter.Write(intVal); return;
                case uint uintVal: tokenWriter.Write(uintVal); return;
                case long longVal: tokenWriter.Write(longVal); return;
                case ulong ulongVal: tokenWriter.Write(ulongVal); return;
                case float floatVal: tokenWriter.Write(floatVal); return;
                case double doubleVal: tokenWriter.Write(doubleVal); return;
                case decimal decimalVal: tokenWriter.Write(decimalVal); return;

                // JSON Boolean
                case bool boolVal: tokenWriter.Write(boolVal); return;

                // JSON Array
                case Array arrayVal: {
                    // Faster code for the common case.
                    if (arrayVal.Rank == 1) {
                        Write1DArray(arrayVal, tokenWriter);
                        return;
                    }
                    
                    // Handles arbitrary dimension arrays.
                    int[] index = new int[arrayVal.Rank];

                    void JsonifyArray(Array arr, int currentDimension) {
                        tokenWriter.WriteArrayStart();

                        int length = arr.GetLength(currentDimension);
                        for (int i = 0; i < length; ++i) {
                            index[currentDimension] = i;

                            if (currentDimension == arr.Rank - 1) {
                                object arrayObject = arr.GetValue(index);
                                WriteValue(arrayObject, arr.GetType().GetElementType(), arrayObject.GetType(), tokenWriter);
                            } else {
                                JsonifyArray(arr, currentDimension + 1);
                            }
                            
                            if (i < length - 1) {
                                tokenWriter.WriteArrayOrObjectSeparator();
                            } else {
                                tokenWriter.WriteArrayOrObjectBodyTerminator();
                            }
                        }

                        tokenWriter.WriteArrayEnd();
                    }
                    JsonifyArray(arrayVal, 0);
                    return;
                }
                case IList listVal: Write1DArray(listVal, tokenWriter); return;

                // JSON Object
                case IDictionary dictionary: {
                    tokenWriter.WriteObjectStart();

                    int entryIndex = 0;
                    int length = dictionary.Count;
                    
                    foreach (DictionaryEntry entry in dictionary) {
                        string propertyName = entry.Key as string ?? Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                        tokenWriter.WriteObjectKey(propertyName);
                        object value = entry.Value;
                        WriteValue(value, value.GetType(), value.GetType(), tokenWriter);
                        
                        if (entryIndex < length - 1) {
                            tokenWriter.WriteArrayOrObjectSeparator();
                        } else {
                            tokenWriter.WriteArrayOrObjectBodyTerminator();
                        }
                        entryIndex++;
                    }

                    tokenWriter.WriteObjectEnd();
                    return;
                }
            }

            if (obj is Enum) {
                var enumType = Enum.GetUnderlyingType(valueType);
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(byte)) { tokenWriter.Write((byte) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(sbyte)) { tokenWriter.Write((sbyte) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(short)) { tokenWriter.Write((short) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(ushort)) { tokenWriter.Write((ushort) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(int)) { tokenWriter.Write((int) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(uint)) { tokenWriter.Write((uint) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(long)) { tokenWriter.Write((long) obj); return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(ulong)) { tokenWriter.Write((ulong) obj); return; }
            }

            tokenWriter.WriteObjectStart();

            if (referenceType != valueType) {
                tokenWriter.WriteObjectKey("$t");
                tokenWriter.Write(valueType.AssemblyQualifiedName);
                tokenWriter.WriteArrayOrObjectSeparator();
            }
            
            var fieldsAndProperties = TypeInfo.GetTypePropertyMetadata(valueType);

            // Write the object's field and property values
            for (int fieldIndex = 0; fieldIndex < fieldsAndProperties.Count; fieldIndex++) {
                var propertyMetadata = fieldsAndProperties[fieldIndex];
                
                if (propertyMetadata.IsField) {
                    var fieldInfo = (FieldInfo) propertyMetadata.Info;
                    object value = fieldInfo.GetValue(obj);
                    tokenWriter.WriteObjectKey(fieldInfo.Name);
                    WriteValue(value, fieldInfo.FieldType, value != null ? value.GetType() : fieldInfo.FieldType, tokenWriter);
                } else {
                    var propertyInfo = (PropertyInfo) propertyMetadata.Info;
                    object value = propertyInfo.GetValue(obj);
                    tokenWriter.WriteObjectKey(propertyInfo.Name);
                    WriteValue(value, propertyInfo.PropertyType, value.GetType(), tokenWriter);
                }

                if (fieldIndex < fieldsAndProperties.Count - 1) {
                    tokenWriter.WriteArrayOrObjectSeparator();
                } else {
                    tokenWriter.WriteArrayOrObjectBodyTerminator();
                }
            }

            tokenWriter.WriteObjectEnd();
        }

        void Write1DArray(IList list, JsonTokenWriter tokenWriter) {
            tokenWriter.WriteArrayStart();
            for (int i = 0; i < list.Count; i++) {
                object listVal = list[i];
                WriteValue(listVal, listVal.GetType(), listVal.GetType(), tokenWriter);

                if (i < list.Count - 1) {
                    tokenWriter.WriteArrayOrObjectSeparator();
                } else {
                    tokenWriter.WriteArrayOrObjectBodyTerminator();
                }
            }
            tokenWriter.WriteArrayEnd();
        }

        static void WriteJsonValue(JsonValue val, JsonTokenWriter tokenWriter) {
            if (val == null) {
                tokenWriter.WriteNull();
                return;
            }

            switch (val.Type) {
                case JsonValueType.Int:     tokenWriter.Write((int) val); break;
                case JsonValueType.Double:   tokenWriter.Write((double) val); break;
                case JsonValueType.Boolean: tokenWriter.Write((bool) val); break;
                case JsonValueType.String:  tokenWriter.Write((string) val); break;
                case JsonValueType.Null:    tokenWriter.WriteNull(); break;
                case JsonValueType.Array: {
                    tokenWriter.WriteArrayStart();

                    for (int i = 0; i < val.Count; ++i) {
                        WriteJsonValue(val[i], tokenWriter);

                        if (i < val.Count - 1) {
                            tokenWriter.WriteArrayOrObjectSeparator();
                        } else {
                            tokenWriter.WriteArrayOrObjectBodyTerminator();
                        }
                    }

                    tokenWriter.WriteArrayEnd();
                } break;
                case JsonValueType.Object: {
                    tokenWriter.WriteObjectStart();

                    bool first = true;
                    foreach (var objectPair in (IEnumerable<KeyValuePair<string, JsonValue>>)val) {
                        if (!first) {
                            tokenWriter.WriteArrayOrObjectSeparator();
                        }
                        first = false;

                        tokenWriter.WriteObjectKey(objectPair.Key);
                        WriteJsonValue(objectPair.Value, tokenWriter);
                    }

                    if (val.Count > 0) {
                        tokenWriter.WriteArrayOrObjectBodyTerminator();
                    }

                    tokenWriter.WriteObjectEnd();
                } break;
                case JsonValueType.Unspecified: 
                default:
                    throw new InvalidOperationException("Can't write JsonValue instance because it is of unspecified type");
            }
        }
    }
}
