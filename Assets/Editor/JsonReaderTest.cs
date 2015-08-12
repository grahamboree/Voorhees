using NUnit.Framework;

[TestFixture]
public class JsonReaderTest {
	[Test]
	public void SimpleRead() {
		// int
		JsonValue test = JsonReader.Read("1");
		Assert.That(test.Type, Is.EqualTo(JsonType.Int));
		Assert.That((int)test, Is.EqualTo(1));

		// float
		test = JsonReader.Read("1.5");
		Assert.That(test.Type, Is.EqualTo(JsonType.Float));
		Assert.That((float)test, Is.EqualTo(1.5f));

		// string
		test = JsonReader.Read("\"test\"");
		Assert.That(test.Type, Is.EqualTo(JsonType.String));
		Assert.That((string)test, Is.EqualTo("test"));

		test = JsonReader.Read("\"\\\\\"");
		Assert.That(test.Type, Is.EqualTo(JsonType.String));
		Assert.That((string)test, Is.EqualTo("\\\\"));
	}
}
