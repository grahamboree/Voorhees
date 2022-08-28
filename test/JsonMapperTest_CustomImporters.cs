using System;
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
            mapper.RegisterImporter(tokenReader => new TestType {PubIntVal = int.Parse(tokenReader.ConsumeNumber())});
            
            Assert.That(mapper.Read<TestType>("42"), Is.Not.Null);
            Assert.That(mapper.Read<TestType>("42"), Is.TypeOf<TestType>());
            Assert.That(mapper.Read<TestType>("42").PubIntVal, Is.EqualTo(42));
        }

        [Test]
        public void UnRegisterImporter() {
            var mapper = new JsonMapper();
            Assert.Throws<InvalidCastException>(() => mapper.Read<TestType>("42"));
            
            mapper.RegisterImporter(tokenReader => new TestType {PubIntVal = int.Parse(tokenReader.ConsumeNumber())});
            
            Assert.That(mapper.Read<TestType>("42"), Is.Not.Null);
            Assert.That(mapper.Read<TestType>("42"), Is.TypeOf<TestType>());
            Assert.That(mapper.Read<TestType>("42").PubIntVal, Is.EqualTo(42));
            
            mapper.UnRegisterImporter<TestType>();
            
            Assert.Throws<InvalidCastException>(() => mapper.Read<TestType>("42"));
        }

        [Test]
        public void UnregistersSingleImporter() {
            var mapper = new JsonMapper();
            mapper.RegisterImporter(tokenReader => new TestType {PubIntVal = int.Parse(tokenReader.ConsumeNumber())});
            
            Assert.That(mapper.Read<TestType>("42"), Is.Not.Null);
            Assert.That(mapper.Read<TestType>("42"), Is.TypeOf<TestType>());
            Assert.That(mapper.Read<TestType>("42").PubIntVal, Is.EqualTo(42));

            mapper.UnRegisterAllImporters();
            
            Assert.Throws<InvalidCastException>(() => mapper.Read<TestType>("42"));
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
            
            Assert.Throws<InvalidCastException>(() => mapper.Read<TestType>("42"));
            Assert.Throws<InvalidCastException>(() => mapper.Read<TestType2>("\"test\""));
        }
    }
}
