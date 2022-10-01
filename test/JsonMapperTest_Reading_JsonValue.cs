using System.IO;
using NUnit.Framework;

namespace Voorhees.Tests {
    [TestFixture]
	public class JsonValue_Reading_Primitives {
		[Test]
		public void Int() {
			using var json = new StringReader("1");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Int));
				Assert.That((int)test, Is.EqualTo(1));
			});
		}

		[Test]
		public void NegativeInt() {
			using var json = new StringReader("-1");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Int));
				Assert.That((int)test, Is.EqualTo(-1));
			});
		}
		
		[Test]
		public void Float() {
			using var json = new StringReader("1.5");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Float));
				Assert.That((float) test, Is.EqualTo(1.5f));
			});
		}
		
		[Test]
		public void String() {
			using var json = new StringReader("\"test\"");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.String));
				Assert.That((string) test, Is.EqualTo("test"));
			});
		}
		
		[Test]
		public void True() {
			using var json = new StringReader("true");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Boolean));
				Assert.That((bool) test, Is.True);
			});
		}

		[Test]
		public void False() {
			using var json = new StringReader("false");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Boolean));
				Assert.That((bool) test, Is.False);
			});
		}
		
		[Test]
		public void Null() {
			using var json = new StringReader("null");
			var test = JsonMapper.FromJson(json);
			Assert.That(test.Type, Is.EqualTo(JsonType.Null));
		}
	}
	
	[TestFixture]
	public class JsonValue_Reading_Arrays {
		[Test]
		public void EmptyArray() {
			using var json = new StringReader("[]");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Array));
				Assert.That(test.Count, Is.EqualTo(0));
			});
		}

		[Test]
		public void SingleValueArray() {
			using var json = new StringReader("[1]");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Array));
				Assert.That(test.Count, Is.EqualTo(1));
				Assert.That(test[0].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) test[0], Is.EqualTo(1));
			});
		}

		[Test]
		public void MultiValueArray() {
			using var json = new StringReader("[1, 2, 3]");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Array));
				Assert.That(test.Count, Is.EqualTo(3));
				Assert.That(test[0].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) test[0], Is.EqualTo(1));
				Assert.That(test[1].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) test[1], Is.EqualTo(2));
				Assert.That(test[2].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) test[2], Is.EqualTo(3));
			});
		}

		[Test]
		public void NestedArray() {
			using var json = new StringReader("[[1, 2], [3, 4]]");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Array));
				Assert.That(test.Count, Is.EqualTo(2));

				Assert.That(test[0].Type, Is.EqualTo(JsonType.Array));
				Assert.That(test[0].Count, Is.EqualTo(2));

				Assert.That(test[0][0].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int)test[0][0], Is.EqualTo(1));
				Assert.That(test[0][1].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int)test[0][1], Is.EqualTo(2));

				Assert.That(test[1][0].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int)test[1][0], Is.EqualTo(3));
				Assert.That(test[1][1].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int)test[1][1], Is.EqualTo(4));
			});
		}

		[Test]
		public void MultipleNestedArrays() {
			using var json = new StringReader("[[1, 2, 3], [4, [5]]]");
			var outer = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(outer.Type, Is.EqualTo(JsonType.Array));
				Assert.That(outer.Count, Is.EqualTo(2));

				Assert.That(outer[0].Type, Is.EqualTo(JsonType.Array));
				Assert.That(outer[0].Count, Is.EqualTo(3));
			
				Assert.That(outer[0][0].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) outer[0][0], Is.EqualTo(1));
				Assert.That(outer[0][1].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) outer[0][1], Is.EqualTo(2));
				Assert.That(outer[0][2].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) outer[0][2], Is.EqualTo(3));

				Assert.That(outer[1].Type, Is.EqualTo(JsonType.Array));
				Assert.That(outer[1].Count, Is.EqualTo(2));
				Assert.That(outer[1][0].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) outer[1][0], Is.EqualTo(4));

				Assert.That(outer[1][1].Type, Is.EqualTo(JsonType.Array));
				Assert.That(outer[1][1].Count, Is.EqualTo(1));
				Assert.That(outer[1][1][0].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) outer[1][1][0], Is.EqualTo(5));
			});
		}

		[Test]
		public void ArrayOfAllTypes() {
			using var json = new StringReader(@"[1, 1.5, ""test"", true, false, null, [], {}]");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Array));
				Assert.That(test.Count, Is.EqualTo(8));

				Assert.That(test[0].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) test[0], Is.EqualTo(1));
				Assert.That(test[1].Type, Is.EqualTo(JsonType.Float));
				Assert.That((float) test[1], Is.EqualTo(1.5));
				Assert.That(test[2].Type, Is.EqualTo(JsonType.String));
				Assert.That((string) test[2], Is.EqualTo("test"));
				Assert.That(test[3].Type, Is.EqualTo(JsonType.Boolean));
				Assert.That((bool) test[3], Is.True);
				Assert.That(test[4].Type, Is.EqualTo(JsonType.Boolean));
				Assert.That((bool) test[4], Is.False);
				Assert.That(test[5].Type, Is.EqualTo(JsonType.Null));
				Assert.That(test[6].Type, Is.EqualTo(JsonType.Array));
				Assert.That(test[6].Count, Is.EqualTo(0));
				Assert.That(test[7].Type, Is.EqualTo(JsonType.Object));
				Assert.That(test[7].Count, Is.EqualTo(0));
			});
		}
		
		[Test]
		public void JustArrayComma() {
			using var json = new StringReader("[,]");
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson(json); });
		}
		
		[Test]
		public void JustMinusArray() {
			using var json = new StringReader("[-]");
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson(json); });
		}

		[Test]
		public void MissingArrayComma() {
			using var json = new StringReader("[1 2]");
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson(json); });
		}

		[Test]
		public void ExtraLeadingArrayComma() {
			using var json = new StringReader("[,1, 2]");
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson(json); });
		}

		[Test]
		public void ExtraSeparatingArrayComma() {
			using var json = new StringReader("[1,, 2]");
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson(json); });
		}

		[Test]
		public void ExtraTrailingArrayComma() {
			using var json = new StringReader("[1, 2,]");
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson(json); });
		}

		[Test]
		public void TooManyClosingArrayBrackets() {
			using var json = new StringReader("[1, 2]]");
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson(json); });
		}

		[Test]
		public void TooFewClosingArrayBrackets() {
			using var json = new StringReader("[1, 2");
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson(json); });
		}

		[Test]
		public void LeadingClosingArrayBracket() {
			using var json = new StringReader("]1, 2]");
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson(json); });
		}
	}
	
	[TestFixture]
	public class JsonValue_Reading_Objects {
		[Test]
		public void EmptyObject() {
			using var json = new StringReader("{}");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Object));
				Assert.That(test.Count, Is.EqualTo(0));
			});
		}

		[Test]
		public void SimpleObject() {
			using var json = new StringReader("{\"test\": 1}");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Object));
				Assert.That(test.Count, Is.EqualTo(1));
				Assert.That(test.ContainsKey("test"), Is.True);
				Assert.That(test["test"].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int)test["test"], Is.EqualTo(1));
			});
		}

		[Test]
		public void MultiObject() {
			using var json = new StringReader("{\"test\": 1, \"test2\": 2}");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Object));
				Assert.That(test.Count, Is.EqualTo(2));

				Assert.That(test.ContainsKey("test"), Is.True);
				Assert.That(test["test"].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int)test["test"], Is.EqualTo(1));

				Assert.That(test.ContainsKey("test2"), Is.True);
				Assert.That(test["test2"].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int)test["test2"], Is.EqualTo(2));
			});
		}

		[Test]
		public void NestedObject() {
			using var json = new StringReader("{\"test\": {\"test2\": 1}}");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Object));
				Assert.That(test.Count, Is.EqualTo(1));
				Assert.That(test.ContainsKey("test"), Is.True);
				Assert.That(test["test"].Type, Is.EqualTo(JsonType.Object));
				Assert.That(test["test"].ContainsKey("test2"), Is.True);
				Assert.That(test["test"]["test2"].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int)test["test"]["test2"], Is.EqualTo(1));
			});
		}

        [Test]
		public void ObjectMappingToArray() {
			using var json = new StringReader("{\"test\": [1, 2, 3]}");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Object));
				Assert.That(test.Count, Is.EqualTo(1));
				Assert.That(test.ContainsKey("test"), Is.True);
				Assert.That(test["test"].Type, Is.EqualTo(JsonType.Array));
				Assert.That(test["test"].Count, Is.EqualTo(3));

				Assert.That(test["test"][0].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) test["test"][0], Is.EqualTo(1));
				Assert.That(test["test"][1].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) test["test"][1], Is.EqualTo(2));
				Assert.That(test["test"][2].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) test["test"][2], Is.EqualTo(3));
			});
		}

		[Test]
		public void LeadingObjectClosingBrace() {
			using var json = new StringReader("}\"test\": 1}");
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson(json); });
		}

		[Test]
		public void ObjectWithDuplicateKeysPrefersTheLastOccurenceOfTheKey() {
			using var json = new StringReader("{\"a\":\"b\",\"a\":\"c\"}");
			var test = JsonMapper.FromJson(json);
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonType.Object));
				Assert.That(test.Count, Is.EqualTo(1));
				Assert.That(test.ContainsKey("a"), Is.True);
				Assert.That(test["a"].Type, Is.EqualTo(JsonType.String));
				Assert.That((string)test["a"], Is.EqualTo("c")); // c since it's the last value for the key "a" appearing in the json
			});
		}
	}
}