using NUnit.Framework;

namespace Voorhees.Tests {
    [TestFixture]
	public class JsonValue_Reading_Primitives {
		[Test]
		public void Int() {
			var test = JsonMapper.FromJson<JsonValue>("1");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)test, Is.EqualTo(1));
			});
		}

		[Test]
		public void NegativeInt() {
			var test = JsonMapper.FromJson<JsonValue>("-1");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)test, Is.EqualTo(-1));
			});
		}
		
		[Test]
		public void Float() {
			var test = JsonMapper.FromJson<JsonValue>("1.5");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Double));
				Assert.That((double) test, Is.EqualTo(1.5));
			});
		}
		
		[Test]
		public void String() {
			var test = JsonMapper.FromJson<JsonValue>("\"test\"");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.String));
				Assert.That((string) test, Is.EqualTo("test"));
			});
		}
		
		[Test]
		public void True() {
			var test = JsonMapper.FromJson<JsonValue>("true");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Boolean));
				Assert.That((bool) test, Is.True);
			});
		}

		[Test]
		public void False() {
			var test = JsonMapper.FromJson<JsonValue>("false");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Boolean));
				Assert.That((bool) test, Is.False);
			});
		}
		
		[Test]
		public void Null() {
			var test = JsonMapper.FromJson<JsonValue>("null");
			Assert.That(test.Type, Is.EqualTo(JsonValueType.Null));
		}
	}
	
	[TestFixture]
	public class JsonValue_Reading_Arrays {
		[Test]
		public void EmptyArray() {
			var test = JsonMapper.FromJson<JsonValue>("[]");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Array));
				Assert.That(test.Count, Is.EqualTo(0));
			});
		}

		[Test]
		public void SingleValueArray() {
			var test = JsonMapper.FromJson<JsonValue>("[1]");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Array));
				Assert.That(test.Count, Is.EqualTo(1));
				Assert.That(test[0].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int) test[0], Is.EqualTo(1));
			});
		}

		[Test]
		public void MultiValueArray() {
			var test = JsonMapper.FromJson<JsonValue>("[1, 2, 3]");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Array));
				Assert.That(test.Count, Is.EqualTo(3));
				Assert.That(test[0].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int) test[0], Is.EqualTo(1));
				Assert.That(test[1].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int) test[1], Is.EqualTo(2));
				Assert.That(test[2].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int) test[2], Is.EqualTo(3));
			});
		}

		[Test]
		public void NestedArray() {
			var test = JsonMapper.FromJson<JsonValue>("[[1, 2], [3, 4]]");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Array));
				Assert.That(test.Count, Is.EqualTo(2));

				Assert.That(test[0].Type, Is.EqualTo(JsonValueType.Array));
				Assert.That(test[0].Count, Is.EqualTo(2));

				Assert.That(test[0][0].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)test[0][0], Is.EqualTo(1));
				Assert.That(test[0][1].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)test[0][1], Is.EqualTo(2));

				Assert.That(test[1][0].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)test[1][0], Is.EqualTo(3));
				Assert.That(test[1][1].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)test[1][1], Is.EqualTo(4));
			});
		}

		[Test]
		public void MultipleNestedArrays() {
			var outer = JsonMapper.FromJson<JsonValue>("[[1, 2, 3], [4, [5]]]");
			Assert.Multiple(() => {
				Assert.That(outer.Type, Is.EqualTo(JsonValueType.Array));
				Assert.That(outer.Count, Is.EqualTo(2));

				Assert.That(outer[0].Type, Is.EqualTo(JsonValueType.Array));
				Assert.That(outer[0].Count, Is.EqualTo(3));
			
				Assert.That(outer[0][0].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int) outer[0][0], Is.EqualTo(1));
				Assert.That(outer[0][1].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int) outer[0][1], Is.EqualTo(2));
				Assert.That(outer[0][2].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int) outer[0][2], Is.EqualTo(3));

				Assert.That(outer[1].Type, Is.EqualTo(JsonValueType.Array));
				Assert.That(outer[1].Count, Is.EqualTo(2));
				Assert.That(outer[1][0].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int) outer[1][0], Is.EqualTo(4));

				Assert.That(outer[1][1].Type, Is.EqualTo(JsonValueType.Array));
				Assert.That(outer[1][1].Count, Is.EqualTo(1));
				Assert.That(outer[1][1][0].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int) outer[1][1][0], Is.EqualTo(5));
			});
		}

		[Test]
		public void ArrayOfAllTypes() {
			var test = JsonMapper.FromJson<JsonValue>(@"[1, 1.5, ""test"", true, false, null, [], {}]");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Array));
				Assert.That(test.Count, Is.EqualTo(8));

				Assert.That(test[0].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int) test[0], Is.EqualTo(1));
				Assert.That(test[1].Type, Is.EqualTo(JsonValueType.Double));
				Assert.That((double) test[1], Is.EqualTo(1.5));
				Assert.That(test[2].Type, Is.EqualTo(JsonValueType.String));
				Assert.That((string) test[2], Is.EqualTo("test"));
				Assert.That(test[3].Type, Is.EqualTo(JsonValueType.Boolean));
				Assert.That((bool) test[3], Is.True);
				Assert.That(test[4].Type, Is.EqualTo(JsonValueType.Boolean));
				Assert.That((bool) test[4], Is.False);
				Assert.That(test[5].Type, Is.EqualTo(JsonValueType.Null));
				Assert.That(test[6].Type, Is.EqualTo(JsonValueType.Array));
				Assert.That(test[6].Count, Is.EqualTo(0));
				Assert.That(test[7].Type, Is.EqualTo(JsonValueType.Object));
				Assert.That(test[7].Count, Is.EqualTo(0));
			});
		}
		
		[Test]
		public void JustArrayComma() {
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson<JsonValue>("[,]"); });
		}
		
		[Test]
		public void JustMinusArray() {
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson<JsonValue>("[-]"); });
		}

		[Test]
		public void MissingArrayComma() {
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson<JsonValue>("[1 2]"); });
		}

		[Test]
		public void ExtraLeadingArrayComma() {
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson<JsonValue>("[,1, 2]"); });
		}

		[Test]
		public void ExtraSeparatingArrayComma() {
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson<JsonValue>("[1,, 2]"); });
		}

		[Test]
		public void ExtraTrailingArrayComma() {
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson<JsonValue>("[1, 2,]"); });
		}

		[Test]
		public void TooManyClosingArrayBrackets() {
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson<JsonValue>("[1, 2]]"); });
		}

		[Test]
		public void TooFewClosingArrayBrackets() {
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson<JsonValue>("[1, 2"); });
		}

		[Test]
		public void LeadingClosingArrayBracket() {
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson<JsonValue>("]1, 2]"); });
		}
	}
	
	[TestFixture]
	public class JsonValue_Reading_Objects {
		[Test]
		public void EmptyObject() {
			var test = JsonMapper.FromJson<JsonValue>("{}");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Object));
				Assert.That(test, Is.Empty);
			});
		}

		[Test]
		public void SimpleObject() {
			var test = JsonMapper.FromJson<JsonValue>("{\"test\": 1}");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Object));
				Assert.That(test, Has.Count.EqualTo(1));
				Assert.That(test.ContainsKey("test"), Is.True);
				Assert.That(test["test"].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)test["test"], Is.EqualTo(1));
			});
		}

		[Test]
		public void MultiObject() {
			var test = JsonMapper.FromJson<JsonValue>("{\"test\": 1, \"test2\": 2}");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Object));
				Assert.That(test, Has.Count.EqualTo(2));

				Assert.That(test.ContainsKey("test"), Is.True);
				Assert.That(test["test"].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)test["test"], Is.EqualTo(1));

				Assert.That(test.ContainsKey("test2"), Is.True);
				Assert.That(test["test2"].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)test["test2"], Is.EqualTo(2));
			});
		}

		[Test]
		public void NestedObject() {
			var test = JsonMapper.FromJson<JsonValue>("{\"test\": {\"test2\": 1}}");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Object));
				Assert.That(test, Has.Count.EqualTo(1));
				Assert.That(test.ContainsKey("test"), Is.True);
				Assert.That(test["test"].Type, Is.EqualTo(JsonValueType.Object));
				Assert.That(test["test"].ContainsKey("test2"), Is.True);
				Assert.That(test["test"]["test2"].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)test["test"]["test2"], Is.EqualTo(1));
			});
		}

        [Test]
		public void ObjectMappingToArray() {
			var test = JsonMapper.FromJson<JsonValue>("{\"test\": [1, 2, 3]}");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Object));
				Assert.That(test, Has.Count.EqualTo(1));
				Assert.That(test.ContainsKey("test"), Is.True);
				Assert.That(test["test"].Type, Is.EqualTo(JsonValueType.Array));
				Assert.That(test["test"], Has.Count.EqualTo(3));

				Assert.That(test["test"][0].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int) test["test"][0], Is.EqualTo(1));
				Assert.That(test["test"][1].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int) test["test"][1], Is.EqualTo(2));
				Assert.That(test["test"][2].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int) test["test"][2], Is.EqualTo(3));
			});
		}

		[Test]
		public void LeadingObjectClosingBrace() {
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson<JsonValue>("}\"test\": 1}"); });
		}

		[Test]
		public void ObjectWithDuplicateKeysPrefersTheLastOccurenceOfTheKey() {
			var test = JsonMapper.FromJson<JsonValue>("{\"a\":\"b\",\"a\":\"c\"}");
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Object));
				Assert.That(test, Has.Count.EqualTo(1));
				Assert.That(test.ContainsKey("a"), Is.True);
				Assert.That(test["a"].Type, Is.EqualTo(JsonValueType.String));
				Assert.That((string)test["a"], Is.EqualTo("c")); // c since it's the last value for the key "a" appearing in the json
			});
		}
	}
}
