using NUnit.Framework;

namespace Voorhees.Tests {
	[TestFixture]
	public class JsonWriterTest {
		[Test]
		public void WriteNull() {
			Assert.That(JsonWriter.ToJson(new JsonValue()), Is.EqualTo("null"));
			Assert.That(JsonWriter.ToJson(null), Is.EqualTo("null"));
		}

		[Test]
		public void WriteTrue() {
			Assert.That(JsonWriter.ToJson(true), Is.EqualTo("true"));
		}

		[Test]
		public void WriteFalse() {
			Assert.That(JsonWriter.ToJson(false), Is.EqualTo("false"));
		}

		[Test]
		public void WriteFloat() {
			Assert.That(JsonWriter.ToJson(1.5f), Is.EqualTo("1.5"));
			Assert.That(JsonWriter.ToJson(1f), Is.EqualTo("1"));
		}

		[Test]
		public void WriteInt() {
			Assert.That(JsonWriter.ToJson(1), Is.EqualTo("1"));
		}

		[Test]
		public void WriteString() {
			Assert.That(JsonWriter.ToJson("test"), Is.EqualTo("\"test\""));
		}

		[Test]
		public void WriteStringWithEscapeCharacters() {
			Assert.That(JsonWriter.ToJson("te\"st"), Is.EqualTo("\"te\\\"st\""));
			Assert.That(JsonWriter.ToJson("te\\st"), Is.EqualTo("\"te\\\\st\""));
			Assert.That(JsonWriter.ToJson("te/st"), Is.EqualTo("\"te\\/st\""));
			Assert.That(JsonWriter.ToJson("te\bst"), Is.EqualTo("\"te\\bst\""));
			Assert.That(JsonWriter.ToJson("te\fst"), Is.EqualTo("\"te\\fst\""));
			Assert.That(JsonWriter.ToJson("te\nst"), Is.EqualTo("\"te\\nst\""));
			Assert.That(JsonWriter.ToJson("te\rst"), Is.EqualTo("\"te\\rst\""));
			Assert.That(JsonWriter.ToJson("te\tst"), Is.EqualTo("\"te\\tst\""));
		}

		[Test]
		public void WriteEmptyArray() {
			var test = new JsonValue {Type = JsonType.Array};
			Assert.That(JsonWriter.ToJson(test), Is.EqualTo("[]"));
		}

		[Test]
		public void WriteSimpleArray() {
			var test = new JsonValue {1, 2, 3, 4};
			Assert.That(JsonWriter.ToJson(test), Is.EqualTo("[1,2,3,4]"));
		}

		[Test]
		public void WriteNestedArray() {
			var test = new JsonValue {1, new JsonValue {2, 3}, 4, 5};
			Assert.That(JsonWriter.ToJson(test), Is.EqualTo("[1,[2,3],4,5]"));
		}

		[Test]
		public void WriteEmptyObject() {
			var test = new JsonValue {Type = JsonType.Object};
			Assert.That(JsonWriter.ToJson(test), Is.EqualTo("{}"));
		}

		[Test]
		public void WriteSimpleObject() {
			var test = new JsonValue {
				{"test", 1}
			};
			test.Type = JsonType.Object;
			Assert.That(JsonWriter.ToJson(test), Is.EqualTo("{\"test\":1}"));
		}

		[Test]
		public void WriteNestedObject() {
			var test = new JsonValue {
				{
					"test",
					new JsonValue {
						{"test2", 2}
					}
				}
			};
			test.Type = JsonType.Object;
			Assert.That(JsonWriter.ToJson(test), Is.EqualTo("{\"test\":{\"test2\":2}}"));
		}
	}

	[TestFixture]
	public class JsonWriterPrettyPrint {
		[SetUp]
		public void SetUp() {
			JsonConfig.CurrentConfig.PrettyPrint = true;
		}

		[TearDown]
		public void TearDown() {
			JsonConfig.CurrentConfig.PrettyPrint = false;
		}

		[Test]
		public void WriteNullWritesNull() {
			var os = new PrettyPrintJsonOutputStream();
			os.WriteNull();
			Assert.That(os.ToString(), Is.EqualTo("null"));
		}

		[Test]
		public void WriteBoolWritesJsonTrueOrFalse() {
			var os = new PrettyPrintJsonOutputStream();
			os.Write(true);
			Assert.That(os.ToString(), Is.EqualTo("true"));
		}

