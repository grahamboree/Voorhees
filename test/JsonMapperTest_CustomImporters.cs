using System;
using System.IO;
using NUnit.Framework;

namespace Voorhees.Tests {
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
            mapper.RegisterImporter(tokenReader => new TestType { PubIntVal = (int)tokenReader.ConsumeNumber() });

            Assert.Multiple(() => {
                using (var json = new StringReader("42")) {
                    Assert.That(mapper.Read<TestType>(new JsonTokenReader(json)), Is.Not.Null);
                }
                using (var json = new StringReader("42")) {
                    Assert.That(mapper.Read<TestType>(new JsonTokenReader(json)), Is.TypeOf<TestType>());
                }
                using (var json = new StringReader("42")) {
                    Assert.That(mapper.Read<TestType>(new JsonTokenReader(json)).PubIntVal, Is.EqualTo(42));
                }
            });
        }

        [Test]
        public void UnRegisterImporter() {
            var mapper = new JsonMapper();

            Assert.Multiple(() => {
                using (var json = new StringReader("42")) {
                    Assert.Throws<InvalidCastException>(() => mapper.Read<TestType>(new JsonTokenReader(json)));
                }

                mapper.RegisterImporter(tokenReader => new TestType { PubIntVal = (int)tokenReader.ConsumeNumber() });

                using (var json = new StringReader("42")) {
                    Assert.That(mapper.Read<TestType>(new JsonTokenReader(json)), Is.Not.Null);
                }
                using (var json = new StringReader("42")) {
                    Assert.That(mapper.Read<TestType>(new JsonTokenReader(json)), Is.TypeOf<TestType>());
                }
                using (var json = new StringReader("42")) {
                    Assert.That(mapper.Read<TestType>(new JsonTokenReader(json)).PubIntVal, Is.EqualTo(42));
                }

                mapper.UnRegisterImporter<TestType>();

                using (var json = new StringReader("42")) {
                    Assert.Throws<InvalidCastException>(() => mapper.Read<TestType>(new JsonTokenReader(json)));
                }
            });
        }

        [Test]
        public void UnregistersSingleImporter() {
            var mapper = new JsonMapper();
            mapper.RegisterImporter(tokenReader => new TestType { PubIntVal = (int)tokenReader.ConsumeNumber() });

            Assert.Multiple(() => {
                using (var json = new StringReader("42")) {
                    Assert.That(mapper.Read<TestType>(new JsonTokenReader(json)), Is.Not.Null);
                }
                using (var json = new StringReader("42")) {
                    Assert.That(mapper.Read<TestType>(new JsonTokenReader(json)), Is.TypeOf<TestType>());
                }
                using (var json = new StringReader("42")) {
                    Assert.That(mapper.Read<TestType>(new JsonTokenReader(json)).PubIntVal, Is.EqualTo(42));
                }

                mapper.UnRegisterAllImporters();

                using (var json = new StringReader("42")) {
                    Assert.Throws<InvalidCastException>(() => mapper.Read<TestType>(new JsonTokenReader(json)));
                }
            });
        }

        [Test]
        public void UnregistersMultipleImporters() {
            var mapper = new JsonMapper();
            mapper.RegisterImporter(tokenReader => new TestType { PubIntVal = (int)tokenReader.ConsumeNumber() });
            mapper.RegisterImporter(tokenReader => new TestType2 { PubString = tokenReader.ConsumeString() });

            Assert.Multiple(() => {
                using (var json = new StringReader("42")) {
                    Assert.That(mapper.Read<TestType>(new JsonTokenReader(json)), Is.Not.Null);
                }
                using (var json = new StringReader("42")) {
                    Assert.That(mapper.Read<TestType>(new JsonTokenReader(json)), Is.TypeOf<TestType>());
                }
                using (var json = new StringReader("42")) {
                    Assert.That(mapper.Read<TestType>(new JsonTokenReader(json)).PubIntVal, Is.EqualTo(42));
                }

                using (var json = new StringReader("\"test\"")) {
                    Assert.That(mapper.Read<TestType2>(new JsonTokenReader(json)), Is.Not.Null);
                }
                using (var json = new StringReader("\"test\"")) {
                    Assert.That(mapper.Read<TestType2>(new JsonTokenReader(json)), Is.TypeOf<TestType2>());
                }
                using (var json = new StringReader("\"test\"")) {
                    Assert.That(mapper.Read<TestType2>(new JsonTokenReader(json)).PubString, Is.EqualTo("test"));
                }

                mapper.UnRegisterAllImporters();

                using (var json = new StringReader("42")) {
                    Assert.Throws<InvalidCastException>(() => mapper.Read<TestType>(new JsonTokenReader(json)));
                }
                using (var json = new StringReader("\"test\"")) {
                    Assert.Throws<InvalidCastException>(() => mapper.Read<TestType2>(new JsonTokenReader(json)));
                }
            });
        }
    }
}
