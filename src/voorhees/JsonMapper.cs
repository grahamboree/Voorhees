using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using TypeInfo = Voorhees.Internal.TypeInfo;

namespace Voorhees {
    // Writing
    public static partial class JsonMapper {
        public static string ToJson<T>(T obj, bool prettyPrint = false) {
            var stringBuilder = new StringBuilder();
            using (var writer = new StringWriter(stringBuilder)) {
                WriteValueAsJson(obj, typeof(T), obj?.GetType(), new JsonWriter(writer, prettyPrint));
            }
            return stringBuilder.ToString();
        }

        /////////////////////////////////////////////////

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
                case JsonValue jsonValue: writer.Write(jsonValue); return;

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
                                writer.WriteArrayListTerminator();
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
                            writer.WriteArrayListTerminator();
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
                if (enumType == typeof(sbyte)) { writer.Write((sbyte) obj);  return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(short)) { writer.Write((short) obj);  return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(ushort)) { writer.Write((ushort) obj);  return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(int)) { writer.Write((int) obj);  return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(uint)) { writer.Write((uint) obj);  return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(long)) { writer.Write((long) obj);  return; }
                // ReSharper disable once PossibleInvalidCastException
                if (enumType == typeof(ulong)) { writer.Write((ulong) obj);  return; }

                throw new InvalidOperationException("Unknown underlying enum type: " + enumType);
            }

            writer.WriteObjectStart();

            if (referenceType != valueType) {
                writer.Write("$t");
                writer.WriteObjectKeyValueSeparator();
                writer.Write(valueType.AssemblyQualifiedName);
                writer.WriteArraySeparator();
            }
            
            var fieldsAndProperties = TypeInfo.GetTypePropertyMetadata(valueType);
            
            // TODO This should be instead done in TypeInfo.GetTypePropertyMetadata
            // Remove any non-readable properties from the list
            for (int fieldIndex = 0; fieldIndex < fieldsAndProperties.Count; fieldIndex++) {
                var propertyMetadata = fieldsAndProperties[fieldIndex];
                if (!propertyMetadata.IsField) {
                    var propertyInfo = propertyMetadata.Info as PropertyInfo;
                    if (propertyInfo != null && !propertyInfo.CanRead) {
                        fieldsAndProperties.RemoveAt(fieldIndex);
                        fieldIndex--;                        
                    }
                }
            }

            // Write the object's field and property values
            for (int fieldIndex = 0; fieldIndex < fieldsAndProperties.Count; fieldIndex++) {
                var propertyMetadata = fieldsAndProperties[fieldIndex];
                
                if (propertyMetadata.IsField) {
                    var fieldInfo = (FieldInfo) propertyMetadata.Info;
                    object value = fieldInfo.GetValue(obj);
                    writer.Write(fieldInfo.Name);
                    writer.WriteObjectKeyValueSeparator();
                    WriteValueAsJson(value, fieldInfo.FieldType, value.GetType(), writer);
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
                    writer.WriteArrayListTerminator();
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
                    writer.WriteArrayListTerminator();
                }
            }
            writer.WriteArrayEnd();
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
            var config = Voorhees.Instance;
            if (config.LowLevelCustomImporters.TryGetValue(destinationType, out var lowLevelImporter)) {
                return lowLevelImporter(tokenizer);
            }
            if (config.CustomImporters.TryGetValue(destinationType, out var customImporter)) {
                return customImporter(JsonReader.ReadJsonValue(tokenizer));
            }
            
