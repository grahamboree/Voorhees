using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Voorhees.Tests {
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
            Assert.That(JsonMapper.ToJson((short) 42), Is.EqualTo("42"));
        }

        [Test]
        public void BasicUShortMapping() {
            Assert.That(JsonMapper.ToJson((ushort) 42), Is.EqualTo("42"));
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
    public class JsonMapper_Write_JsonValue {
        [Test]
        public void MapJsonValue() {
            var val = new JsonValue {
                {"one", 1},
                {"two", 2},
                {"three", new JsonValue {"tres", "san" }}
            };

            const string expected = "{\"one\":1,\"two\":2,\"three\":[\"tres\",\"san\"]}";
            Assert.That(JsonMapper.ToJson(val), Is.EqualTo(expected));
        }
    }
    
    [TestFixture]
    public class JsonMapper_Write_JsonValue_PrettyPrint {
        [SetUp]
        public void Setup() {
            JsonConfig.CurrentConfig.PrettyPrint = true;
        }

        [TearDown]
        public void TearDown() {
            JsonConfig.CurrentConfig.PrettyPrint = false;
        }

        [Test]
        public void MapJsonValue() {
            var val = new JsonValue {
                {"one", 1},
                {"two", 2},
                {"three", new JsonValue {"tres", "san" }}
            };

            const string expected = 
                "{\n" +
                "\t\"one\": 1,\n" +
                "\t\"two\": 2,\n" +
                "\t\"three\": [\n" +
                "\t\t\"tres\",\n" +
                "\t\t\"san\"\n" +
                "\t]\n" +
                "}";
            Assert.That(JsonMapper.ToJson(val), Is.EqualTo(expected));
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
            Assert.That(JsonMapper.ToJson(new int[] { }), Is.EqualTo("[]"));
        }

        [Test]
        public void Length1Array() {
            Assert.That(JsonMapper.ToJson(new[] {1}), Is.EqualTo("[1]"));
        }

        [Test]
        public void IntArray() {
            Assert.That(JsonMapper.ToJson(new[] {1, 2, 3}), Is.EqualTo("[1,2,3]"));
        }
    }
    
    [TestFixture]
    public class JsonMapper_Write_List {
        [Test]
        public void EmptyList() {
            Assert.That(JsonMapper.ToJson(new List<int>()), Is.EqualTo("[]"));
        }

        [Test]
        public void Length1List() {
            Assert.That(JsonMapper.ToJson(new List<int> {1}), Is.EqualTo("[1]"));
        }

        [Test]
        public void IntList() {
            Assert.That(JsonMapper.ToJson(new List<int> {1, 2, 3}), Is.EqualTo("[1,2,3]"));
        }
    }

    [TestFixture]
    public class JsonMapper_Write_Array_PrettyPrint {
        [SetUp]
        public void Setup() {
            JsonConfig.CurrentConfig.PrettyPrint = true;
        }

        [TearDown]
        public void TearDown() {
            JsonConfig.CurrentConfig.PrettyPrint = false;
        }

        [Test]
        public void EmptyArray() {
            Assert.That(JsonMapper.ToJson(new int[] { }), Is.EqualTo("[\n]"));
        }

        [Test]
        public void Length1Array() {
            Assert.That(JsonMapper.ToJson(new[] {1}), Is.EqualTo("[\n\t1\n]"));
        }

        [Test]
        public void IntArray() {
            Assert.That(JsonMapper.ToJson(new[] {1, 2, 3}), Is.EqualTo("[\n\t1,\n\t2,\n\t3\n]"));
        }

        [Test]
        public void IntList() {
            Assert.That(JsonMapper.ToJson(new List<int> {1, 2, 3}), Is.EqualTo("[\n\t1,\n\t2,\n\t3\n]"));
        }
    }

    [TestFixture]
    public class JsonMapper_Write_List_PrettyPrint {
        [SetUp]
        public void Setup() {
            JsonConfig.CurrentConfig.PrettyPrint = true;
        }

        [TearDown]
        public void TearDown() {
            JsonConfig.CurrentConfig.PrettyPrint = false;
        }

        [Test]
        public void EmptyArray() {
            Assert.That(JsonMapper.ToJson(new List<int>()), Is.EqualTo("[\n]"));
        }

        [Test]
        public void Length1Array() {
            Assert.That(JsonMapper.ToJson(new List<int> {1}), Is.EqualTo("[\n\t1\n]"));
        }

        [Test]
        public void IntArray() {
            Assert.That(JsonMapper.ToJson(new List<int> {1, 2, 3}), Is.EqualTo("[\n\t1,\n\t2,\n\t3\n]"));
        }
    }

    [TestFixture]
    public class JsonMapper_Write_MultiDimensionalArray {
        [Test]
        public void EmptyJaggedArray() {
            Assert.That(JsonMapper.ToJson(new int[][] { }), Is.EqualTo("[]"));
        }

        [Test]
        public void EmptyMultiArray() {
            Assert.That(JsonMapper.ToJson(new int[,] { }), Is.EqualTo("[]"));
        }

        [Test]
        public void JaggedArray() {
            var arrayOfArrays = new[] {new[] {1, 2}, new[] {3, 4}, new[] {5, 6}};
            Assert.That(JsonMapper.ToJson(arrayOfArrays), Is.EqualTo("[[1,2],[3,4],[5,6]]"));
        }

        [Test]
        public void MultiArray() {
            Assert.That(JsonMapper.ToJson(new[,] {{1}, {2}}), Is.EqualTo("[[1],[2]]"));
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
            Assert.That(JsonMapper.ToJson(multi), Is.EqualTo(json));
        }
    }
    
    [TestFixture]
    public class JsonMapper_Write_MultiDimensionalList {
        [Test]
        public void EmptyJaggedList() {
            Assert.That(JsonMapper.ToJson(new List<List<int>>()), Is.EqualTo("[]"));
        }

        [Test]
        public void JaggedList() {
            var arrayOfArrays = new List<List<int>> {
                new List<int> {1, 2}, 
                new List<int> {3, 4}, 
                new List<int> {5, 6}
            };
            Assert.That(JsonMapper.ToJson(arrayOfArrays), Is.EqualTo("[[1,2],[3,4],[5,6]]"));
        }
    }
    
    [TestFixture]
    public class JsonMapper_Write_MultiDimensionalArray_PrettyPrint {
        [SetUp]
        public void Setup() {
            JsonConfig.CurrentConfig.PrettyPrint = true;
        }

        [TearDown]
        public void TearDown() {
            JsonConfig.CurrentConfig.PrettyPrint = false;
        }
        
        [Test]
        public void EmptyJaggedArray() {
            Assert.That(JsonMapper.ToJson(new int[][] { }), Is.EqualTo("[\n]"));
        }

        [Test]
        public void EmptyMultiArray() {
            Assert.That(JsonMapper.ToJson(new int[,] { }), Is.EqualTo("[\n]"));
        }

        [Test]
        public void JaggedArray() {
            var arrayOfArrays = new[] {
                new[] {1, 2}, 
                new[] {3, 4}, 
                new[] {5, 6}
            };
            
            string json = 
                "[\n" +
                "\t[\n" +
                "\t\t1,\n" +
                "\t\t2\n" +
                "\t],\n" +
                "\t[\n" +
                "\t\t3,\n" +
                "\t\t4\n" +
                "\t],\n" +
                "\t[\n" +
                "\t\t5,\n" +
                "\t\t6\n" +
                "\t]\n" +
                "]";
            
            Assert.That(JsonMapper.ToJson(arrayOfArrays), Is.EqualTo(json));
        }

        [Test]
        public void MultiArray() {
            string json = 
                "[\n" +
                "\t[\n" +
                "\t\t1\n" +
                "\t],\n" +
                "\t[\n" +
                "\t\t2\n" +
                "\t]\n" +
                "]";
            
            Assert.That(JsonMapper.ToJson(new[,] {{1}, {2}}), Is.EqualTo(json));
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
                "[\n"+
                "\t[\n" +
                "\t\t[\n" +
                "\t\t\t[\n" +
                "\t\t\t\t1,\n" +
                "\t\t\t\t2\n" +
                "\t\t\t],\n" +
                "\t\t\t[\n" +
                "\t\t\t\t3,\n" +
                "\t\t\t\t4\n" +
                "\t\t\t]\n" +
                "\t\t],\n" +
                "\t\t[\n" +
                "\t\t\t[\n" +
                "\t\t\t\t5,\n" +
                "\t\t\t\t6\n" +
                "\t\t\t],\n" +
                "\t\t\t[\n" +
                "\t\t\t\t7,\n" +
                "\t\t\t\t8\n" +
                "\t\t\t]\n" +
                "\t\t]\n" +
                "\t],\n" +
                "\t[\n" +
                "\t\t[\n" +
                "\t\t\t[\n" +
                "\t\t\t\t9,\n" +
                "\t\t\t\t10\n" +
                "\t\t\t],\n" +
                "\t\t\t[\n" +
                "\t\t\t\t11,\n" +
                "\t\t\t\t12\n" +
                "\t\t\t]\n" +
                "\t\t],\n" +
                "\t\t[\n" +
                "\t\t\t[\n" +
                "\t\t\t\t13,\n" +
                "\t\t\t\t14\n" +
                "\t\t\t],\n" +
                "\t\t\t[\n" +
                "\t\t\t\t15,\n" +
                "\t\t\t\t16\n" +
                "\t\t\t]\n" +
                "\t\t]\n" +
                "\t]\n" +
                "]";
            Assert.That(JsonMapper.ToJson(multi), Is.EqualTo(json));
        }
    }
    
    [TestFixture]
    public class JsonMapper_Write_MultiDimensionalList_PrettyPrint {
        [SetUp]
        public void Setup() {
            JsonConfig.CurrentConfig.PrettyPrint = true;
        }

        [TearDown]
        public void TearDown() {
            JsonConfig.CurrentConfig.PrettyPrint = false;
        }
        
        [Test]
        public void EmptyJaggedList() {
            Assert.That(JsonMapper.ToJson(new List<List<int>>()), Is.EqualTo("[\n]"));
        }

        [Test]
        public void JaggedList() {
            var listOfLists = new List<List<int>> {
                new List<int> {1, 2}, 
                new List<int> {3, 4}, 
                new List<int> {5, 6}
            };
            
            string json = 
                "[\n" +
                "\t[\n" +
                "\t\t1,\n" +
                "\t\t2\n" +
                "\t],\n" +
                "\t[\n" +
                "\t\t3,\n" +
                "\t\t4\n" +
                "\t],\n" +
                "\t[\n" +
                "\t\t5,\n" +
                "\t\t6\n" +
                "\t]\n" +
                "]";
            
            Assert.That(JsonMapper.ToJson(listOfLists), Is.EqualTo(json));
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
            var dict = new Dictionary<string, int> {{"one", 1}};
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
    public class JsonMapper_Write_Dictionary_PrettyPrint {
        [SetUp]
        public void Setup() {
            JsonConfig.CurrentConfig.PrettyPrint = true;
        }

        [TearDown]
        public void TearDown() {
            JsonConfig.CurrentConfig.PrettyPrint = false;
        }
        
        [Test]
        public void EmptyDictionary() {
            var dict = new Dictionary<string, int>();
            Assert.That(JsonMapper.ToJson(dict), Is.EqualTo("{\n}"));
        }

        [Test]
        public void Length1Dictionary() {
            var dict = new Dictionary<string, int> {{"one", 1}};
            Assert.That(JsonMapper.ToJson(dict), Is.EqualTo("{\n\t\"one\": 1\n}"));
        }

        [Test]
        public void Length3Dictionary() {
            var dict = new Dictionary<string, int> {
                {"one", 1},
                {"two", 2},
                {"three", 3}
            };
            const string expected = "{\n\t\"one\": 1,\n\t\"two\": 2,\n\t\"three\": 3\n}";
            Assert.That(JsonMapper.ToJson(dict), Is.EqualTo(expected));
        }
    }
    
    [TestFixture]
    public class JsonMapper_Write_Object {
        class TestType {
            public int PubIntVal;
#pragma warning disable 414
            int privIntVal = -1;
#pragma warning restore 414
        }

        [Test]
        public void OnlySerializePublicFields() {
            var instance = new TestType {PubIntVal = 42};
            const string expected = "{\"PubIntVal\":42}";
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo(expected));
        }
        
        class MultiFieldType {
            public int PubIntVal;
            public string PubStringVal;
        }

        [Test]
        public void SerializeMultipleFields() {
            var instance = new MultiFieldType {
                PubIntVal = 42,
                PubStringVal = "test"
            };
            const string expected = "{\"PubIntVal\":42,\"PubStringVal\":\"test\"}";
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo(expected));
        }

        class NestedClass {
            public int PubIntegerVal;
            public TestType TestTypeVal;
        }
        
        [Test]
        public void NestedObjectReferences() {
            var instance = new NestedClass {
                PubIntegerVal = 42,
                TestTypeVal = new TestType {
                    PubIntVal = 99
                }
            };
            const string expected = "{\"PubIntegerVal\":42,\"TestTypeVal\":{\"PubIntVal\":99}}";
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo(expected));
        }

        [Test]
        public void AnonymousObject() {
            var instance = new {stringVal = "someString", intVal = 3};
            string expected = "{\"stringVal\":\"someString\",\"intVal\":3}";
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo(expected));
        }
    }

    [TestFixture]
    public class JsonMapper_Write_Object_PrettyPrint {
        [SetUp]
        public void Setup() {
            JsonConfig.CurrentConfig.PrettyPrint = true;
        }

        [TearDown]
        public void TearDown() {
            JsonConfig.CurrentConfig.PrettyPrint = false;
        }

        class TestType {
            public int PubIntVal;
#pragma warning disable 414
            int privIntVal = -1;
#pragma warning restore 414
        }

        [Test]
        public void OnlySerializePublicFields() {
            var instance = new TestType {PubIntVal = 42};
            const string expected = "{\n\t\"PubIntVal\": 42\n}";
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo(expected));
        }
        
        class MultiFieldType {
            public int PubIntVal;
            public string PubStringVal;
        }

        [Test]
        public void SerializeMultipleFields() {
            var instance = new MultiFieldType {
                PubIntVal = 42,
                PubStringVal = "test"
            };
            const string expected = "{\n\t\"PubIntVal\": 42,\n\t\"PubStringVal\": \"test\"\n}";
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo(expected));
        }

        class NestedClass {
            public int PubIntegerVal;
            public TestType TestTypeVal;
        }
        
        [Test]
        public void NestedObjectReferences() {
            var instance = new NestedClass {
                PubIntegerVal = 42,
                TestTypeVal = new TestType {
                    PubIntVal = 99
                }
            };
            const string expected =
                "{\n" + 
                "\t\"PubIntegerVal\": 42,\n" +
                "\t\"TestTypeVal\": {\n" +
                "\t\t\"PubIntVal\": 99\n" +
                "\t}\n" +
                "}";
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo(expected));
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
    public class JsonMapper_Write_PolymorphicObjectReference {
        class BaseClass {
            public int baseClassValue;
        }

        class DerivedClass : BaseClass {
            public bool derivedClassValue;
        }

        [Test]
        public void PolymorphicTypeReference() {
            var value = new DerivedClass {baseClassValue = 2, derivedClassValue = false};
            string json = "{\"derivedClassValue\":false,\"baseClassValue\":2}";
            Assert.That(JsonMapper.ToJson(value), Is.EqualTo(json));
        }
    }
}
