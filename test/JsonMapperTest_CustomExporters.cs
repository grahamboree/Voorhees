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
            var instance = new TestType { PubIntVal = 42 };
            Assert.That(mapper.Write(instance), Is.EqualTo("42"));
        }

        [Test]
        public void UnRegisterExporter() {
            var mapper = new JsonMapper();
            var instance = new TestType { PubIntVal = 42 };
            mapper.RegisterExporter<TestType>((t, os) => os.Write(t.PubIntVal));
            Assert.That(mapper.Write(instance), Is.EqualTo("42"));
            mapper.UnRegisterExporter<TestType>();
            Assert.That(mapper.Write(instance), Is.EqualTo("{\"PubIntVal\":42}"));
        }

        [Test]
        public void UnregistersSingleExporter() {
            var mapper = new JsonMapper();
            var instance = new TestType { PubIntVal = 42 };

            mapper.RegisterExporter<TestType>((t, os) => os.Write(t.PubIntVal));
            Assert.That(mapper.Write(instance), Is.EqualTo("42"));

            mapper.UnRegisterAllExporters();
            Assert.That(mapper.Write(instance), Is.EqualTo("{\"PubIntVal\":42}"));
        }

        [Test]
        public void UnregistersMultipleExporters() {
            var mapper = new JsonMapper();

            var instance1 = new TestType { PubIntVal = 42 };
            var instance2 = new TestType2 { PubString = "hello" };

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
}