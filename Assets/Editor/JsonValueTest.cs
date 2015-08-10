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
		Assert.That(test.Type == JsonType.None);

		test = new JsonValue(false);
		Assert.That(test.Type == JsonType.Boolean);

		test = new JsonValue(1.0f);
		Assert.That(test.Type == JsonType.Float);

		test = new JsonValue(1);
		Assert.That(test.Type == JsonType.Int);

		test = new JsonValue("test");
		Assert.That(test.Type == JsonType.String);
	}

	[Test]
	public void ImplicitConversions() {
		JsonValue test = false;
		Assert.That(test.Type == JsonType.Boolean);

		test = 1.0f;
		Assert.That(test.Type == JsonType.Float);

		test = 1;
		Assert.That(test.Type == JsonType.Int);

		test = "test";
		Assert.That(test.Type == JsonType.String);
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
}
