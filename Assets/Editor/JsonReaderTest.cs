using NUnit.Framework;

[TestFixture]
public class JsonReaderTest {
	[Test]
	public void Int() {
		JsonValue test;
		test = JsonReader.Read("1");
		Assert.That(test.Type, Is.EqualTo(JsonType.Int));
		Assert.That((int)test, Is.EqualTo(1));
	}

	[Test]
	public void NegativeInt() {
		JsonValue test;
		test = JsonReader.Read("-1");
		Assert.That(test.Type, Is.EqualTo(JsonType.Int));
		Assert.That((int)test, Is.EqualTo(-1));
	}

	[Test]
	public void Float() {
		JsonValue test;
		test = JsonReader.Read("1.5");
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(1.5f));
	}

	[Test]
	public void NegativeFloat() {
		JsonValue test;
		test = JsonReader.Read("-1.5");
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(-1.5f));
	}

	[Test]
	public void FloatPositiveE() {
		JsonValue test;
		test = JsonReader.Read("1.5e+1");
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(15f));
	}

	[Test]
	public void FloatE() {
		JsonValue test;
		test = JsonReader.Read("1.5e2");
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(1.5e2f));
	}

	[Test]
	public void FloatNegativeE() {
		JsonValue test;
		test = JsonReader.Read("1.5e-2");
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(1.5e-2f));
	}

	[Test]
	public void NegativeFloatPositiveE() {
		JsonValue test;
		test = JsonReader.Read("-1.5e+1");
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(-15f));
	}

	[Test]
	public void NegativeFloatE() {
		JsonValue test;
		test = JsonReader.Read("-1.5e2");
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(-1.5e2f));
	}

	[Test]
	public void NegativeFloatNegativeE() {
		JsonValue test;
		test = JsonReader.Read("-1.5e-2");
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(-1.5e-2f));
	}

	[Test]
	public void String() {
		JsonValue test;
		test = JsonReader.Read("\"test\"");
		Assert.That(test.Type, Is.EqualTo(JsonType.String));
		Assert.That((string)test, Is.EqualTo("test"));
	}

	[Test]
	public void StringWithEscapedQuotes() {
		JsonValue test;
		test = JsonReader.Read("\"\\\\\"");
		Assert.That(test.Type, Is.EqualTo(JsonType.String));
		Assert.That((string)test, Is.EqualTo("\\\\"));
	}
	
	[Test]
	public void True() {
		JsonValue test;
		test = JsonReader.Read("true");
		Assert.That(test.Type, Is.EqualTo(JsonType.Boolean));
		Assert.That((bool)test, Is.True);
	}

	[Test]
	public void False() {
		JsonValue test;
		test = JsonReader.Read("false");
		Assert.That(test.Type, Is.EqualTo(JsonType.Boolean));
		Assert.That((bool)test, Is.False);
	}

	[Test]
	public void Null() {
		JsonValue test;
		test = JsonReader.Read("null");
		Assert.That(test.Type, Is.EqualTo(JsonType.Null));
		Assert.That(test.IsNull);
	}

	[Test]
	public void EmptyArray() {
		JsonValue test;
		test = JsonReader.Read("[]");
		Assert.That(test.Type, Is.EqualTo(JsonType.Array));
		Assert.That(test.Count, Is.EqualTo(0));
	}

	[Test]
	public void SimpleArray() {
		JsonValue test;
		test = JsonReader.Read("[1]");
		Assert.That(test.Type, Is.EqualTo(JsonType.Array));
		Assert.That(test.Count, Is.EqualTo(1));
		Assert.That(test[0].Type, Is.EqualTo(JsonType.Int));
		Assert.That((int)test[0], Is.EqualTo(1));
	}

	[Test]
	public void MultivalueArray() {
		JsonValue test;
		test = JsonReader.Read("[1, 2, 3]");
		Assert.That(test.Type, Is.EqualTo(JsonType.Array));
		Assert.That(test.Count, Is.EqualTo(3));
		for (int i = 0; i < 3; ++i) {
			Assert.That(test[i].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int)test[i], Is.EqualTo(i + 1));
		}
	}

	[Test]
	public void NestedArray() {
		JsonValue test;
		test = JsonReader.Read("[[1, 2, 3]]");
		Assert.That(test.Type, Is.EqualTo(JsonType.Array));
		Assert.That(test.Count, Is.EqualTo(1));
		test = test[0];

		Assert.That(test.Type, Is.EqualTo(JsonType.Array));
		Assert.That(test.Count, Is.EqualTo(3));
		for (int i = 0; i < 3; ++i) {
			Assert.That(test[i].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int)test[i], Is.EqualTo(i + 1));
		}
	}

	[Test]
	public void MultipleNestedArrays() {
		JsonValue outer;
		outer = JsonReader.Read("[[1, 2, 3], [4, [5]]]");
		Assert.That(outer.Type, Is.EqualTo(JsonType.Array));
		Assert.That(outer.Count, Is.EqualTo(2));
		JsonValue test = outer[0];
		
		Assert.That(test.Type, Is.EqualTo(JsonType.Array));
		Assert.That(test.Count, Is.EqualTo(3));
		for (int i = 0; i < 3; ++i) {
			Assert.That(test[i].Type, Is.EqualTo(JsonType.Int));
			Assert.That((int)test[i], Is.EqualTo(i + 1));
		}

		test = outer[1];
		Assert.That(test.Type, Is.EqualTo(JsonType.Array));
		Assert.That(test.Count, Is.EqualTo(2));
		Assert.That(test[0].Type, Is.EqualTo(JsonType.Int));
		Assert.That((int)test[0], Is.EqualTo(4));

		test = test[1];
		Assert.That(test.Type, Is.EqualTo(JsonType.Array));
		Assert.That(test.Count, Is.EqualTo(1));
		Assert.That(test[0].Type, Is.EqualTo(JsonType.Int));
		Assert.That((int)test[0], Is.EqualTo(5));
	}

	[Test]
	public void ArrayOfAllTypes() {
		JsonValue test;
		test = JsonReader.Read(@"[1, 1.5, ""test"", true, false, null, []]");
		Assert.That(test.Type, Is.EqualTo(JsonType.Array));
		Assert.That(test.Count, Is.EqualTo(7));

		Assert.That(test[0].Type, Is.EqualTo(JsonType.Int));
		Assert.That((int)test[0], Is.EqualTo(1));
		Assert.That(test[1].Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test[1], Is.EqualTo(1.5));
		Assert.That(test[2].Type, Is.EqualTo(JsonType.String));
		Assert.That((string)test[2], Is.EqualTo("test"));
		Assert.That(test[3].Type, Is.EqualTo(JsonType.Boolean));
		Assert.That((bool)test[3], Is.True);
		Assert.That(test[4].Type, Is.EqualTo(JsonType.Boolean));
		Assert.That((bool)test[4], Is.False);
		Assert.That(test[5].Type, Is.EqualTo(JsonType.Null));
		Assert.That(test[6].Type, Is.EqualTo(JsonType.Array));
		Assert.That(test[6].Count, Is.EqualTo(0));
	}

	[Test]
	public void CheckForCommas() {
		Assert.Throws<InvalidJsonException>(() => {
			JsonReader.Read("[1 2]");
		});
	}
}