            // Maybe there's a base importer that works
            if (Voorhees.BuiltInImporters.TryGetValue(destinationType, out var builtInImporter)) {
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
                    var numberString = tokenizer.ConsumeNumber();
                    if (int.TryParse(numberString, out int intVal)) {
                        jsonType = typeof(int);
                        jsonValue = intVal;
                    } else {
                        jsonType = typeof(float);
                        jsonValue = float.Parse(numberString);
                    }
                } break;
                case JsonToken.True: jsonType = typeof(bool); jsonValue = true; tokenizer.SkipToken(JsonToken.True); break;
                case JsonToken.False: jsonType = typeof(bool); jsonValue = false; tokenizer.SkipToken(JsonToken.False); break;
                default: throw new ArgumentOutOfRangeException();
            }

            return MapValueToType(jsonValue, jsonType, valueType, destinationType);
        }
        
        static void ReadList(JsonTokenizer tokenizer, IList list, Type elementType) {
            tokenizer.SkipToken(JsonToken.ArrayStart);
            
            bool expectingValue = false;
            while (tokenizer.NextToken != JsonToken.ArrayEnd) {
                expectingValue = false;
                list.Add(FromJson(tokenizer, elementType));
                if (tokenizer.NextToken == JsonToken.Separator) {
                    expectingValue = true;
                    tokenizer.SkipToken(JsonToken.Separator);
                } else if (tokenizer.NextToken != JsonToken.ArrayEnd) {
                    throw new InvalidJsonException($"{tokenizer.LineColString} Expected end array token or separator");
                }
            }

            if (expectingValue) {
                throw new InvalidJsonException($"{tokenizer.LineColString} Unexpected end array token");
            }

            tokenizer.SkipToken(JsonToken.ArrayEnd);
        }

        static IList ReadMultiList(JsonTokenizer tokenizer, Type elementType, int rank) {
            tokenizer.SkipToken(JsonToken.ArrayStart);

            IList list = new ArrayList();
            
            bool expectingValue = false;
            while (tokenizer.NextToken != JsonToken.ArrayEnd) {
                expectingValue = false;

                list.Add(rank > 1 ? ReadMultiList(tokenizer, elementType, rank - 1) : FromJson(tokenizer, elementType));

                if (tokenizer.NextToken == JsonToken.Separator) {
                    expectingValue = true;
                    tokenizer.SkipToken(JsonToken.Separator);
                } else if (tokenizer.NextToken != JsonToken.ArrayEnd) {
                    throw new InvalidJsonException($"{tokenizer.LineColString} Expected end array token or separator");
                }
            }

            if (expectingValue) {
                throw new InvalidJsonException($"{tokenizer.LineColString} Unexpected end array token");
            }
            
            tokenizer.SkipToken(JsonToken.ArrayEnd);

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
            var valueType = destinationType;
            tokenizer.SkipToken(JsonToken.ObjectStart);

            string firstKey = null;
            if (tokenizer.NextToken == JsonToken.String) {
                firstKey = tokenizer.ConsumeString();
                if (firstKey == "$t") { // Type specifier
                    firstKey = null;
                    
                    tokenizer.SkipToken(JsonToken.KeyValueSeparator);
                    
                    string valueAssemblyQualifiedName = tokenizer.ConsumeString();
                    valueType = Type.GetType(valueAssemblyQualifiedName);
                    if (valueType == null) {
                        throw new InvalidJsonException($"Can't deserialize value of type {valueAssemblyQualifiedName} because a type with that " +
                                                       $"assembly qualified name name cannot be found.");
                    }
                    
                    if (tokenizer.NextToken == JsonToken.Separator) {
                        tokenizer.SkipToken(JsonToken.Separator);
                    }
                }
            }

            var objectMetadata = TypeInfo.GetObjectMetadata(valueType);
            object instance = Activator.CreateInstance(valueType);

            bool expectingValue = false;

            while (tokenizer.NextToken != JsonToken.ObjectEnd) {
                expectingValue = false;
                string propertyName;
                if (firstKey != null) {
                    propertyName = firstKey;
                    firstKey = null;
                } else {
                    propertyName = tokenizer.ConsumeString();
                }
                    
                tokenizer.SkipToken(JsonToken.KeyValueSeparator);
                
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
                    tokenizer.SkipToken(JsonToken.Separator);
                }
            }

            if (expectingValue) {
                throw new InvalidJsonException($"{tokenizer.LineColString} Unexpected object end token");
            }

            tokenizer.SkipToken(JsonToken.ObjectEnd);
            
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