		[Test]
		public void WriteIntegralTypesWritesInts() {
			var os = new PrettyPrintJsonOutputStream();
			os.Write((byte)1);
			Assert.That(os.ToString(), Is.EqualTo("1"));

			os = new PrettyPrintJsonOutputStream();
			os.Write((sbyte)1);
			Assert.That(os.ToString(), Is.EqualTo("1"));

			os = new PrettyPrintJsonOutputStream();
			os.Write((short)1);
			Assert.That(os.ToString(), Is.EqualTo("1"));

			os = new PrettyPrintJsonOutputStream();
			os.Write((ushort)1);
			Assert.That(os.ToString(), Is.EqualTo("1"));

			os = new PrettyPrintJsonOutputStream();
			os.Write(1);
			Assert.That(os.ToString(), Is.EqualTo("1"));

			os = new PrettyPrintJsonOutputStream();
			os.Write((uint)1);
			Assert.That(os.ToString(), Is.EqualTo("1"));

			os = new PrettyPrintJsonOutputStream();
			os.Write((long)1);
			Assert.That(os.ToString(), Is.EqualTo("1"));

			os = new PrettyPrintJsonOutputStream();
			os.Write((ulong)1);
			Assert.That(os.ToString(), Is.EqualTo("1"));
		}

		[Test]
		public void WriteFloatingPointTypesWritesNumber() {
			var os = new PrettyPrintJsonOutputStream();
			os.Write(1.5f);
			Assert.That(os.ToString(), Is.EqualTo("1.5"));

			os = new PrettyPrintJsonOutputStream();
			os.Write(1.5);
			Assert.That(os.ToString(), Is.EqualTo("1.5"));

			os = new PrettyPrintJsonOutputStream();
			os.Write(1.5m);
			Assert.That(os.ToString(), Is.EqualTo("1.5"));
		}

		[Test]
		public void WriteStringTypesWritesString() {
			var os = new PrettyPrintJsonOutputStream();
			os.Write('c');
			Assert.That(os.ToString(), Is.EqualTo("\"c\""));

			os = new PrettyPrintJsonOutputStream();
			os.Write("test");
			Assert.That(os.ToString(), Is.EqualTo("\"test\""));
		}

		[Test]
		public void WriteArrayWritesPrettyPrintedArray() {
			var test = new JsonValue { 1, 2, 3, 4 };
			Assert.That(JsonWriter.ToJson(test), Is.EqualTo("[\n\t1,\n\t2,\n\t3,\n\t4\n]"));
		}

		[Test]
		public void WriteNestedArrayWritesPrettyPrintedArrays() {
			var test = new JsonValue { 1, new JsonValue { 2, 3 }, 4 };
			Assert.That(JsonWriter.ToJson(test), Is.EqualTo("[\n\t1,\n\t[\n\t\t2,\n\t\t3\n\t],\n\t4\n]"));
		}

		[Test]
		public void WriteObjectWritesPrettyPrintedObject() {
			var test = new JsonValue {
				{ "test", 1 },
				{ "test2", 2 }
			};
			Assert.That(JsonWriter.ToJson(test), Is.EqualTo("{\n\t\"test\": 1,\n\t\"test2\": 2\n}"));
		}

		[Test]
		public void WriteNestedObjectWritesPrettyPrintedNestedObjects() {
			var test = new JsonValue {
				{ "test", 1 }, {
					"test2", new JsonValue {
						{ "test3", 3 },
						{ "test4", 4 }
					}
				}
			};
			var prettyJson = "{\n\t\"test\": 1,\n\t\"test2\": {\n\t\t\"test3\": 3,\n\t\t\"test4\": 4\n\t}\n}";
			Assert.That(JsonWriter.ToJson(test), Is.EqualTo(prettyJson));
		}

		[Test]
		public void PrettyPrintDeeplyNestedArray() {
			var test = new JsonValue { Type = JsonType.Array };

			var current = test;
			for (int i = 0; i < 21; ++i) {
				var newVal = new JsonValue { Type = JsonType.Array };
				current.Add(newVal);
				current = newVal;
			}
            current.Add(new JsonValue(42));
			
			var expected
				= "[\n"
				+ "\t[\n"
				+ "\t\t[\n"
				+ "\t\t\t[\n"
				+ "\t\t\t\t[\n"
				+ "\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t[\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t42\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t\t]\n"
				+ "\t\t\t\t\t]\n"
				+ "\t\t\t\t]\n"
				+ "\t\t\t]\n"
				+ "\t\t]\n"
				+ "\t]\n"
				+ "]";
			Assert.That(JsonWriter.ToJson(test), Is.EqualTo(expected));
		}
	}
}
