using System.IO;
using System.Text;
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
            var instance = new TestType { PubIntVal = 42 };
            
            // Register the exporter.
            var mapper = new JsonMapper();
            mapper.RegisterExporter<TestType>((t, tokenWriter) => tokenWriter.Write(t.PubIntVal));
            
            // Serialize the value.
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder)) {
                mapper.Write(instance, new JsonTokenWriter(stringWriter, false));
            }
            
            Assert.That(stringBuilder.ToString(), Is.EqualTo("42"));
        }

        [Test]
        public void UnRegisterExporter() {
            var instance = new TestType { PubIntVal = 42 };
            
            var mapper = new JsonMapper();
            mapper.RegisterExporter<TestType>((t, os) => os.Write(t.PubIntVal));
            
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder)) {
                mapper.Write(instance, new JsonTokenWriter(stringWriter, false));
            }
            Assert.That(stringBuilder.ToString(), Is.EqualTo("42"));
            
            mapper.UnRegisterExporter<TestType>();
            stringBuilder.Length = 0; // Clear the string builder
            using (var stringWriter = new StringWriter(stringBuilder)) {
                mapper.Write(instance, new JsonTokenWriter(stringWriter, false));
            }
            Assert.That(stringBuilder.ToString(), Is.EqualTo("{\"PubIntVal\":42}"));
        }

        [Test]
        public void UnregistersSingleExporter() {
            var instance = new TestType { PubIntVal = 42 };
            
            var mapper = new JsonMapper();
            mapper.RegisterExporter<TestType>((t, os) => os.Write(t.PubIntVal));
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder)) {
                mapper.Write(instance, new JsonTokenWriter(stringWriter, false));
            }
            Assert.That(stringBuilder.ToString(), Is.EqualTo("42"));

            mapper.UnRegisterAllExporters();
            stringBuilder.Length = 0; // Clear the string builder
            using (var stringWriter = new StringWriter(stringBuilder)) {
                mapper.Write(instance, new JsonTokenWriter(stringWriter, false));
            }
            Assert.That(stringBuilder.ToString(), Is.EqualTo("{\"PubIntVal\":42}"));
        }

        [Test]
        public void UnregistersMultipleExporters() {
            var instance1 = new TestType { PubIntVal = 42 };
            var instance2 = new TestType2 { PubString = "hello" };

            var mapper = new JsonMapper();
            
            Assert.Multiple(() => {
                mapper.RegisterExporter<TestType>((t, os) => os.Write(t.PubIntVal));
                var stringBuilder = new StringBuilder();
                using (var stringWriter = new StringWriter(stringBuilder)) {
                    mapper.Write(instance1, new JsonTokenWriter(stringWriter, false));
                }
                Assert.That(stringBuilder.ToString(), Is.EqualTo("42"));
                
                stringBuilder.Length = 0; // Clear the string builder
                using (var stringWriter = new StringWriter(stringBuilder)) {
                    mapper.Write(instance2, new JsonTokenWriter(stringWriter, false));
                }
                Assert.That(stringBuilder.ToString(), Is.EqualTo("{\"PubString\":\"hello\"}"));

                mapper.RegisterExporter<TestType2>((t, os) => os.Write(t.PubString.ToUpper()));
                stringBuilder.Length = 0; // Clear the string builder
                using (var stringWriter = new StringWriter(stringBuilder)) {
                    mapper.Write(instance1, new JsonTokenWriter(stringWriter, false));
                }
                Assert.That(stringBuilder.ToString(), Is.EqualTo("42"));
                
                stringBuilder.Length = 0; // Clear the string builder
                using (var stringWriter = new StringWriter(stringBuilder)) {
                    mapper.Write(instance2, new JsonTokenWriter(stringWriter, false));
                }
                Assert.That(stringBuilder.ToString(), Is.EqualTo("\"HELLO\""));

                mapper.UnRegisterAllExporters();
                stringBuilder.Length = 0; // Clear the string builder
                using (var stringWriter = new StringWriter(stringBuilder)) {
                    mapper.Write(instance1, new JsonTokenWriter(stringWriter, false));
                }
                Assert.That(stringBuilder.ToString(), Is.EqualTo("{\"PubIntVal\":42}"));
                
                stringBuilder.Length = 0; // Clear the string builder
                using (var stringWriter = new StringWriter(stringBuilder)) {
                    mapper.Write(instance2, new JsonTokenWriter(stringWriter, false));
                }
                Assert.That(stringBuilder.ToString(), Is.EqualTo("{\"PubString\":\"hello\"}"));
            });
        }
    }
}
