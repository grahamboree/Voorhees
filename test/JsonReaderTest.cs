using NUnit.Framework;

namespace Voorhees.Tests {
	[TestFixture]
	public class JsonReaderTest_Primitives {
		[Test]
		public void Int() {
			var test = JsonReader.Read("1");
			Assert.That(test.Type, Is.EqualTo(JsonType.Int));
			Assert.That((int)test, Is.EqualTo(1));
		}

		[Test]
		public void NegativeInt() {
			var test = JsonReader.Read("-1");
			Assert.That(test.Type, Is.EqualTo(JsonType.Int));
			Assert.That((int)test, Is.EqualTo(-1));
		}
		
		[Test]
		public void Float() {
			var test = JsonReader.Read("1.5");
			Assert.That(test.Type, Is.EqualTo(JsonType.Float));
			Assert.That((float) test, Is.EqualTo(1.5f));
		}
		
		[Test]
		public void String() {
			var test = JsonReader.Read("\"test\"");
			Assert.That(test.Type, Is.EqualTo(JsonType.String));
			Assert.That((string) test, Is.EqualTo("test"));
		}
		
		[Test]
		public void True() {
			var test = JsonReader.Read("true");
			Assert.That(test.Type, Is.EqualTo(JsonType.Boolean));
			Assert.That((bool) test, Is.True);
		}

		[Test]
		public void False() {
			var test = JsonReader.Read("false");
			Assert.That(test.Type, Is.EqualTo(JsonType.Boolean));
			Assert.That((bool) test, Is.False);
		}
		
		[Test]
		public void Null() {
			var test = JsonReader.Read("null");
			Assert.That(test.Type, Is.EqualTo(JsonType.Null));
			Assert.That(test.IsNull);
		}
	}
	
	[TestFixture]
	public class JsonReaderTest_Arrays {
		[Test]
		public void EmptyArray() {
			var test = JsonReader.Read("[]");
			Assert.That(test.Type, Is.EqualTo(JsonType.Array));
			Assert.That(test.Count, Is.EqualTo(0));
		}

		[Test]
		public void SingleValueArray() {
			var test = JsonReader.Read("[1]");
			Assert.That(test.Type, Is.EqualTo(JsonType.Array));
			Assert.That(test.Count, Is.EqualTo(1));
			Assert.That(test[0].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int) test[0], Is.EqualTo(1));
		}

