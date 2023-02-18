using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Voorhees.Tests {
    [TestFixture]
    public class JsonMapper_Read_Null {
        class TestReferenceType {
        }

        struct TestValueType {
        }

        [Test]
        public void ReadNull_ReferenceType() {
            using var json = new StringReader("null");
            Assert.That(JsonMapper.FromJson<TestReferenceType>(json), Is.Null);
        }

        [Test]
        public void ReadNull_ValueType() {
            // Value types cannot be null.
            using var json = new StringReader("null");
            Assert.Throws<InvalidCastException>(() => JsonMapper.FromJson<TestValueType>(json));
            Assert.Throws<InvalidOperationException>(() => JsonMapper.FromJson<int>(json));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_Int {
        [Test]
        public void Basic() {
            using var json = new StringReader("3");
            Assert.That(JsonMapper.FromJson<int>(json), Is.EqualTo(3));
        }

        class ClassWithImplicitConversionOperator {
            public int intVal;

            public static implicit operator ClassWithImplicitConversionOperator(int data) {
                return new ClassWithImplicitConversionOperator {intVal = data};
            }
        }

        [Test]
        public void ImplicitOperator() {
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>(json), Is.Not.Null);
            }
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>(json).intVal, Is.EqualTo(3));
            }
        }

        [Test]
        public void ReadByte() {
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<byte>(json), Is.TypeOf<byte>());                
            }
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<byte>(json), Is.EqualTo((byte)3));
            }
        }

        [Test]
        public void ReadSByte() {
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<sbyte>(json), Is.TypeOf<sbyte>());
            }
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<sbyte>(json), Is.EqualTo((sbyte)3));
            }
        }

        [Test]
        public void ReadShort() {
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<short>(json), Is.TypeOf<short>());
            }
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<short>(json), Is.EqualTo((short)3));
            }
        }

        [Test]
        public void ReadUShort() {
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<ushort>(json), Is.TypeOf<ushort>());
            }
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<ushort>(json), Is.EqualTo((ushort)3));
            }
        }

        [Test]
        public void ReadUInt() {
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<uint>(json), Is.TypeOf<uint>());
            }
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<uint>(json), Is.EqualTo((uint)3));
            }
        }

        [Test]
        public void ReadLong() {
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<long>(json), Is.TypeOf<long>());
            }
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<long>(json), Is.EqualTo((long)3));
            }
        }

        [Test]
        public void ReadULong() {
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<ulong>(json), Is.TypeOf<ulong>());
            }
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<ulong>(json), Is.EqualTo((ulong)3));
            }
        }

        [Test]
        public void ReadFloat() {
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<float>(json), Is.TypeOf<float>());
            }
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<float>(json), Is.EqualTo((float)3));
            }
        }

        [Test]
        public void ReadDouble() {
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<double>(json), Is.TypeOf<double>());
            }
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<double>(json), Is.EqualTo((double)3));
            }
        }

        [Test]
        public void ReadDecimal() {
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<decimal>(json), Is.TypeOf<decimal>());
            }
            using (var json = new StringReader("3")) {
                Assert.That(JsonMapper.FromJson<decimal>(json), Is.EqualTo((decimal)3));
            }
        }
    }

    [TestFixture]
    public class JsonMapper_Read_Bool {
        [Test]
        public void True() {
            using var json = new StringReader("true");
            Assert.That(JsonMapper.FromJson<bool>(json), Is.True);
        }
        
        [Test]
        public void False() {
            using var json = new StringReader("false");
            Assert.That(JsonMapper.FromJson<bool>(json), Is.False);
        }
    }
    
    [TestFixture]
    public class JsonMapper_Read_Double {
        class ClassWithImplicitConversionOperator {
            public double doubleVal;

            public static implicit operator ClassWithImplicitConversionOperator(double data) {
                return new ClassWithImplicitConversionOperator {doubleVal = data};
            }
        }

        [Test]
        public void ImplicitOperator() {
            using (var json = new StringReader("3.5")) {
                Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>(json), Is.Not.Null);
            }

            using (var json = new StringReader("3.5")) {
                Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>(json).doubleVal, Is.EqualTo(3.5));
            }
        }
        
        [Test]
        public void ReadFloat() {
            using (var json = new StringReader("3.5")) {
                Assert.That(JsonMapper.FromJson<float>(json), Is.TypeOf<float>());
            }
            using (var json = new StringReader("3.5")) {
                Assert.That(JsonMapper.FromJson<float>(json), Is.EqualTo(3.5f));
            }
        }

        [Test]
        public void ReadDouble() {
            using (var json = new StringReader("3.5")) {
                Assert.That(JsonMapper.FromJson<double>(json), Is.TypeOf<double>());
            }
            using (var json = new StringReader("3.5")) {
                Assert.That(JsonMapper.FromJson<double>(json), Is.EqualTo(3.5));
            }
        }

        [Test]
        public void ReadDecimal() {
            using (var json = new StringReader("3.5")) {
                Assert.That(JsonMapper.FromJson<decimal>(json), Is.TypeOf<decimal>());
            }
            using (var json = new StringReader("3.5")) {
                Assert.That(JsonMapper.FromJson<decimal>(json), Is.EqualTo(3.5m));
            }
        }
    }

    [TestFixture]
    public class JsonMapper_Read_String {
        [Test]
        public void ReadFloat() {
            using var json = new StringReader("\"test\"");
            Assert.That(JsonMapper.FromJson<string>(json), Is.EqualTo("test"));
        }

        class ClassWithImplicitConversionOperator {
            public string stringVal;

            public static implicit operator ClassWithImplicitConversionOperator(string data) {
                return new ClassWithImplicitConversionOperator {stringVal = data};
            }
        }

        [Test]
        public void ImplicitOperator() {
            using (var json = new StringReader("\"test\"")) {
                Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>(json), Is.Not.Null);
            }
            using (var json = new StringReader("\"test\"")) {
                Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>(json), Is.TypeOf<ClassWithImplicitConversionOperator>());
            }
            using (var json = new StringReader("\"test\"")) {
                Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>(json).stringVal, Is.EqualTo("test"));
            }
        }

        [Test]
        public void ReadChar() {
            Assert.Multiple(() => {
                using (var json = new StringReader("\"c\"")) {
                    Assert.That(JsonMapper.FromJson<char>(json), Is.TypeOf<char>());                
                }
                using (var json = new StringReader("\"c\"")) {
                    Assert.That(JsonMapper.FromJson<char>(json), Is.EqualTo('c'));
                }
            });
        }

        [Test]
        public void ReadCharTooLong() {
            using var json = new StringReader("\"test\"");
            Assert.Throws<FormatException>(() => JsonMapper.FromJson<char>(json));
        }

        [Test]
        public void ReadDateTime() {
            using var json = new StringReader("\"1970-01-02T03:04:05.0060000\"");
            var dateTime = new DateTime(1970, 1, 2, 3, 4, 5, 6);
            Assert.That(JsonMapper.FromJson<DateTime>(json), Is.EqualTo(dateTime));
        }

        [Test]
        public void ReadInvalidDateTime() {
            using var json = new StringReader("\"test\"");
            Assert.Throws<FormatException>(() => JsonMapper.FromJson<DateTime>(json));
        }

        [Test]
        public void ReadDateTimeOffset() {
            var span = new TimeSpan(0, -5, 0, 0, 0);
            var offset = new DateTimeOffset(1970, 1, 2, 3, 4, 5, 6, span);
            using var json = new StringReader("\"1970-01-02T03:04:05.0060000-05:00\"");
            Assert.That(JsonMapper.FromJson<DateTimeOffset>(json), Is.EqualTo(offset));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_Array {
        [Test]
        public void IntArray() {
            using var json = new StringReader("[1, 2, 3]");
            Assert.That(JsonMapper.FromJson<int[]>(json), Is.EqualTo(new[] {1, 2, 3}));
        }

        class IntOrString {
            public int? intValue;
            public string stringValue;

            public static implicit operator IntOrString(string data) {
                return new IntOrString {stringValue = data};
            }

            public static implicit operator IntOrString(int data) {
                return new IntOrString {intValue = data};
            }

            public override bool Equals(object obj) {
                return obj is IntOrString other && Equals(other);
            }

            bool Equals(IntOrString other) {
                return intValue == other.intValue && stringValue == other.stringValue;
            }

            public override int GetHashCode() {
                // autogenerated
                unchecked {
                    return (intValue.GetHashCode() * 397) ^
                           (stringValue != null ? stringValue.GetHashCode() : 0);
                }
            }
        }

        [Test]
        public void MixedTypeArray() {
            var expected = new[] {
                new IntOrString {intValue = 1},
                new IntOrString {intValue = 2},
                new IntOrString {stringValue = "three"}
            };

            using var json = new StringReader("[1, 2, \"three\"]");
            Assert.That(JsonMapper.FromJson<IntOrString[]>(json), Is.EqualTo(expected));
        }

        [Test]
        public void ReadArrayToList() {
            using var json = new StringReader("[1, 2, 3]");
            var result = JsonMapper.FromJson<List<int>>(json);
            Assert.Multiple(() => {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Count, Is.EqualTo(3));
                Assert.That(result[0], Is.EqualTo(1));
                Assert.That(result[1], Is.EqualTo(2));
                Assert.That(result[2], Is.EqualTo(3));
            });
        }

        [Test]
        public void ReadArrayToNonArrayTypeThrows() {
            using var json = new StringReader("[1,2,3]");
            Assert.Throws<InvalidCastException>(() => JsonMapper.FromJson<string>(json));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_MultiDimensionalArray {
        [Test]
        public void EmptyJaggedArray() {
            using var json = new StringReader("[]");
            Assert.That(JsonMapper.FromJson<int[][]>(json), Is.EqualTo(Array.Empty<int[]>()));
        }

        [Test]
        public void EmptyMultiArray() {
            using var json = new StringReader("[]");
            Assert.That(JsonMapper.FromJson<int[,]>(json), Is.EqualTo(new int[,] { }));
        }

        [Test]
        public void JaggedArray() {
            using var json = new StringReader("[[1, 2], [3, 4]]");
            Assert.That(JsonMapper.FromJson<int[][]>(json), Is.EqualTo(new[] {new[] {1, 2}, new[] {3, 4}}));
        }

        [Test]
        public void MultiArray() {
            using var json = new StringReader("[[1, 2], [3, 4]]");
            Assert.That(JsonMapper.FromJson<int[,]>(json), Is.EqualTo(new[,] {{1, 2}, {3, 4}}));
        }

        [Test]
        public void LargeMultiArray() {
            int[,,,] multi = {
                {
                    {
                        {1, 2},
                        {3, 4}
                    }, {
                        {5, 6},
                        {7, 8}
                    }
                }, {
                    {
                        {9, 10},
                        {11, 12}
                    }, {
                        {13, 14},
                        {15, 16}
                    }
                }
            };

            using var json = new StringReader(
                "[[" +
                    "[[1,2],[3,4]]," +
                    "[[5,6],[7,8]]" +
                "],[" +
                    "[[9,10],[11,12]]," +
                    "[[13,14],[15,16]]" +
                "]]");
            Assert.That(JsonMapper.FromJson<int[,,,]>(json), Is.EqualTo(multi));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_Object {
        class ObjectWithFields {
            public int publicField = -1;
            public int publicField2 = -1;
        }

        [Test]
        public void PublicFields() {
            Assert.Multiple(() => {
                using (var json = new StringReader("{\"publicField\": 3, \"publicField2\": 5}")) {
                    Assert.That(JsonMapper.FromJson<ObjectWithFields>(json), Is.Not.Null);
                }
                using (var json = new StringReader("{\"publicField\": 3, \"publicField2\": 5}")) {
                    Assert.That(JsonMapper.FromJson<ObjectWithFields>(json).publicField, Is.EqualTo(3));
                }
                using (var json = new StringReader("{\"publicField\": 3, \"publicField2\": 5}")) {
                    Assert.That(JsonMapper.FromJson<ObjectWithFields>(json).publicField2, Is.EqualTo(5));
                }
            });
        }

        class ObjectWithProperties {
            public int publicProperty { get; set; } = -1;
            public int publicProperty2 { get; set; } = -1;
        }

        [Test]
        public void PublicProperties() {
            Assert.Multiple(() => {
                using (var json = new StringReader("{\"publicProperty\": 3, \"publicProperty2\": 5}")) {
                    Assert.That(JsonMapper.FromJson<ObjectWithProperties>(json), Is.Not.Null);
                }
                using (var json = new StringReader("{\"publicProperty\": 3, \"publicProperty2\": 5}")) {
                    Assert.That(JsonMapper.FromJson<ObjectWithProperties>(json).publicProperty, Is.EqualTo(3));
                }
                using (var json = new StringReader("{\"publicProperty\": 3, \"publicProperty2\": 5}")) {
                    Assert.That(JsonMapper.FromJson<ObjectWithProperties>(json).publicProperty2, Is.EqualTo(5));
                }
            });
        }

        [Test]
        public void PublicDictionary() {
            
            Assert.Multiple(() => {
                using (var json = new StringReader("{\"a\": 3, \"b\": 5}")) {
                    Assert.That(JsonMapper.FromJson<Dictionary<string, int>>(json), Is.Not.Null);
                }
                using (var json = new StringReader("{\"a\": 3, \"b\": 5}")) {
                    Assert.That(JsonMapper.FromJson<Dictionary<string, int>>(json), Has.Count.EqualTo(2));
                }
                using (var json = new StringReader("{\"a\": 3, \"b\": 5}")) {
                    Assert.That(JsonMapper.FromJson<Dictionary<string, int>>(json)["a"], Is.EqualTo(3));
                }
                using (var json = new StringReader("{\"a\": 3, \"b\": 5}")) {
                    Assert.That(JsonMapper.FromJson<Dictionary<string, int>>(json)["b"], Is.EqualTo(5));
                }
            });
        }

        class ObjectWithReadOnlyProperties {
            public int publicProperty { get; } = -1;
            public int publicProperty2 { get; } = -1;
        }

        [Test]
        public void PublicReadOnlyProperties() {
            using var json = new StringReader("{\"publicProperty\": 3, \"publicProperty2\": 5}");
            Assert.Throws<InvalidOperationException>(() => JsonMapper.FromJson<ObjectWithReadOnlyProperties>(json));
        }

        [Test]
        public void ThrowsIfAssemblyQualifiedNameDoesNotExist() {
            const string BOGUS_AQN = "TopNamespace.SubNameSpace.ContainingClass+NestedClass, MyAssembly, Version=1.3.0.0, Culture=neutral, PublicKeyToken=b17a5c561934e089";
            using var json = new StringReader("{\"$t\"=\"" + BOGUS_AQN + "\",\"foo\":\"bar\"}");
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<object>(json));
        }

        [Test]
        public void MappingObjectWithExtraKeyThrows() {
            using var json = new StringReader("{\"publicField\":1,\"publicField2\":2,\"publicField3\":3}");
            Assert.Throws<InvalidOperationException>(() => JsonMapper.FromJson<ObjectWithFields>(json));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_PolymorphicObjectReference {
        class BaseClass {
#pragma warning disable CS0649
            public int baseClassValue;
#pragma warning restore CS0649
        }

        class DerivedClass : BaseClass {
#pragma warning disable CS0649
            public bool derivedClassValue;
#pragma warning restore CS0649
        }

        [Test]
        public void PolymorphicTypeReference() {
            Assert.Multiple(() => {
                using (var json = new StringReader("{\"derivedClassValue\":false,\"baseClassValue\":2}")) {
                    Assert.That(JsonMapper.FromJson<DerivedClass>(json), Is.Not.Null);
                }
                using (var json = new StringReader("{\"derivedClassValue\":false,\"baseClassValue\":2}")) {
                    Assert.That(JsonMapper.FromJson<DerivedClass>(json).derivedClassValue, Is.False);
                }
                using (var json = new StringReader("{\"derivedClassValue\":false,\"baseClassValue\":2}")) {
                    Assert.That(JsonMapper.FromJson<DerivedClass>(json).baseClassValue, Is.EqualTo(2));
                }
            });
        }

        [Test]
        public void DerivedValueInBaseReference() {
            Assert.Multiple(() => {
                using (var json = new StringReader($"{{\"$t\":\"{typeof(DerivedClass).AssemblyQualifiedName}\",\"derivedClassValue\":false,\"baseClassValue\":2}}")) {
                    Assert.That(JsonMapper.FromJson<BaseClass>(json), Is.Not.Null);
                }
                using (var json = new StringReader($"{{\"$t\":\"{typeof(DerivedClass).AssemblyQualifiedName}\",\"derivedClassValue\":false,\"baseClassValue\":2}}")) {
                    Assert.That(JsonMapper.FromJson<BaseClass>(json), Is.TypeOf<DerivedClass>());
                }
                using (var json = new StringReader($"{{\"$t\":\"{typeof(DerivedClass).AssemblyQualifiedName}\",\"derivedClassValue\":false,\"baseClassValue\":2}}")) {
                    Assert.That(JsonMapper.FromJson<BaseClass>(json).baseClassValue, Is.EqualTo(2));
                }
                using (var json = new StringReader($"{{\"$t\":\"{typeof(DerivedClass).AssemblyQualifiedName}\",\"derivedClassValue\":false,\"baseClassValue\":2}}")) {
                    Assert.That(((DerivedClass)JsonMapper.FromJson<BaseClass>(json)).derivedClassValue, Is.False);
                }
            });
        }
    }

    [TestFixture]
    public class JsonMapper_Read_ReadOrWriteOnlyProperties {
        class ObjectWithFields {
            public int ComputedProperty => ReadOnlyProperty + WriteOnlyProperty;
            public int ReadOnlyProperty { get; private set; } = 1;
            public int WriteOnlyProperty { private get; set; } = 1;

            public int GetWriteOnlyValue() {
                return WriteOnlyProperty;
            }
        }
        
        [Test]
        public void JsonContainsReadOnlyAndWriteOnlyProperties() {
            Assert.Multiple(() => {
                using (var json = new StringReader("{\"ReadOnlyProperty\": 5, \"WriteOnlyProperty\": 3}")) {
                    Assert.That(JsonMapper.FromJson<ObjectWithFields>(json), Is.Not.Null);
                }
                using (var json = new StringReader("{\"ReadOnlyProperty\": 5, \"WriteOnlyProperty\": 3}")) {
                    Assert.That(JsonMapper.FromJson<ObjectWithFields>(json).ReadOnlyProperty, Is.EqualTo(5));
                }
                using (var json = new StringReader("{\"ReadOnlyProperty\": 5, \"WriteOnlyProperty\": 3}")) {
                    Assert.That(JsonMapper.FromJson<ObjectWithFields>(json).GetWriteOnlyValue(), Is.EqualTo(3));
                }
                using (var json = new StringReader("{\"ReadOnlyProperty\": 5, \"WriteOnlyProperty\": 3}")) {
                    Assert.That(JsonMapper.FromJson<ObjectWithFields>(json).ComputedProperty, Is.EqualTo(8));
                }
            });
        }
        
        [Test]
        public void JsonContainsReadOnlyWriteOnlyAndComputedProperties() {
            using var json = new StringReader("{\"ReadOnlyProperty\": 5, \"WriteOnlyProperty\": 3, \"ComputedProperty\": 8}");
            Assert.Throws<InvalidOperationException>(() => JsonMapper.FromJson<ObjectWithFields>(json));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_FromJsonOverloads {
        [Test]
        public void ReadingWithAProvidedTokenReaderIsEquivalentToTheGeneratedOne() {
            using var json1 = new StringReader("\"test\"");
            using var json2 = new StringReader("\"test\"");
            var tokenReader = new JsonTokenReader(json1);
            Assert.That(JsonMapper.FromJson<string>(tokenReader), Is.EqualTo(JsonMapper.FromJson<string>(json2)));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_ReadingInvalidJsonThrows {
        [Test]
        public void EOF() {
            using var json = new StringReader("");
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<bool>(json));
        }
        
        [Test]
        public void ArrayEnd() {
            using var json = new StringReader("]");
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<bool>(json));
        }
        
        [Test]
        public void KeyValueSeparator() {
            using var json = new StringReader(":");
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<bool>(json));
        }
        
        [Test]
        public void ObjectEnd() {
            using var json = new StringReader("}");
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<bool>(json));
        }
        
        [Test]
        public void Separator() {
            using var json = new StringReader(",");
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<bool>(json));
        }
        
        [Test]
        public void ListWithoutSeparators() {
            using var json = new StringReader("[1 2 3]");
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<List<int>>(json));
        }
        
        [Test]
        public void ListWithTrailingSeparator() {
            using var json = new StringReader("[1,2,3,]");
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<List<int>>(json));
        }

        [Test]
        public void ListWithoutClosingBracket() {
            using var json = new StringReader("[1,2,3");
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<List<int>>(json));
        }
    }
}
