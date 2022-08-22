using System;
using System.Collections.Generic;
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
            Assert.That(JsonMapper.FromJson<TestReferenceType>("null"), Is.Null);
        }

        [Test]
        public void ReadNull_ValueType() {
            // Value types cannot be null.
            Assert.Throws<Exception>(() => JsonMapper.FromJson<TestValueType>("null"));
            Assert.Throws<Exception>(() => JsonMapper.FromJson<int>("null"));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_Int {
        [Test]
        public void Basic() {
            Assert.That(JsonMapper.FromJson<int>("3"), Is.EqualTo(3));
        }

        enum SampleEnum {
            Three = 3
        }

        [Test]
        public void EnumBackedByInt() {
            Assert.That(JsonMapper.FromJson<SampleEnum>("3"), Is.EqualTo(SampleEnum.Three));
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
            Assert.That(JsonMapper.FromJson<byte>("3"), Is.EqualTo((byte) 3));
        }

        [Test]
        public void ReadSByte() {
            Assert.That(JsonMapper.FromJson<sbyte>("3"), Is.TypeOf<sbyte>());
            Assert.That(JsonMapper.FromJson<sbyte>("3"), Is.EqualTo((sbyte) 3));
        }

        [Test]
        public void ReadShort() {
            Assert.That(JsonMapper.FromJson<short>("3"), Is.TypeOf<short>());
            Assert.That(JsonMapper.FromJson<short>("3"), Is.EqualTo((short) 3));
        }

        [Test]
        public void ReadUShort() {
            Assert.That(JsonMapper.FromJson<ushort>("3"), Is.TypeOf<ushort>());
            Assert.That(JsonMapper.FromJson<ushort>("3"), Is.EqualTo((ushort) 3));
        }

        [Test]
        public void ReadUInt() {
            Assert.That(JsonMapper.FromJson<uint>("3"), Is.TypeOf<uint>());
            Assert.That(JsonMapper.FromJson<uint>("3"), Is.EqualTo((uint) 3));
        }

        [Test]
        public void ReadLong() {
            Assert.That(JsonMapper.FromJson<long>("3"), Is.TypeOf<long>());
            Assert.That(JsonMapper.FromJson<long>("3"), Is.EqualTo((long) 3));
        }

        [Test]
        public void ReadULong() {
            Assert.That(JsonMapper.FromJson<ulong>("3"), Is.TypeOf<ulong>());
            Assert.That(JsonMapper.FromJson<ulong>("3"), Is.EqualTo((ulong) 3));
        }

        [Test]
        public void ReadFloat() {
            Assert.That(JsonMapper.FromJson<float>("3"), Is.TypeOf<float>());
            Assert.That(JsonMapper.FromJson<float>("3"), Is.EqualTo((float) 3));
        }

        [Test]
        public void ReadDouble() {
            Assert.That(JsonMapper.FromJson<double>("3"), Is.TypeOf<double>());
            Assert.That(JsonMapper.FromJson<double>("3"), Is.EqualTo((double) 3));
        }

        [Test]
        public void ReadDecimal() {
            Assert.That(JsonMapper.FromJson<decimal>("3"), Is.TypeOf<decimal>());
            Assert.That(JsonMapper.FromJson<decimal>("3"), Is.EqualTo((decimal) 3));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_Float {
        [Test]
        public void ReadFloat() {
            Assert.That(JsonMapper.FromJson<float>("3.5"), Is.EqualTo(3.5f));
        }

        class ClassWithImplicitConversionOperator {
            public float floatVal;

            public static implicit operator ClassWithImplicitConversionOperator(float data) {
                return new ClassWithImplicitConversionOperator {floatVal = data};
            }
        }

        [Test]
        public void ImplicitOperator() {
            Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>("3.5"), Is.Not.Null);
            Assert.That(JsonMapper.FromJson<ClassWithImplicitConversionOperator>("3.5").floatVal, Is.EqualTo(3.5f));
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
        public void ReadFloat() {
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
            Assert.That(JsonMapper.FromJson<char>("\"c\""), Is.TypeOf<char>());
            Assert.That(JsonMapper.FromJson<char>("\"c\""), Is.EqualTo('c'));
        }

        [Test]
        public void ReadCharTooLong() {
            Assert.Throws<FormatException>(() => JsonMapper.FromJson<char>("\"test\""));
        }

        [Test]
        public void ReadDateTime() {
            var json = "\"1970-01-02T03:04:05.0060000\"";
            var dateTime = new DateTime(1970, 1, 2, 3, 4, 5, 6);
            Assert.That(JsonMapper.FromJson<DateTime>(json), Is.EqualTo(dateTime));
        }

        [Test]
        public void ReadInvalidDateTime() {
            Assert.Throws<FormatException>(() => JsonMapper.FromJson<DateTime>("\"test\""));
        }

        [Test]
        public void ReadDateTimeOffset() {
            var span = new TimeSpan(0, -5, 0, 0, 0);
            var offset = new DateTimeOffset(1970, 1, 2, 3, 4, 5, 6, span);
            var json = "\"1970-01-02T03:04:05.0060000-05:00\"";
            Assert.That(JsonMapper.FromJson<DateTimeOffset>(json), Is.EqualTo(offset));
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
    }

    [TestFixture]
    public class JsonMapper_Read_MultiDimensionalArray {
        [Test]
        public void EmptyJaggedArray() {
            Assert.That(JsonMapper.FromJson<int[][]>("[]"), Is.EqualTo(new int[][] { }));
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
            var multi = new[,,,] {
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
            var json = "{\"publicField\": 3, \"publicField2\": 5}";
            Assert.That(JsonMapper.FromJson<ObjectWithFields>(json), Is.Not.Null);
            Assert.That(JsonMapper.FromJson<ObjectWithFields>(json).publicField, Is.EqualTo(3));
            Assert.That(JsonMapper.FromJson<ObjectWithFields>(json).publicField2, Is.EqualTo(5));
        }

        class ObjectWithProperties {
            public int publicProperty { get; set; } = -1;
            public int publicProperty2 { get; set; } = -1;
        }

        [Test]
        public void PublicProperties() {
            var json = "{\"publicProperty\": 3, \"publicProperty2\": 5}";
            Assert.That(JsonMapper.FromJson<ObjectWithProperties>(json), Is.Not.Null);
            Assert.That(JsonMapper.FromJson<ObjectWithProperties>(json).publicProperty, Is.EqualTo(3));
            Assert.That(JsonMapper.FromJson<ObjectWithProperties>(json).publicProperty2, Is.EqualTo(5));
        }

        [Test]
        public void PublicDictionary() {
            const string json = "{\"a\": 3, \"b\": 5}";
            Assert.Multiple(() =>
            {
                Assert.That(JsonMapper.FromJson<Dictionary<string, int>>(json), Is.Not.Null);
                Assert.That(JsonMapper.FromJson<Dictionary<string, int>>(json), Has.Count.EqualTo(2));
                Assert.That(JsonMapper.FromJson<Dictionary<string, int>>(json)["a"], Is.EqualTo(3));
                Assert.That(JsonMapper.FromJson<Dictionary<string, int>>(json)["b"], Is.EqualTo(5));
            });
        }

        class ObjectWithReadOnlyProperties {
            public int publicProperty { get; } = -1;
            public int publicProperty2 { get; } = -1;
        }

        [Test]
        public void PublicReadOnlyProperties() {
            var json = "{\"publicProperty\": 3, \"publicProperty2\": 5}";
            Assert.Throws<Exception>(() => JsonMapper.FromJson<ObjectWithReadOnlyProperties>(json));
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
            const string JSON = "{\"derivedClassValue\":false,\"baseClassValue\":2}";
            Assert.That(JsonMapper.FromJson<DerivedClass>(JSON), Is.Not.Null);
            Assert.That(JsonMapper.FromJson<DerivedClass>(JSON).derivedClassValue, Is.False);
            Assert.That(JsonMapper.FromJson<DerivedClass>(JSON).baseClassValue, Is.EqualTo(2));
        }
        
        [Test]
        public void DerivedValueInBaseReference() {
            string json = $"{{\"$t\":\"{typeof(DerivedClass).AssemblyQualifiedName}\",\"derivedClassValue\":false,\"baseClassValue\":2}}";
            Assert.Multiple(() => {
                Assert.That(JsonMapper.FromJson<BaseClass>(json), Is.Not.Null);
                Assert.That(JsonMapper.FromJson<BaseClass>(json), Is.TypeOf<DerivedClass>());
                Assert.That(JsonMapper.FromJson<BaseClass>(json).baseClassValue, Is.EqualTo(2));
                Assert.That(((DerivedClass)JsonMapper.FromJson<BaseClass>(json)).derivedClassValue, Is.False);    
            });
        }
    }
}
