using System;
using NUnit.Framework;

namespace Voorhees.Tests {
    [TestFixture]
    public class JsonMapper_CustomExporters {
        class TestType {
            public int PubIntVal;
        }

        class TestType2 {
            public string PubString;
        }

        [Test]
        public void RegisterExporter() {
            var mapper = new JsonMapper();
            mapper.RegisterExporter<TestType>((t, tokenWriter) => tokenWriter.Write(t.PubIntVal));
            var instance = new TestType {PubIntVal = 42};
            Assert.That(mapper.Write(instance), Is.EqualTo("42"));
        }

        [Test]
        public void UnRegisterExporter() {
            var mapper = new JsonMapper();
            var instance = new TestType {PubIntVal = 42};
            mapper.RegisterExporter<TestType>((t, os) => os.Write(t.PubIntVal));
            Assert.That(mapper.Write(instance), Is.EqualTo("42"));
            mapper.UnRegisterExporter<TestType>();
            Assert.That(mapper.Write(instance), Is.EqualTo("{\"PubIntVal\":42}"));
        }

        [Test]
        public void UnregistersSingleExporter() {
            var mapper = new JsonMapper();
            var instance = new TestType {PubIntVal = 42};

            mapper.RegisterExporter<TestType>((t, os) => os.Write(t.PubIntVal));
            Assert.That(mapper.Write(instance), Is.EqualTo("42"));

            mapper.UnRegisterAllExporters();
            Assert.That(mapper.Write(instance), Is.EqualTo("{\"PubIntVal\":42}"));
        }

        [Test]
        public void UnregistersMultipleExporters() {
            var mapper = new JsonMapper();
            
            var instance1 = new TestType {PubIntVal = 42};
            var instance2 = new TestType2 {PubString = "hello"};

            mapper.RegisterExporter<TestType>((t, os) => os.Write(t.PubIntVal));
            Assert.That(mapper.Write(instance1), Is.EqualTo("42"));
            Assert.That(mapper.Write(instance2), Is.EqualTo("{\"PubString\":\"hello\"}"));

            mapper.RegisterExporter<TestType2>((t, os) => os.Write(t.PubString.ToUpper()));
            Assert.That(mapper.Write(instance1), Is.EqualTo("42"));
            Assert.That(mapper.Write(instance2), Is.EqualTo("\"HELLO\""));

            mapper.UnRegisterAllExporters();
            Assert.That(mapper.Write(instance1), Is.EqualTo("{\"PubIntVal\":42}"));
            Assert.That(mapper.Write(instance2), Is.EqualTo("{\"PubString\":\"hello\"}"));
        }
    }

    [TestFixture]
    public class JsonMapper_CustomImporters {
        class TestType {
            public int PubIntVal;
        }

        class TestType2 {
            public string PubString;
        }

        [Test]
        public void RegisterImporter() {
            var mapper = new JsonMapper();
            mapper.RegisterImporter(tokenReader => new TestType {PubIntVal = int.Parse(tokenReader.ConsumeNumber())});
            
            Assert.That(mapper.Read<TestType>("42"), Is.Not.Null);
            Assert.That(mapper.Read<TestType>("42"), Is.TypeOf<TestType>());
            Assert.That(mapper.Read<TestType>("42").PubIntVal, Is.EqualTo(42));
        }

        [Test]
        public void UnRegisterImporter() {
            var mapper = new JsonMapper();
            Assert.Throws<Exception>(() => mapper.Read<TestType>("42"));
            
            mapper.RegisterImporter(tokenReader => new TestType {PubIntVal = int.Parse(tokenReader.ConsumeNumber())});
            
            Assert.That(mapper.Read<TestType>("42"), Is.Not.Null);
            Assert.That(mapper.Read<TestType>("42"), Is.TypeOf<TestType>());
            Assert.That(mapper.Read<TestType>("42").PubIntVal, Is.EqualTo(42));
            
            mapper.UnRegisterImporter<TestType>();
            
            Assert.Throws<Exception>(() => mapper.Read<TestType>("42"));
        }

        [Test]
        public void UnregistersSingleImporter() {
            var mapper = new JsonMapper();
            mapper.RegisterImporter(tokenReader => new TestType {PubIntVal = int.Parse(tokenReader.ConsumeNumber())});
            
            Assert.That(mapper.Read<TestType>("42"), Is.Not.Null);
            Assert.That(mapper.Read<TestType>("42"), Is.TypeOf<TestType>());
            Assert.That(mapper.Read<TestType>("42").PubIntVal, Is.EqualTo(42));

            mapper.UnRegisterAllImporters();
            
            Assert.Throws<Exception>(() => mapper.Read<TestType>("42"));
        }

        [Test]
        public void UnregistersMultipleImporters() {
            var mapper = new JsonMapper();
            mapper.RegisterImporter(tokenReader => new TestType {PubIntVal = int.Parse(tokenReader.ConsumeNumber())});
            mapper.RegisterImporter(tokenReader => new TestType2 {PubString = tokenReader.ConsumeString()});

            Assert.That(mapper.Read<TestType>("42"), Is.Not.Null);
            Assert.That(mapper.Read<TestType>("42"), Is.TypeOf<TestType>());
            Assert.That(mapper.Read<TestType>("42").PubIntVal, Is.EqualTo(42));

            Assert.That(mapper.Read<TestType2>("\"test\""), Is.Not.Null);
            Assert.That(mapper.Read<TestType2>("\"test\""), Is.TypeOf<TestType2>());
            Assert.That(mapper.Read<TestType2>("\"test\"").PubString, Is.EqualTo("test"));

            mapper.UnRegisterAllImporters();
            
            Assert.Throws<Exception>(() => mapper.Read<TestType>("42"));
            Assert.Throws<Exception>(() => mapper.Read<TestType2>("\"test\""));
        }
    }
}
