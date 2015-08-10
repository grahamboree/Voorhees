using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

[TestFixture]
class JsonValueTest {
	[Test]
	public void Constructors() {
		JsonValue test = new JsonValue();
		Assert.That(test.Type, Is.EqualTo(JsonType.None));

		test = new JsonValue(false);
		Assert.That(test.Type, Is.EqualTo(JsonType.Boolean));

		test = new JsonValue(1.0f);
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));

		test = new JsonValue(1);
		Assert.That(test.Type, Is.EqualTo(JsonType.Int));

		test = new JsonValue("test");
		Assert.That(test.Type, Is.EqualTo(JsonType.String));
	}

	[Test]
	public void ImplicitConversions() {
		JsonValue test = false;
		Assert.That(test.Type, Is.EqualTo(JsonType.Boolean));

		test = 1.0f;
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));

		test = 1;
		Assert.That(test.Type, Is.EqualTo(JsonType.Int));

		test = "test";
		Assert.That(test.Type, Is.EqualTo(JsonType.String));
	}

	[Test]
	public void ExplicitConversions() {
		JsonValue test = false;
		Assert.That(test.Type, Is.EqualTo(JsonType.Boolean));
		Assert.That((bool)test, Is.False);

		test = 1.0f;
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(1.0f));

		test = 1;
		Assert.That(test.Type, Is.EqualTo(JsonType.Int));
		Assert.That((int)test, Is.EqualTo(1));

		test = "test";
		Assert.That(test.Type, Is.EqualTo(JsonType.String));
		Assert.That((string)test, Is.EqualTo("test"));
	}

	[Test]
	public void ArrayLiteral() {
		JsonValue test = new JsonValue() {
			3,
			2,
			1,
			"blastoff!"
		};
		Assert.That(test.Type, Is.EqualTo(JsonType.Array));
		Assert.That(test[0].Type, Is.EqualTo(JsonType.Int));
		Assert.That(test[1].Type, Is.EqualTo(JsonType.Int));
		Assert.That(test[2].Type, Is.EqualTo(JsonType.Int));
		Assert.That(test[3].Type, Is.EqualTo(JsonType.String));
		Assert.That(test[3].Type, Is.EqualTo(JsonType.String));
		Assert.That((int)test[0], Is.EqualTo(3));
		Assert.That((int)test[1], Is.EqualTo(2));
		Assert.That((int)test[2], Is.EqualTo(1));
		Assert.That((string)test[3], Is.EqualTo("blastoff!"));
	}

	[Test]
	public void DictionaryLiteral() {
		JsonValue test = new JsonValue() {
			{"three", 3},
			{"two", 2},
			{"one", 1},
			{"blast", "off!"}
		};
		Assert.That(test.Type, Is.EqualTo(JsonType.Object));

		Assert.That(test.ContainsKey("three"), Is.True);
		Assert.That(test.ContainsKey("two"), Is.True);
		Assert.That(test.ContainsKey("one"), Is.True);
		Assert.That(test.ContainsKey("blast"), Is.True);

		Assert.That(test["three"].Type, Is.EqualTo(JsonType.Int));
		Assert.That(test["two"].Type, Is.EqualTo(JsonType.Int));
		Assert.That(test["one"].Type, Is.EqualTo(JsonType.Int));
		Assert.That(test["blast"].Type, Is.EqualTo(JsonType.String));

		Assert.That((int)test["three"], Is.EqualTo(3));
		Assert.That((int)test["two"], Is.EqualTo(2));
		Assert.That((int)test["one"], Is.EqualTo(1));
		Assert.That((string)test["blast"], Is.EqualTo("off!"));
	}

	[Test]
	public void TypeProperties() {
		// object
		JsonValue test = new JsonValue() {
			{"one", 1},
			{"two", 2},
			{"three", 3}
		};
		Assert.That(test.IsObject, Is.True);
		Assert.That(test.IsArray, Is.False);
		Assert.That(test.IsString, Is.False);
		Assert.That(test.IsBoolean, Is.False);
		Assert.That(test.IsInt, Is.False);
		Assert.That(test.IsFloat, Is.False);

		// array
		test = new JsonValue() { 1, 2, 3 };
		Assert.That(test.IsObject, Is.False);
		Assert.That(test.IsArray, Is.True);
		Assert.That(test.IsString, Is.False);
		Assert.That(test.IsBoolean, Is.False);
		Assert.That(test.IsInt, Is.False);
		Assert.That(test.IsFloat, Is.False);
		
		// string
		test = "test";
		Assert.That(test.IsObject, Is.False);
		Assert.That(test.IsArray, Is.False);
		Assert.That(test.IsString, Is.True);
		Assert.That(test.IsBoolean, Is.False);
		Assert.That(test.IsInt, Is.False);
		Assert.That(test.IsFloat, Is.False);

		// boolean
		test = false;
		Assert.That(test.IsObject, Is.False);
		Assert.That(test.IsArray, Is.False);
		Assert.That(test.IsString, Is.False);
		Assert.That(test.IsBoolean, Is.True);
		Assert.That(test.IsInt, Is.False);
		Assert.That(test.IsFloat, Is.False);

		// int
		test = 1;
		Assert.That(test.IsObject, Is.False);
		Assert.That(test.IsArray, Is.False);
		Assert.That(test.IsString, Is.False);
		Assert.That(test.IsBoolean, Is.False);
		Assert.That(test.IsInt, Is.True);
		Assert.That(test.IsFloat, Is.False);

		// float
		test = 1f;
		Assert.That(test.IsObject, Is.False);
		Assert.That(test.IsArray, Is.False);
		Assert.That(test.IsString, Is.False);
		Assert.That(test.IsBoolean, Is.False);
		Assert.That(test.IsInt, Is.False);
		Assert.That(test.IsFloat, Is.True);
	}

	[Test]
	public void ArrayOperations() {
		JsonValue test = new JsonValue() { 1, 2, 3 };

		Assert.That(test.Count, Is.EqualTo(3));
		Assert.That(test.Contains(1), Is.True);
		Assert.That(test.Contains(4), Is.False);

		JsonValue[] values = new JsonValue[3];
		test.CopyTo(values, 0);
		for (int i = 0; i < 3; i++) {
			Assert.That((int)values[i], Is.EqualTo((int)test[i]));
		}
		
		test.Remove(2);
		Assert.That(test.Contains(1), Is.True);
		Assert.That(test.Contains(2), Is.False);
		Assert.That(test.Contains(3), Is.True);

		Assert.That(test.IsReadOnly, Is.False);

		test.Clear();
		Assert.That(test.Count, Is.EqualTo(0));

		test.Add(5);
		Assert.That(test.Count, Is.EqualTo(1));
		Assert.That((int)test[0], Is.EqualTo(5));
	}
}