		[Test]
		public void MultiValueArray() {
			var test = JsonReader.Read("[1, 2, 3]");
			Assert.That(test.Type, Is.EqualTo(JsonType.Array));
			Assert.That(test.Count, Is.EqualTo(3));
			for (int i = 0; i < 3; ++i) {
				Assert.That(test[i].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) test[i], Is.EqualTo(i + 1));
			}
		}

		[Test]
		public void NestedArray() {
			var test = JsonReader.Read("[[1, 2], [3, 4]]");
			Assert.That(test.Type, Is.EqualTo(JsonType.Array));
			Assert.That(test.Count, Is.EqualTo(2));

			Assert.That(test[0].Type, Is.EqualTo(JsonType.Array));
			Assert.That(test[0].Count, Is.EqualTo(2));

			Assert.That(test[0][0].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int) test[0][0], Is.EqualTo(1));
			Assert.That(test[0][1].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int) test[0][1], Is.EqualTo(2));

			Assert.That(test[1][0].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int) test[1][0], Is.EqualTo(3));
			Assert.That(test[1][1].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int) test[1][1], Is.EqualTo(4));
		}

		[Test]
		public void MultipleNestedArrays() {
			var outer = JsonReader.Read("[[1, 2, 3], [4, [5]]]");
			Assert.That(outer.Type, Is.EqualTo(JsonType.Array));
			Assert.That(outer.Count, Is.EqualTo(2));
			JsonValue test = outer[0];

			Assert.That(test.Type, Is.EqualTo(JsonType.Array));
			Assert.That(test.Count, Is.EqualTo(3));
			for (int i = 0; i < 3; ++i) {
				Assert.That(test[i].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) test[i], Is.EqualTo(i + 1));
			}

			test = outer[1];
			Assert.That(test.Type, Is.EqualTo(JsonType.Array));
			Assert.That(test.Count, Is.EqualTo(2));
			Assert.That(test[0].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int) test[0], Is.EqualTo(4));

			test = test[1];
			Assert.That(test.Type, Is.EqualTo(JsonType.Array));
			Assert.That(test.Count, Is.EqualTo(1));
			Assert.That(test[0].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int) test[0], Is.EqualTo(5));
		}

		[Test]
		public void ArrayOfAllTypes() {
			var test = JsonReader.Read(@"[1, 1.5, ""test"", true, false, null, []]");
			Assert.That(test.Type, Is.EqualTo(JsonType.Array));
			Assert.That(test.Count, Is.EqualTo(7));

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
		}

		[Test]
		public void MissingArrayComma() {
			Assert.Throws<InvalidJsonException>(() => { JsonReader.Read("[1 2]"); });
		}

		[Test]
		public void ExtraLeadingArrayComma() {
			Assert.Throws<InvalidJsonException>(() => { JsonReader.Read("[,1, 2]"); });
		}

		[Test]
		public void ExtraSeparatingArrayComma() {
			Assert.Throws<InvalidJsonException>(() => { JsonReader.Read("[1,, 2]"); });
		}

		[Test]
		public void ExtraTrailingArrayComma() {
			Assert.Throws<InvalidJsonException>(() => { JsonReader.Read("[1, 2,]"); });
		}

		[Test]
		public void TooManyClosingArrayBrackets() {
			Assert.Throws<InvalidJsonException>(() => { JsonReader.Read("[1, 2]]"); });
		}

		[Test]
		public void TooFewClosingArrayBrackets() {
			Assert.Throws<InvalidJsonException>(() => { JsonReader.Read("[1, 2"); });
		}

		[Test]
		public void LeadingClosingArrayBracket() {
			Assert.Throws<InvalidJsonException>(() => { JsonReader.Read("]1, 2]"); });
		}
	}
	
	[TestFixture]
	public class JsonReaderTest_Objects {
		[Test]
		public void EmptyObject() {
			var test = JsonReader.Read("{}");
			Assert.That(test.Type, Is.EqualTo(JsonType.Object));
			Assert.That(test.Count, Is.EqualTo(0));
		}

		[Test]
		public void SimpleObject() {
			var test = JsonReader.Read("{\"test\": 1}");
			Assert.That(test.Type, Is.EqualTo(JsonType.Object));
			Assert.That(test.Count, Is.EqualTo(1));
			Assert.That(test.ContainsKey("test"), Is.True);
			Assert.That(test["test"].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int) test["test"], Is.EqualTo(1));
		}

		[Test]
		public void MultiObject() {
			var test = JsonReader.Read("{\"test\": 1, \"test2\": 2}");
			Assert.That(test.Type, Is.EqualTo(JsonType.Object));
			Assert.That(test.Count, Is.EqualTo(2));

			Assert.That(test.ContainsKey("test"), Is.True);
			Assert.That(test["test"].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int) test["test"], Is.EqualTo(1));

			Assert.That(test.ContainsKey("test2"), Is.True);
			Assert.That(test["test2"].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int) test["test2"], Is.EqualTo(2));
		}

		[Test]
		public void NestedObject() {
			var test = JsonReader.Read("{\"test\": {\"test2\": 1}}");
			Assert.That(test.Type, Is.EqualTo(JsonType.Object));
			Assert.That(test.Count, Is.EqualTo(1));
			Assert.That(test.ContainsKey("test"), Is.True);
			Assert.That(test["test"].Type, Is.EqualTo(JsonType.Object));
			Assert.That(test["test"].ContainsKey("test2"), Is.True);
			Assert.That(test["test"]["test2"].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int) test["test"]["test2"], Is.EqualTo(1));
		}

		[Test]
		public void ObjectMappingToArray() {
			var test = JsonReader.Read("{\"test\": [1, 2, 3]}");
			Assert.That(test.Type, Is.EqualTo(JsonType.Object));
			Assert.That(test.Count, Is.EqualTo(1));
			Assert.That(test.ContainsKey("test"), Is.True);
			Assert.That(test["test"].Type, Is.EqualTo(JsonType.Array));
			Assert.That(test["test"].Count, Is.EqualTo(3));

			for (int i = 0; i < 3; ++i) {
				Assert.That(test["test"][i].Type, Is.EqualTo(JsonType.Int));
				Assert.That((int) test["test"][i], Is.EqualTo(i + 1));
			}
		}

		[Test]
		public void LeadingObjectClosingBrace() {
			Assert.Throws<InvalidJsonException>(() => { JsonReader.Read("}\"test\": 1}"); });
		}
	}
}
