using NUnit.Framework;

[TestFixture]
public class JsonReaderTest {
	[Test]
	public void SimpleRead() {
		JsonValue test;

		test = JsonReader.Read("1");
		Assert.That(test.Type, Is.EqualTo(JsonType.Int));
		Assert.That((int)test, Is.EqualTo(1));
		
		test = JsonReader.Read("-1");
		Assert.That(test.Type, Is.EqualTo(JsonType.Int));
		Assert.That((int)test, Is.EqualTo(-1));
		
		test = JsonReader.Read("1.5");
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(1.5f));

		test = JsonReader.Read("1.5e+1");
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(15f));

		test = JsonReader.Read("1.5e2");
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(1.5e2f));

		test = JsonReader.Read("1.5e-2");
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(1.5e-2f));

		// string
		test = JsonReader.Read("\"test\"");
		Assert.That(test.Type, Is.EqualTo(JsonType.String));
		Assert.That((string)test, Is.EqualTo("test"));

		test = JsonReader.Read("\"\\\\\"");
		Assert.That(test.Type, Is.EqualTo(JsonType.String));
		Assert.That((string)test, Is.EqualTo("\\\\"));

		// Boolean
		test = JsonReader.Read("true");
		Assert.That(test.Type, Is.EqualTo(JsonType.Boolean));
		Assert.That((bool)test, Is.True);

		test = JsonReader.Read("false");
		Assert.That(test.Type, Is.EqualTo(JsonType.Boolean));
		Assert.That((bool)test, Is.False);

		// Null
		test = JsonReader.Read("null");
		Assert.That(test.Type, Is.EqualTo(JsonType.Null));
		Assert.That(test.IsNull);
	}
}
