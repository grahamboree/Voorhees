using System;
using System.Collections.Generic;
using NUnit.Framework;
using Voorhees;

[TestFixture]
public class JsonMapperTest_Numbers {
    [Test]
    public void BasicByteMapping() {
        Assert.That(JsonMapper.Serialize((byte) 42), Is.EqualTo("42"));
    }

    [Test]
    public void BasicSByteMapping() {
        Assert.That(JsonMapper.Serialize((sbyte) 42), Is.EqualTo("42"));
    }

    [Test]
    public void BasicShortMapping() {
        Assert.That(JsonMapper.Serialize((short)42), Is.EqualTo("42"));
    }

    [Test]
    public void BasicUShortMapping() {
        Assert.That(JsonMapper.Serialize((ushort)42), Is.EqualTo("42"));
    }
    
    [Test]
    public void BasicIntMapping() {
        Assert.That(JsonMapper.Serialize(42), Is.EqualTo("42"));
    }
    
    [Test]
    public void BasicUIntMapping() {
        Assert.That(JsonMapper.Serialize(42u), Is.EqualTo("42"));
    }

    [Test]
    public void BasicLongMapping() {
        Assert.That(JsonMapper.Serialize(42L), Is.EqualTo("42"));
    }

    [Test]
    public void BasicULongMapping() {
        Assert.That(JsonMapper.Serialize(42ul), Is.EqualTo("42"));
    }

    [Test]
    public void BasicFloatMapping() {
        Assert.That(JsonMapper.Serialize(0.01f), Is.EqualTo("0.01"));
    }

    [Test]
    public void BasicDoubleMapping() {
        Assert.That(JsonMapper.Serialize(0.01d), Is.EqualTo("0.01"));
    }

    [Test]
    public void BasicDecimalMapping() {
        Assert.That(JsonMapper.Serialize(0.01m), Is.EqualTo("0.01"));
    }
}

[TestFixture]
public class JsonMapperTest_Strings {
    [Test]
    public void BasicCharMapping() {
        Assert.That(JsonMapper.Serialize('a'), Is.EqualTo("\"a\""));
    }
    
    [Test]
    public void BasicStringMapping() {
        Assert.That(JsonMapper.Serialize("test"), Is.EqualTo("\"test\""));
    }
}

[TestFixture]
public class JsonMapperTest_BoolNull {
    [Test]
    public void Bool() {
        Assert.That(JsonMapper.Serialize(true), Is.EqualTo("true"));
        Assert.That(JsonMapper.Serialize(false), Is.EqualTo("false"));
    }
    
    [Test]
    public void Null() {
        Assert.That(JsonMapper.Serialize(null), Is.EqualTo("null"));
    }
}

[TestFixture]
public class JsonMapperTest_Array {
    [Test]
    public void EmptyArray() {
        Assert.That(JsonMapper.Serialize(new int[] {}), Is.EqualTo("[]"));
    }
    
    [Test]
    public void Length1Array() {
        Assert.That(JsonMapper.Serialize(new[] {1}), Is.EqualTo("[1]"));
    }
    
    [Test]
    public void IntArray() {
        Assert.That(JsonMapper.Serialize(new[] {1, 2, 3}), Is.EqualTo("[1,2,3]"));
    }

    [Test]
    public void NestedArray() {
        var arrayOfArrays = new[] {new[] {1, 2}, new[] {3, 4}, new[] {5, 6}};
        Assert.That(JsonMapper.Serialize(arrayOfArrays), Is.EqualTo("[[1,2],[3,4],[5,6]]"));
    }
    
    [Test]
    public void IntList() {
        Assert.That(JsonMapper.Serialize(new List<int> {1, 2, 3}), Is.EqualTo("[1,2,3]"));
    }
}

[TestFixture]
public class JsonMapperTest_Dictionary {
    [Test]
    public void EmptyDictionary() {
        var dict = new Dictionary<string, int>();
        Assert.That(JsonMapper.Serialize(dict), Is.EqualTo("{}"));
    }
    
    [Test]
    public void Length1Dictionary() {
        var dict = new Dictionary<string, int>{{"one", 1}};
        Assert.That(JsonMapper.Serialize(dict), Is.EqualTo("{\"one\":1}"));
    }
    
    [Test]
    public void Length3Dictionary() {
        var dict = new Dictionary<string, int> {
            {"one", 1},
            {"two", 2},
            {"three", 3}
        };
        const string expected = "{\"one\":1,\"two\":2,\"three\":3}";
        Assert.That(JsonMapper.Serialize(dict), Is.EqualTo(expected));
    }
}

