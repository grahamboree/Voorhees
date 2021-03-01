using System;
using NUnit.Framework;

namespace Voorhees.Tests {
    [TestFixture]
    public class JsonConfig_RegisterExporter {
        class TestType {
            public int PubIntVal;
        }

        [SetUp]
        public void SetUp() {
            JsonConfig.CurrentConfig.RegisterExporter<TestType>((t, os) => os.Write(t.PubIntVal));
        }

        [TearDown]
        public void TearDown() {
            JsonConfig.CurrentConfig.UnRegisterExporter<TestType>();
        }

        [Test]
        public void RegisterExporter() {
            var instance = new TestType {PubIntVal = 42};
            const string expected = "42";
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo(expected));
        }
    }

    [TestFixture]
    public class JsonConfig_UnRegisterExporter {
        class TestType {
            public int PubIntVal;
        }

        [TearDown]
        public void TearDown() {
            JsonConfig.CurrentConfig.UnRegisterExporter<TestType>();
        }

        [Test]
        public void UnRegisterExporter() {
            var instance = new TestType {PubIntVal = 42};

            JsonConfig.CurrentConfig.RegisterExporter<TestType>((t, os) => os.Write(t.PubIntVal));
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo("42"));

            JsonConfig.CurrentConfig.UnRegisterExporter<TestType>();
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo("{\"PubIntVal\":42}"));
        }
    }

    [TestFixture]
    public class JsonConfig_UnRegisterAllExporters {
        class TestType {
            public int PubIntVal;
        }

        class TestType2 {
            public string PubString;
        }

        [TearDown]
        public void TearDown() {
            JsonConfig.CurrentConfig.UnRegisterExporter<TestType>();
        }

        [Test]
        public void UnregistersSingleExporter() {
            var instance = new TestType {PubIntVal = 42};

            JsonConfig.CurrentConfig.RegisterExporter<TestType>((t, os) => os.Write(t.PubIntVal));
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo("42"));

            JsonConfig.CurrentConfig.UnRegisterAllExporters();
            Assert.That(JsonMapper.ToJson(instance), Is.EqualTo("{\"PubIntVal\":42}"));
        }

        [Test]
        public void UnregistersMultipleExporters() {
            var instance1 = new TestType {PubIntVal = 42};
            var instance2 = new TestType2 {PubString = "hello"};

            JsonConfig.CurrentConfig.RegisterExporter<TestType>((t, os) => os.Write(t.PubIntVal));
            Assert.That(JsonMapper.ToJson(instance1), Is.EqualTo("42"));
            Assert.That(JsonMapper.ToJson(instance2), Is.EqualTo("{\"PubString\":\"hello\"}"));

            JsonConfig.CurrentConfig.RegisterExporter<TestType2>((t, os) => os.Write(t.PubString.ToUpper()));
            Assert.That(JsonMapper.ToJson(instance1), Is.EqualTo("42"));
            Assert.That(JsonMapper.ToJson(instance2), Is.EqualTo("\"HELLO\""));

            JsonConfig.CurrentConfig.UnRegisterAllExporters();
            Assert.That(JsonMapper.ToJson(instance1), Is.EqualTo("{\"PubIntVal\":42}"));
            Assert.That(JsonMapper.ToJson(instance2), Is.EqualTo("{\"PubString\":\"hello\"}"));
        }
    }

    [TestFixture]
    public class JsonConfig_RegisterImporter {
        class TestType {
            public int PubIntVal;
        }

        [SetUp]
        public void SetUp() {
            JsonConfig.CurrentConfig.RegisterImporter<int, TestType>(json =>
                new TestType {PubIntVal = json}
            );
        }

        [TearDown]
        public void TearDown() {
            JsonConfig.CurrentConfig.UnRegisterImporter<int, TestType>();
        }

        [Test]
        public void RegisterImporter() {
            Assert.That(JsonMapper.FromJson<TestType>("42"), Is.Not.Null);
            Assert.That(JsonMapper.FromJson<TestType>("42"), Is.TypeOf<TestType>());
            Assert.That(JsonMapper.FromJson<TestType>("42").PubIntVal, Is.EqualTo(42));
        }
    }
    
    [TestFixture]
    public class JsonConfig_UnRegisterImporter {
        class TestType {
            public int PubIntVal;
        }

        [TearDown]
        public void TearDown() {
            JsonConfig.CurrentConfig.UnRegisterImporter<int, TestType>();
        }

        [Test]
        public void UnRegisterImporter() {
            JsonConfig.CurrentConfig.RegisterImporter<int, TestType>(i => new TestType {PubIntVal = i});
            
            Assert.That(JsonMapper.FromJson<TestType>("42"), Is.Not.Null);
            Assert.That(JsonMapper.FromJson<TestType>("42"), Is.TypeOf<TestType>());
            Assert.That(JsonMapper.FromJson<TestType>("42").PubIntVal, Is.EqualTo(42));
            
            JsonConfig.CurrentConfig.UnRegisterImporter<int, TestType>();
            
            Assert.Throws<Exception>(() => JsonMapper.FromJson<TestType>("42"));
        }
    }

    [TestFixture]
    public class JsonConfig_UnRegisterAllImporters {
        class TestType {
            public int PubIntVal;
        }

        class TestType2 {
            public string PubString;
        }

        [Test]
        public void UnregistersSingleImporter() {
            JsonConfig.CurrentConfig.RegisterImporter<int, TestType>(i => new TestType {PubIntVal = i});
            
            Assert.That(JsonMapper.FromJson<TestType>("42"), Is.Not.Null);
            Assert.That(JsonMapper.FromJson<TestType>("42"), Is.TypeOf<TestType>());
            Assert.That(JsonMapper.FromJson<TestType>("42").PubIntVal, Is.EqualTo(42));

            JsonConfig.CurrentConfig.UnRegisterAllImporters();
            
            Assert.Throws<Exception>(() => JsonMapper.FromJson<TestType>("42"));
        }

        [Test]
        public void UnregistersMultipleImporters() {
            JsonConfig.CurrentConfig.RegisterImporter<int, TestType>(i => new TestType {PubIntVal = i});
            JsonConfig.CurrentConfig.RegisterImporter<string, TestType2>(s => new TestType2 { PubString = s});

            Assert.That(JsonMapper.FromJson<TestType>("42"), Is.Not.Null);
            Assert.That(JsonMapper.FromJson<TestType>("42"), Is.TypeOf<TestType>());
            Assert.That(JsonMapper.FromJson<TestType>("42").PubIntVal, Is.EqualTo(42));

            Assert.That(JsonMapper.FromJson<TestType2>("\"test\""), Is.Not.Null);
            Assert.That(JsonMapper.FromJson<TestType2>("\"test\""), Is.TypeOf<TestType2>());
            Assert.That(JsonMapper.FromJson<TestType2>("\"test\"").PubString, Is.EqualTo("test"));

            JsonConfig.CurrentConfig.UnRegisterAllImporters();
            
            Assert.Throws<Exception>(() => JsonMapper.FromJson<TestType>("42"));
            Assert.Throws<Exception>(() => JsonMapper.FromJson<TestType2>("\"test\""));
        }
    }
}
