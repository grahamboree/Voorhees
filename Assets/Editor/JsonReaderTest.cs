using NUnit.Framework;

[TestFixture]
public class JsonReaderTest {
	[Test]
	public void SimpleRead() {
		JsonValue one = JsonReader.Read("1");
		Assert.That(one.Type, Is.EqualTo(JsonType.Int));
	}
}