[TestFixture]
public class JsonMapperTest_OnlySerializePublicFields {
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
        Assert.That(JsonMapper.Serialize(instance), Is.EqualTo(expected));
    }
}

[TestFixture]
public class JsonMapperTest_RegisterCustomSerializer {
    class TestType {
        public int PubIntVal;
    }

    [SetUp]
    public void SetUp() {
        JsonMapper.RegisterSerializer<TestType>(t => t.PubIntVal.ToString());
    }

    [TearDown]
    public void TearDown() {
        JsonMapper.UnRegisterSerializer<TestType>();
    }

    [Test]
    public void RegisterCustomSerializer() {
        var instance = new TestType { PubIntVal = 42 };
        const string expected = "42";
        Assert.That(JsonMapper.Serialize(instance), Is.EqualTo(expected));
    }
}

[TestFixture]
public class JsonMapperTest_UnRegisterCustomSerializer {
    class TestType {
        public int PubIntVal;
    }
    
    [TearDown]
    public void TearDown() {
        JsonMapper.UnRegisterSerializer<TestType>();
    }

    [Test]
    public void UnRegisterCustomSerializer() {
        var instance = new TestType { PubIntVal = 42 };
        
        JsonMapper.RegisterSerializer<TestType>(t => t.PubIntVal.ToString());
        Assert.That(JsonMapper.Serialize(instance), Is.EqualTo("42"));
        
        JsonMapper.UnRegisterSerializer<TestType>();
        Assert.That(JsonMapper.Serialize(instance), Is.EqualTo("{\"PubIntVal\":42}"));
    }
}

[TestFixture]
public class JsonMapperTest_UnRegisterAllCustomSerializers {
    class TestType {
        public int PubIntVal;
    }

    class TestType2 {
        public string PubString;
    }
    
    [TearDown]
    public void TearDown() {
        JsonMapper.UnRegisterSerializer<TestType>();
    }

    [Test]
    public void UnregistersSingleCustomSerializer() {
        var instance = new TestType { PubIntVal = 42 };
        
        JsonMapper.RegisterSerializer<TestType>(t => t.PubIntVal.ToString());
        Assert.That(JsonMapper.Serialize(instance), Is.EqualTo("42"));
        
        JsonMapper.UnRegisterAllSerializers();
        Assert.That(JsonMapper.Serialize(instance), Is.EqualTo("{\"PubIntVal\":42}"));
    }
    
    [Test]
    public void UnregistersMultipleCustomSerializers() {
        var instance1 = new TestType { PubIntVal = 42 };
        var instance2 = new TestType2 { PubString = "hello" };
        
        JsonMapper.RegisterSerializer<TestType>(t => t.PubIntVal.ToString());
        Assert.That(JsonMapper.Serialize(instance1), Is.EqualTo("42"));
        Assert.That(JsonMapper.Serialize(instance2), Is.EqualTo("{\"PubString\":\"hello\"}"));
        
        JsonMapper.RegisterSerializer<TestType2>(t => t.PubString.ToUpper());
        Assert.That(JsonMapper.Serialize(instance1), Is.EqualTo("42"));
        Assert.That(JsonMapper.Serialize(instance2), Is.EqualTo("HELLO"));
        
        JsonMapper.UnRegisterAllSerializers();
        Assert.That(JsonMapper.Serialize(instance1), Is.EqualTo("{\"PubIntVal\":42}"));
        Assert.That(JsonMapper.Serialize(instance2), Is.EqualTo("{\"PubString\":\"hello\"}"));
    }
}

[TestFixture]
public class JsonMapperTest_DateTime {
    [Test]
    public void BasicDateTime() {
        var dateTime = new DateTime(1970, 1, 2, 3, 4, 5, 6);
        Assert.That(JsonMapper.Serialize(dateTime), Is.EqualTo("\"1970-01-02T03:04:05\""));
    }
}

[TestFixture]
public class JsonMapperTest_DateTimeOffset {
    [Test]
    public void BasicDateTime() {
        var span = new TimeSpan(0, -5, 0, 0, 0);
        var offset = new DateTimeOffset(1970, 1, 2, 3, 4, 5, 6, span);
        Assert.That(JsonMapper.Serialize(offset), Is.EqualTo("\"1970-01-02T03:04:05.0060000-05:00\""));
    }
}
