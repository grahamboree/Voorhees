using System;
using System.Collections.Generic;
using NUnit.Framework;
using Voorhees;

[TestFixture]
public class JsonMapper_Write_Numbers {
    [Test]
    public void BasicByteMapping() {
        Assert.That(JsonMapper.ToJson((byte) 42), Is.EqualTo("42"));
    }

    [Test]
    public void BasicSByteMapping() {
        Assert.That(JsonMapper.ToJson((sbyte) 42), Is.EqualTo("42"));
    }

    [Test]
    public void BasicShortMapping() {
        Assert.That(JsonMapper.ToJson((short)42), Is.EqualTo("42"));
    }

    [Test]
    public void BasicUShortMapping() {
        Assert.That(JsonMapper.ToJson((ushort)42), Is.EqualTo("42"));
    }
    
    [Test]
    public void BasicIntMapping() {
        Assert.That(JsonMapper.ToJson(42), Is.EqualTo("42"));
    }
    
    [Test]
    public void BasicUIntMapping() {
        Assert.That(JsonMapper.ToJson(42u), Is.EqualTo("42"));
    }

    [Test]
    public void BasicLongMapping() {
        Assert.That(JsonMapper.ToJson(42L), Is.EqualTo("42"));
    }

    [Test]
    public void BasicULongMapping() {
        Assert.That(JsonMapper.ToJson(42ul), Is.EqualTo("42"));
    }

    [Test]
    public void BasicFloatMapping() {
        Assert.That(JsonMapper.ToJson(0.01f), Is.EqualTo("0.01"));
    }

    [Test]
    public void BasicDoubleMapping() {
        Assert.That(JsonMapper.ToJson(0.01d), Is.EqualTo("0.01"));
    }

    [Test]
    public void BasicDecimalMapping() {
        Assert.That(JsonMapper.ToJson(0.01m), Is.EqualTo("0.01"));
    }
}

[TestFixture]
public class JsonMapper_Write_Strings {
    [Test]
    public void BasicCharMapping() {
        Assert.That(JsonMapper.ToJson('a'), Is.EqualTo("\"a\""));
    }
    
    [Test]
    public void BasicStringMapping() {
        Assert.That(JsonMapper.ToJson("test"), Is.EqualTo("\"test\""));
    }
}

[TestFixture]
public class JsonMapper_Write_BoolNull {
    [Test]
    public void Bool() {
        Assert.That(JsonMapper.ToJson(true), Is.EqualTo("true"));
        Assert.That(JsonMapper.ToJson(false), Is.EqualTo("false"));
    }
    
    [Test]
    public void Null() {
        Assert.That(JsonMapper.ToJson(null), Is.EqualTo("null"));
    }
}

[TestFixture]
public class JsonMapper_Write_Array {
    [Test]
    public void EmptyArray() {
        Assert.That(JsonMapper.ToJson(new int[] {}), Is.EqualTo("[]"));
    }
    
    [Test]
    public void Length1Array() {
        Assert.That(JsonMapper.ToJson(new[] {1}), Is.EqualTo("[1]"));
    }
    
    [Test]
    public void IntArray() {
        Assert.That(JsonMapper.ToJson(new[] {1, 2, 3}), Is.EqualTo("[1,2,3]"));
    }

    [Test]
    public void NestedArray() {
        var arrayOfArrays = new[] {new[] {1, 2}, new[] {3, 4}, new[] {5, 6}};
        Assert.That(JsonMapper.ToJson(arrayOfArrays), Is.EqualTo("[[1,2],[3,4],[5,6]]"));
    }
    
    [Test]
    public void IntList() {
        Assert.That(JsonMapper.ToJson(new List<int> {1, 2, 3}), Is.EqualTo("[1,2,3]"));
    }
}

[TestFixture]
public class JsonMapper_Write_Dictionary {
    [Test]
    public void EmptyDictionary() {
        var dict = new Dictionary<string, int>();
        Assert.That(JsonMapper.ToJson(dict), Is.EqualTo("{}"));
    }
    
    [Test]
    public void Length1Dictionary() {
        var dict = new Dictionary<string, int>{{"one", 1}};
        Assert.That(JsonMapper.ToJson(dict), Is.EqualTo("{\"one\":1}"));
    }
    
    [Test]
    public void Length3Dictionary() {
        var dict = new Dictionary<string, int> {
            {"one", 1},
            {"two", 2},
            {"three", 3}
        };
        const string expected = "{\"one\":1,\"two\":2,\"three\":3}";
        Assert.That(JsonMapper.ToJson(dict), Is.EqualTo(expected));
    }
}

[TestFixture]
public class JsonMapper_Write_OnlySerializePublicFields {
    class TestType {
        public int PubIntVal;
#pragma warning disable 414
        int privIntVal = -1;
#pragma warning restore 414
    }
    
    [Test]
    public void OnlySerializePublicFields() {
        var instance = new TestType { PubIntVal = 42 };
        const string expected = "{\"PubIntVal\":42}";
        Assert.That(JsonMapper.ToJson(instance), Is.EqualTo(expected));
    }
}

