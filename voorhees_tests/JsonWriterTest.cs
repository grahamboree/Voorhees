using NUnit.Framework;
using Voorhees;

[TestFixture]
public class JsonWriterTest {
	[Test]
	public void WriteNull() {
		JsonValue test = new JsonValue();
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("null"));
	}

	[Test]
	public void WriteTrue() {
		JsonValue test = true;
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("true"));
	}

	[Test]
	public void WriteFalse() {
		JsonValue test = false;
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("false"));
	}

	[Test]
	public void WriteFloat() {
		JsonValue test = 1.5f;
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("1.5"));

		test = 1f;
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("1"));
	}

	[Test]
	public void WriteInt() {
		JsonValue test = 1;
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("1"));
	}

	[Test]
	public void WriteString() {
		JsonValue test = "test";
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("\"test\""));
	}

	[Test]
	public void WriteEmptyArray() {
		JsonValue test = new JsonValue();
		test.Type = JsonType.Array;
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("[]"));
	}

	[Test]
	public void WriteSimpleArray() {
		JsonValue test = new JsonValue { 1, 2, 3, 4 };
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("[1,2,3,4]"));
	}

	[Test]
	public void WriteNestedArray() {
		JsonValue test = new JsonValue { 1, new JsonValue { 2, 3 }, 4, 5 };
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("[1,[2,3],4,5]"));
	}

	[Test]
	public void WriteEmptyObject() {
		JsonValue test = new JsonValue();
		test.Type = JsonType.Object;
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("{}"));
	}

	[Test]
	public void WriteSimpleObject() {
		JsonValue test = new JsonValue {
			{ "test", 1 }
		};
		test.Type = JsonType.Object;
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("{\"test\":1}"));
	}

	[Test]
	public void WriteNestedObject() {
		JsonValue test = new JsonValue {
			{
				"test",
				new JsonValue{
					{"test2", 2}
				}
			}
		};
		test.Type = JsonType.Object;
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("{\"test\":{\"test2\":2}}"));
	}

	[Test]
	public void PrettyPrintSimpleArray() {
		JsonValue test = new JsonValue { 1, 2, 3, 4 };
		Assert.That(JsonWriter.ToJson(test, true), Is.EqualTo("[\n\t1,\n\t2,\n\t3,\n\t4\n]"));
	}

	[Test]
	public void PrettyPrintNestedArray() {
		JsonValue test = new JsonValue { 1, new JsonValue { 2, 3 }, 4 };
		Assert.That(JsonWriter.ToJson(test, true), Is.EqualTo("[\n\t1,\n\t[\n\t\t2,\n\t\t3\n\t],\n\t4\n]"));
	}

	[Test]
	public void PrettyPrintSimpleObject() {
		JsonValue test = new JsonValue {
			{ "test", 1 },
			{ "test2", 2 }
		};
		Assert.That(JsonWriter.ToJson(test, true), Is.EqualTo("{\n\t\"test\": 1,\n\t\"test2\": 2\n}"));
	}

	[Test]
	public void PrettyPrintNestedObject() {
		JsonValue test = new JsonValue {
			{ "test", 1 },
			{ "test2", new JsonValue {
					{ "test3", 3 },
					{ "test4", 4 }
				}
			}
		};
		Assert.That(JsonWriter.ToJson(test, true), Is.EqualTo("{\n\t\"test\": 1,\n\t\"test2\": {\n\t\t\"test3\": 3,\n\t\t\"test4\": 4\n\t}\n}"));
	}
}
