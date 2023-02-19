using NUnit.Framework;

namespace Voorhees.Tests {
    [TestFixture]
    public class JsonValue_Writing_Primitives {
        [Test]
        public void Int() {
            var test = new JsonValue(1);
            Assert.That(JsonMapper.ToJson(test), Is.EqualTo("1"));
        }

        [Test]
        public void NegativeInt() {
            var test = new JsonValue(-1);
            Assert.That(JsonMapper.ToJson(test), Is.EqualTo("-1"));
        }

        [Test]
        public void Float() {
            var test = new JsonValue(1.5f);
            Assert.That(JsonMapper.ToJson(test), Is.EqualTo("1.5"));
        }

        [Test]
        public void String() {
            var test = new JsonValue("test");
            Assert.That(JsonMapper.ToJson(test), Is.EqualTo("\"test\""));
        }

        [Test]
        public void True() {
            var test = new JsonValue(true);
            Assert.That(JsonMapper.ToJson(test), Is.EqualTo("true"));
        }

        [Test]
        public void False() {
            var test = new JsonValue(false);
            Assert.That(JsonMapper.ToJson(test), Is.EqualTo("false"));
        }

        [Test]
        public void Null() {
            var test = new JsonValue(null);
            Assert.That(JsonMapper.ToJson(test), Is.EqualTo("null"));
        }
    }
    
    [TestFixture]
	public class JsonValue_Writing_Arrays {
		[Test]
		public void EmptyArray() {
			var test = new JsonValue(JsonValueType.Array);
			Assert.That(JsonMapper.ToJson(test), Is.EqualTo("[]"));
		}

		[Test]
		public void SingleValueArray() {
			var test = new JsonValue { 1 };
			Assert.That(JsonMapper.ToJson(test), Is.EqualTo("[1]"));
		}

		[Test]
		public void MultiValueArray() {
			var test = new JsonValue { 1, 2, 3 };
			Assert.That(JsonMapper.ToJson(test), Is.EqualTo("[1,2,3]"));
		}

		[Test]
		public void NestedArray() {
			var test = new JsonValue { new JsonValue { 1, 2 }, new JsonValue { 3, 4 } };
			Assert.That(JsonMapper.ToJson(test), Is.EqualTo("[[1,2],[3,4]]"));
		}

		[Test]
		public void MultipleNestedArrays() {
			var test = new JsonValue { new JsonValue { 1, 2, 3 }, new JsonValue { 4, new JsonValue { 5 } } };
			Assert.That(JsonMapper.ToJson(test), Is.EqualTo("[[1,2,3],[4,[5]]]"));
		}

		[Test]
		public void ArrayOfAllTypes() {
			var test = new JsonValue {
				1,
				1.5f,
				"test",
				true,
				false,
				null,
				new JsonValue(JsonValueType.Array),
				new JsonValue(JsonValueType.Object)
			};
			Assert.That(JsonMapper.ToJson(test), Is.EqualTo("[1,1.5,\"test\",true,false,null,[],{}]"));
		}
	}

    [TestFixture]
    public class JsonValue_Writing_Objects {
        [Test]
		public void EmptyObject() {
			var test = new JsonValue(JsonValueType.Object);
			Assert.That(JsonMapper.ToJson(test), Is.EqualTo("{}"));
		}

		[Test]
		public void SimpleObject() {
			var test = new JsonValue { {"test", 1} };
			Assert.That(JsonMapper.ToJson(test), Is.EqualTo("{\"test\":1}"));
		}

		[Test]
		public void MultiObject() {
			var test = new JsonValue { {"test", 1}, {"test2", 2} };
			Assert.That(JsonMapper.ToJson(test), Is.EqualTo("{\"test\":1,\"test2\":2}"));
		}

		[Test]
		public void NestedObject() {
			var test = new JsonValue { { "test", new JsonValue { { "test2", 1 } } } };
			Assert.That(JsonMapper.ToJson(test), Is.EqualTo("{\"test\":{\"test2\":1}}"));
		}

        [Test]
		public void ArrayValue() {
			var test = new JsonValue { { "test", new JsonValue { 1, 2, 3 } } };
			Assert.That(JsonMapper.ToJson(test), Is.EqualTo("{\"test\":[1,2,3]}"));
		}
    }
}