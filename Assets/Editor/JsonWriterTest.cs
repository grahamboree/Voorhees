using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

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
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("[1, 2, 3, 4]"));
	}

	[Test]
	public void WriteNestedArray() {
		JsonValue test = new JsonValue { 1, new JsonValue { 2, 3 }, 4, 5 };
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("[1, [2, 3], 4, 5]"));
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
		Assert.That(JsonWriter.ToJson(test), Is.EqualTo("{\"test\": 1}"));
	}
}