[TestFixture]
public class JsonMapper_Write_RegisterCustomSerializer {
    class TestType {
        public int PubIntVal;
    }

    [SetUp]
    public void SetUp() {
        JsonMapper.RegisterJsonExporter<TestType>(t => t.PubIntVal.ToString());
    }

    [TearDown]
    public void TearDown() {
        JsonMapper.UnRegisterJsonExporter<TestType>();
    }

    [Test]
    public void RegisterCustomSerializer() {
        var instance = new TestType { PubIntVal = 42 };
        const string expected = "42";
        Assert.That(JsonMapper.ToJson(instance), Is.EqualTo(expected));
    }
}

[TestFixture]
public class JsonMapper_Write_UnRegisterCustomSerializer {
    class TestType {
        public int PubIntVal;
    }
    
    [TearDown]
    public void TearDown() {
        JsonMapper.UnRegisterJsonExporter<TestType>();
    }

    [Test]
    public void UnRegisterCustomSerializer() {
        var instance = new TestType { PubIntVal = 42 };
        
        JsonMapper.RegisterJsonExporter<TestType>(t => t.PubIntVal.ToString());
        Assert.That(JsonMapper.ToJson(instance), Is.EqualTo("42"));
        
        JsonMapper.UnRegisterJsonExporter<TestType>();
        Assert.That(JsonMapper.ToJson(instance), Is.EqualTo("{\"PubIntVal\":42}"));
    }
}

[TestFixture]
public class JsonMapper_Write_UnRegisterAllCustomSerializers {
    class TestType {
        public int PubIntVal;
    }

    class TestType2 {
        public string PubString;
    }
    
    [TearDown]
    public void TearDown() {
        JsonMapper.UnRegisterJsonExporter<TestType>();
    }

    [Test]
    public void UnregistersSingleCustomSerializer() {
        var instance = new TestType { PubIntVal = 42 };
        
        JsonMapper.RegisterJsonExporter<TestType>(t => t.PubIntVal.ToString());
        Assert.That(JsonMapper.ToJson(instance), Is.EqualTo("42"));
        
        JsonMapper.UnRegisterAllJsonExporters();
        Assert.That(JsonMapper.ToJson(instance), Is.EqualTo("{\"PubIntVal\":42}"));
    }
    
    [Test]
    public void UnregistersMultipleCustomSerializers() {
        var instance1 = new TestType { PubIntVal = 42 };
        var instance2 = new TestType2 { PubString = "hello" };
        
        JsonMapper.RegisterJsonExporter<TestType>(t => t.PubIntVal.ToString());
        Assert.That(JsonMapper.ToJson(instance1), Is.EqualTo("42"));
        Assert.That(JsonMapper.ToJson(instance2), Is.EqualTo("{\"PubString\":\"hello\"}"));
        
        JsonMapper.RegisterJsonExporter<TestType2>(t => t.PubString.ToUpper());
        Assert.That(JsonMapper.ToJson(instance1), Is.EqualTo("42"));
        Assert.That(JsonMapper.ToJson(instance2), Is.EqualTo("HELLO"));
        
        JsonMapper.UnRegisterAllJsonExporters();
        Assert.That(JsonMapper.ToJson(instance1), Is.EqualTo("{\"PubIntVal\":42}"));
        Assert.That(JsonMapper.ToJson(instance2), Is.EqualTo("{\"PubString\":\"hello\"}"));
    }
}

[TestFixture]
public class JsonMapper_Write_DateTime {
    [Test]
    public void BasicDateTime() {
        var dateTime = new DateTime(1970, 1, 2, 3, 4, 5, 6);
        Assert.That(JsonMapper.ToJson(dateTime), Is.EqualTo("\"1970-01-02T03:04:05.0060000\""));
    }
}

[TestFixture]
public class JsonMapper_Write_DateTimeOffset {
    [Test]
    public void BasicDateTime() {
        var span = new TimeSpan(0, -5, 0, 0, 0);
        var offset = new DateTimeOffset(1970, 1, 2, 3, 4, 5, 6, span);
        Assert.That(JsonMapper.ToJson(offset), Is.EqualTo("\"1970-01-02T03:04:05.0060000-05:00\""));
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
        // TODO include line number in the exception.
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
    
    [Test]
    public void NestedIntArray() {
        Assert.That(JsonMapper.FromJson<int[][]>("[[1, 2], [3, 4]]"), Is.EqualTo(new[] {new[] {1, 2}, new[] {3, 4}}));
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
        var json = "{\"a\": 3, \"b\": 5}";
        Assert.That(JsonMapper.FromJson<Dictionary<string, int>>(json), Is.Not.Null);
        Assert.That(JsonMapper.FromJson<Dictionary<string, int>>(json).Count, Is.EqualTo(2));
        Assert.That(JsonMapper.FromJson<Dictionary<string, int>>(json)["a"], Is.EqualTo(3));
        Assert.That(JsonMapper.FromJson<Dictionary<string, int>>(json)["b"], Is.EqualTo(5));
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