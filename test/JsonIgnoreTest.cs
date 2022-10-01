using System.IO;
using NUnit.Framework;

namespace Voorhees.Tests {
    [TestFixture]
    public class JsonMapper_Write_IgnoreAttribute {
        class FieldsTest {
            public int intValue = 3;
            [JsonIgnore] public float floatVal = 3.5f;
        }

        [Test]
        public void IgnoreField() {
            var instance = new FieldsTest();
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo("{\"intValue\":3}"));
        }

        class PropertiesTest {
            public int intValue { get; set; } = 3;
            [JsonIgnore] public float floatVal { get; set; } = 3.5f;
        }

        [Test]
        public void IgnoreProperty() {
            var instance = new PropertiesTest();
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo("{\"intValue\":3}"));
        }
    }

    [TestFixture]
    public class JsonMapper_Read_IgnoreAttribute {
        class FieldsTest {
            public int intValue = 3;
            [JsonIgnore] public float floatVal = 3.5f;
        }

        [Test]
        public void IgnoreField() {
            Assert.Multiple(() => {
                using (var json = new StringReader("{\"intValue\":5}")) {
                    Assert.That(JsonMapper.FromJson<FieldsTest>(json), Is.Not.Null);
                }
                using (var json = new StringReader("{\"intValue\":5}")) {
                    Assert.That(JsonMapper.FromJson<FieldsTest>(json).intValue, Is.EqualTo(5));
                }
                using (var json = new StringReader("{\"intValue\":5}")) {
                    Assert.That(JsonMapper.FromJson<FieldsTest>(json).floatVal, Is.EqualTo(3.5f));
                }
            });
        }

        [Test]
        public void IgnoreFieldEvenIfSpecified() {
            Assert.Multiple(() => {
                using (var json = new StringReader("{\"intValue\":5,\"floatVal\":7.9}")) {
                    Assert.That(JsonMapper.FromJson<FieldsTest>(json), Is.Not.Null);                
                }
                using (var json = new StringReader("{\"intValue\":5,\"floatVal\":7.9}")) {
                    Assert.That(JsonMapper.FromJson<FieldsTest>(json).intValue, Is.EqualTo(5));
                }
                using (var json = new StringReader("{\"intValue\":5,\"floatVal\":7.9}")) {
                    Assert.That(JsonMapper.FromJson<FieldsTest>(json).floatVal, Is.EqualTo(3.5f));
                }
            });
        }

        class PropertiesTest {
            public int intValue { get; set; } = 3;
            [JsonIgnore] public float floatVal { get; set; } = 3.5f;
        }

        [Test]
        public void IgnoreProperty() {
            Assert.Multiple(() => {
                using (var json = new StringReader("{\"intValue\":5}")) {
                    Assert.That(JsonMapper.FromJson<PropertiesTest>(json), Is.Not.Null);
                }
                using (var json = new StringReader("{\"intValue\":5}")) {
                    Assert.That(JsonMapper.FromJson<PropertiesTest>(json).intValue, Is.EqualTo(5));
                }
                using (var json = new StringReader("{\"intValue\":5}")) {
                    Assert.That(JsonMapper.FromJson<PropertiesTest>(json).floatVal, Is.EqualTo(3.5f));
                }
            });
        }

        [Test]
        public void IgnorePropertyEvenIfSpecified() {
            Assert.Multiple(() => {
                using (var json = new StringReader("{\"intValue\":5,\"floatVal\":7.9}")) {
                    Assert.That(JsonMapper.FromJson<PropertiesTest>(json), Is.Not.Null);
                }
                using (var json = new StringReader("{\"intValue\":5,\"floatVal\":7.9}")) {
                    Assert.That(JsonMapper.FromJson<PropertiesTest>(json).intValue, Is.EqualTo(5));
                }
                using (var json = new StringReader("{\"intValue\":5,\"floatVal\":7.9}")) {
                    Assert.That(JsonMapper.FromJson<PropertiesTest>(json).floatVal, Is.EqualTo(3.5f));
                }
            });
        }
    }
}
