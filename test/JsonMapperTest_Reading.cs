using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Voorhees.Tests {
    [TestFixture]
    public class JsonMapper_Read_InvalidJson {
        [Test]
        public void InvalidNumberJsonThrows() {
            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<int>(@"1\\\\22");
            });

            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<int>(@"1eeee2");
            });
        }

        [Test]
        public void InvalidNumberArrayJsonThrows() {
            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<int[]>(@"[1\\\\22]");
            });
        }

        [Test]
        public void InvalidNumberMultiDimensionalArrayJsonThrows() {
            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<int[,]>(@"[[1,2][3]]");
            });

            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<int[,]>(@"[[1],[2,3],]");
            });

            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<int[,]>(@"[[1],[2,");
            });
        }

        class TestObject {
            public int IntVal = default;
            public string StringVal = default;
        }

        [Test]
        public void InvalidObjectJsonThrows() {
            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<TestObject>("{\"IntVal\": 123 \"StringVal\": \"str\"}");
            });
            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<TestObject>("{\"IntVal\" 123, \"StringVal\": \"str\"}");
            });
            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<TestObject>("{\"IntVal\": 123, \"StringVal\": \"str\",}");
            });
        }

        [Test]
        public void ReadingAnInvalidJsonValueThrows() {
            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<JsonValue>(@"");
            });

            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<JsonValue>(@"{");
            });

            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<JsonValue>(@"{""test""");
            });

            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<JsonValue>(@"{""test"": 1 2");
            });

            Assert.Throws<InvalidJsonException>(() => {
                JsonMapper.FromJson<JsonValue>(@"{""test"": 1,}");
            });
        }
    }

    [TestFixture]
    public class JsonMapper_Read_Null {
        class TestReferenceType {
        }

        struct TestValueType {
        }

        [Test]
        public void ReadNull_ReferenceType() {
            Assert.That(JsonMapper.FromJson<TestReferenceType>("null"), Is.Null);
        }

        [Test]
        public void ReadNull_ValueType() {
            // Value types cannot be null.
            Assert.Throws<InvalidCastException>(() => JsonMapper.FromJson<TestValueType>("null"));
            Assert.Throws<InvalidOperationException>(() => JsonMapper.FromJson<int>("null"));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_Int {
        [Test]
        public void Basic() {
            Assert.That(JsonMapper.FromJson<int>("3"), Is.EqualTo(3));
        }

        class ClassWithImplicitConversionOperator {
            public int intVal;

            public static implicit operator ClassWithImplicitConversionOperator(int data) {
                return new ClassWithImplicitConversionOperator {intVal = data};
            }
        }

        [Test]
        public void ImplicitOperator() {
            Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>("3"), Is.Not.Null);
            Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>("3").intVal, Is.EqualTo(3));
        }

        [Test]
        public void ReadByte() {
            Assert.That(JsonMapper.FromJson<byte>("3"), Is.TypeOf<byte>());                
            Assert.That(JsonMapper.FromJson<byte>("3"), Is.EqualTo((byte)3));
        }

        [Test]
        public void ReadSByte() {
            Assert.That(JsonMapper.FromJson<sbyte>("3"), Is.TypeOf<sbyte>());
            Assert.That(JsonMapper.FromJson<sbyte>("3"), Is.EqualTo((sbyte)3));
        }

        [Test]
        public void ReadShort() {
            Assert.That(JsonMapper.FromJson<short>("3"), Is.TypeOf<short>());
            Assert.That(JsonMapper.FromJson<short>("3"), Is.EqualTo((short)3));
        }

        [Test]
        public void ReadUShort() {
            Assert.That(JsonMapper.FromJson<ushort>("3"), Is.TypeOf<ushort>());
            Assert.That(JsonMapper.FromJson<ushort>("3"), Is.EqualTo((ushort)3));
        }

        [Test]
        public void ReadUInt() {
            Assert.That(JsonMapper.FromJson<uint>("3"), Is.TypeOf<uint>());
            Assert.That(JsonMapper.FromJson<uint>("3"), Is.EqualTo((uint)3));
        }

        [Test]
        public void ReadLong() {
            Assert.That(JsonMapper.FromJson<long>("3"), Is.TypeOf<long>());
            Assert.That(JsonMapper.FromJson<long>("3"), Is.EqualTo((long)3));
        }

        [Test]
        public void ReadULong() {
            Assert.That(JsonMapper.FromJson<ulong>("3"), Is.TypeOf<ulong>());
            Assert.That(JsonMapper.FromJson<ulong>("3"), Is.EqualTo((ulong)3));
        }

        [Test]
        public void ReadFloat() {
            Assert.That(JsonMapper.FromJson<float>("3"), Is.TypeOf<float>());
            Assert.That(JsonMapper.FromJson<float>("3"), Is.EqualTo((float)3));
        }

        [Test]
        public void ReadDouble() {
            Assert.That(JsonMapper.FromJson<double>("3"), Is.TypeOf<double>());
            Assert.That(JsonMapper.FromJson<double>("3"), Is.EqualTo((double)3));
        }

        [Test]
        public void ReadDecimal() {
            Assert.That(JsonMapper.FromJson<decimal>("3"), Is.TypeOf<decimal>());
            Assert.That(JsonMapper.FromJson<decimal>("3"), Is.EqualTo((decimal)3));
        }

        enum TestEnum {
            One = 1,
            Two = 2
        }
        
        [Test]
        public void CanConvertIntToEnumValue()
        {
            Assert.Multiple(() => {
                Assert.That(JsonMapper.FromJson<TestEnum>("1"), Is.TypeOf<TestEnum>());
                Assert.That(JsonMapper.FromJson<TestEnum>("1"), Is.EqualTo(TestEnum.One));
                Assert.That(JsonMapper.FromJson<TestEnum>("2"), Is.TypeOf<TestEnum>());
                Assert.That(JsonMapper.FromJson<TestEnum>("2"), Is.EqualTo(TestEnum.Two));
            });
        }
    }

    [TestFixture]
    public class JsonMapper_Read_Bool {
        [Test]
        public void True() {
            Assert.That(JsonMapper.FromJson<bool>("true"), Is.True);
        }
        
        [Test]
        public void False() {
            Assert.That(JsonMapper.FromJson<bool>("false"), Is.False);
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
            Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>("3.5"), Is.Not.Null);
            Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>("3.5").doubleVal, Is.EqualTo(3.5));
        }
        
        [Test]
        public void ReadFloat() {
            Assert.That(JsonMapper.FromJson<float>("3.5"), Is.TypeOf<float>());
            Assert.That(JsonMapper.FromJson<float>("3.5"), Is.EqualTo(3.5f));
        }

        [Test]
        public void ReadDouble() {
            Assert.That(JsonMapper.FromJson<double>("3.5"), Is.TypeOf<double>());
            Assert.That(JsonMapper.FromJson<double>("3.5"), Is.EqualTo(3.5));
        }

        [Test]
        public void ReadDecimal() {
            Assert.That(JsonMapper.FromJson<decimal>("3.5"), Is.TypeOf<decimal>());
            Assert.That(JsonMapper.FromJson<decimal>("3.5"), Is.EqualTo(3.5m));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_String {
        [Test]
        public void ReadString() {
            Assert.That(JsonMapper.FromJson<string>("\"test\""), Is.EqualTo("test"));
        }

        class ClassWithImplicitConversionOperator {
            public string stringVal;

            public static implicit operator ClassWithImplicitConversionOperator(string data) {
                return new ClassWithImplicitConversionOperator {stringVal = data};
            }
        }

        [Test]
        public void ImplicitOperator() {
            Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>("\"test\""), Is.Not.Null);
            Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>("\"test\""), Is.TypeOf<ClassWithImplicitConversionOperator>());
            Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>("\"test\"").stringVal, Is.EqualTo("test"));
        }

        [Test]
        public void ReadChar() {
            Assert.Multiple(() => {
                Assert.That(JsonMapper.FromJson<char>("\"c\""), Is.TypeOf<char>());                
                Assert.That(JsonMapper.FromJson<char>("\"c\""), Is.EqualTo('c'));
            });
        }

        [Test]
        public void ReadCharTooLong() {
            Assert.Throws<FormatException>(() => JsonMapper.FromJson<char>("\"test\""));
        }

        [Test]
        public void ReadDateTime() {
            var dateTime = new DateTime(1970, 1, 2, 3, 4, 5, 6);
            Assert.That(JsonMapper.FromJson<DateTime>("\"1970-01-02T03:04:05.0060000\""), Is.EqualTo(dateTime));
        }

        [Test]
        public void ReadInvalidDateTime() {
            Assert.Throws<FormatException>(() => JsonMapper.FromJson<DateTime>("\"test\""));
        }

        [Test]
        public void ReadDateTimeOffset() {
            var span = new TimeSpan(0, -5, 0, 0, 0);
            var offset = new DateTimeOffset(1970, 1, 2, 3, 4, 5, 6, span);
            Assert.That(JsonMapper.FromJson<DateTimeOffset>("\"1970-01-02T03:04:05.0060000-05:00\""), Is.EqualTo(offset));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_Array {
        [Test]
        public void IntArray() {
            Assert.That(JsonMapper.FromJson<int[]>("[1, 2, 3]"), Is.EqualTo(new[] {1, 2, 3}));
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

            Assert.That(JsonMapper.FromJson<IntOrString[]>("[1, 2, \"three\"]"), Is.EqualTo(expected));
        }

        [Test]
        public void ReadArrayToList() {
            var result = JsonMapper.FromJson<List<int>>("[1, 2, 3]");
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
            Assert.Throws<InvalidCastException>(() => JsonMapper.FromJson<string>("[1,2,3]"));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_MultiDimensionalArray {
        [Test]
        public void EmptyJaggedArray() {
            Assert.That(JsonMapper.FromJson<int[][]>("[]"), Is.EqualTo(Array.Empty<int[]>()));
        }

        [Test]
        public void EmptyMultiArray() {
            Assert.That(JsonMapper.FromJson<int[,]>("[]"), Is.EqualTo(new int[,] { }));
        }

        [Test]
        public void JaggedArray() {
            Assert.That(JsonMapper.FromJson<int[][]>("[[1, 2], [3, 4]]"), Is.EqualTo(new[] {new[] {1, 2}, new[] {3, 4}}));
        }

        [Test]
        public void MultiArray() {
            Assert.That(JsonMapper.FromJson<int[,]>("[[1, 2], [3, 4]]"), Is.EqualTo(new[,] {{1, 2}, {3, 4}}));
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

            string json =
                "[[" +
                    "[[1,2],[3,4]]," +
                    "[[5,6],[7,8]]" +
                "],[" +
                    "[[9,10],[11,12]]," +
                    "[[13,14],[15,16]]" +
                "]]";
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
                Assert.That(JsonMapper.FromJson<ObjectWithFields>("{\"publicField\": 3, \"publicField2\": 5}"), Is.Not.Null);
                Assert.That(JsonMapper.FromJson<ObjectWithFields>("{\"publicField\": 3, \"publicField2\": 5}").publicField, Is.EqualTo(3));
                Assert.That(JsonMapper.FromJson<ObjectWithFields>("{\"publicField\": 3, \"publicField2\": 5}").publicField2, Is.EqualTo(5));
            });
        }

        class ObjectWithProperties {
            public int publicProperty { get; set; } = -1;
            public int publicProperty2 { get; set; } = -1;
        }

        [Test]
        public void PublicProperties() {
            Assert.Multiple(() => {
                Assert.That(JsonMapper.FromJson<ObjectWithProperties>("{\"publicProperty\": 3, \"publicProperty2\": 5}"), Is.Not.Null);
                Assert.That(JsonMapper.FromJson<ObjectWithProperties>("{\"publicProperty\": 3, \"publicProperty2\": 5}").publicProperty, Is.EqualTo(3));
                Assert.That(JsonMapper.FromJson<ObjectWithProperties>("{\"publicProperty\": 3, \"publicProperty2\": 5}").publicProperty2, Is.EqualTo(5));
            });
        }

        [Test]
        public void PublicDictionary() {
            
            Assert.Multiple(() => {
                Assert.That(JsonMapper.FromJson<Dictionary<string, int>>("{\"a\": 3, \"b\": 5}"), Is.Not.Null);
                Assert.That(JsonMapper.FromJson<Dictionary<string, int>>("{\"a\": 3, \"b\": 5}"), Has.Count.EqualTo(2));
                Assert.That(JsonMapper.FromJson<Dictionary<string, int>>("{\"a\": 3, \"b\": 5}")["a"], Is.EqualTo(3));
                Assert.That(JsonMapper.FromJson<Dictionary<string, int>>("{\"a\": 3, \"b\": 5}")["b"], Is.EqualTo(5));
            });
        }

        class ObjectWithReadOnlyProperties {
            public int publicProperty { get; } = -1;
            public int publicProperty2 { get; } = -1;
        }

        [Test]
        public void PublicReadOnlyProperties() {
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<ObjectWithReadOnlyProperties>("{\"publicProperty\": 3, \"publicProperty2\": 5}"));
        }

        [Test]
        public void ThrowsIfAssemblyQualifiedNameDoesNotExist() {
            const string BOGUS_AQN = "TopNamespace.SubNameSpace.ContainingClass+NestedClass, MyAssembly, Version=1.3.0.0, Culture=neutral, PublicKeyToken=b17a5c561934e089";
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<object>("{\"$t\":\"" + BOGUS_AQN + "\",\"foo\":\"bar\"}"));
        }

        [Test]
        public void MappingObjectWithExtraKeyThrows() {
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<ObjectWithFields>("{\"publicField\":1,\"publicField2\":2,\"publicField3\":3}"));
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
                Assert.That(JsonMapper.FromJson<DerivedClass>("{\"derivedClassValue\":false,\"baseClassValue\":2}"), Is.Not.Null);
                Assert.That(JsonMapper.FromJson<DerivedClass>("{\"derivedClassValue\":false,\"baseClassValue\":2}").derivedClassValue, Is.False);
                Assert.That(JsonMapper.FromJson<DerivedClass>("{\"derivedClassValue\":false,\"baseClassValue\":2}").baseClassValue, Is.EqualTo(2));
            });
        }

        [Test]
        public void DerivedValueInBaseReference() {
            Assert.Multiple(() => {
                string json = $"{{\"$t\":\"{typeof(DerivedClass).AssemblyQualifiedName}\",\"derivedClassValue\":false,\"baseClassValue\":2}}";
                Assert.That(JsonMapper.FromJson<BaseClass>(json), Is.Not.Null);
                
                json = $"{{\"$t\":\"{typeof(DerivedClass).AssemblyQualifiedName}\",\"derivedClassValue\":false,\"baseClassValue\":2}}";
                Assert.That(JsonMapper.FromJson<BaseClass>(json), Is.TypeOf<DerivedClass>());
                
                json = $"{{\"$t\":\"{typeof(DerivedClass).AssemblyQualifiedName}\",\"derivedClassValue\":false,\"baseClassValue\":2}}";
                Assert.That(JsonMapper.FromJson<BaseClass>(json).baseClassValue, Is.EqualTo(2));
                
                json = $"{{\"$t\":\"{typeof(DerivedClass).AssemblyQualifiedName}\",\"derivedClassValue\":false,\"baseClassValue\":2}}";
                Assert.That(((DerivedClass)JsonMapper.FromJson<BaseClass>(json)).derivedClassValue, Is.False);
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
                string json = "{\"ReadOnlyProperty\": 5, \"WriteOnlyProperty\": 3}";
                Assert.That(JsonMapper.FromJson<ObjectWithFields>(json), Is.Not.Null);

                json = "{\"ReadOnlyProperty\": 5, \"WriteOnlyProperty\": 3}";
                Assert.That(JsonMapper.FromJson<ObjectWithFields>(json).ReadOnlyProperty, Is.EqualTo(5));

                json = "{\"ReadOnlyProperty\": 5, \"WriteOnlyProperty\": 3}";
                Assert.That(JsonMapper.FromJson<ObjectWithFields>(json).GetWriteOnlyValue(), Is.EqualTo(3));

                json = "{\"ReadOnlyProperty\": 5, \"WriteOnlyProperty\": 3}";
                Assert.That(JsonMapper.FromJson<ObjectWithFields>(json).ComputedProperty, Is.EqualTo(8));
            });
        }
        
        [Test]
        public void JsonContainsReadOnlyWriteOnlyAndComputedProperties() {
            string json = "{\"ReadOnlyProperty\": 5, \"WriteOnlyProperty\": 3, \"ComputedProperty\": 8}";
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<ObjectWithFields>(json));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_ReadingInvalidJsonThrows {
        [Test]
        public void EOF() {
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<bool>(""));
        }
        
        [Test]
        public void ArrayEnd() {
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<bool>("]"));
        }
        
        [Test]
        public void KeyValueSeparator() {
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<bool>(":"));
        }
        
        [Test]
        public void ObjectEnd() {
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<bool>("}"));
        }
        
        [Test]
        public void Separator() {
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<bool>(","));
        }
        
        [Test]
        public void ListWithoutSeparators() {
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<List<int>>("[1 2 3]"));
        }
        
        [Test]
        public void ListWithTrailingSeparator() {
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<List<int>>("[1,2,3,]"));
        }

        [Test]
        public void ListWithoutClosingBracket() {
            Assert.Throws<InvalidJsonException>(() => JsonMapper.FromJson<List<int>>("[1,2,3"));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_StaticReadFunctions {
        [Test]
        public static void StreamReadIsIdenticalToStringRead() {
            const string JSON = "42";
            using var reader = new StringReader(JSON);
            Assert.That(JsonMapper.FromJson<int>(JSON), Is.EqualTo(JsonMapper.FromJson<int>(reader)));
        }

        [Test]
        public static void StreamWriteIsIdenticalToStringWrite() {
            const int DATA = 42;
            var stringBuilder = new StringBuilder();
            using (var writer = new StringWriter(stringBuilder)) {
                JsonMapper.ToJson(DATA, writer);
            }
            Assert.That(stringBuilder.ToString(), Is.EqualTo(JsonMapper.ToJson(DATA)));
        }
    }
}
