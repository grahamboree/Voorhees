using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Voorhees.Tests {
	[TestFixture]
	public class JsonWriterTest {
		static string ToJson(JsonValue json) {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var jsonWriter = new JsonWriter(sw, false);
				jsonWriter.Write(json);
			}
			return sb.ToString();
		}
		
		[Test]
		public void WriteUnspecifiedThrows() {
			Assert.Throws<InvalidOperationException>(() => {
				ToJson(new JsonValue());
			});
		}
		
		[Test]
		public void WriteNull() {
			Assert.That(ToJson(new JsonValue(null)), Is.EqualTo("null"));
			Assert.That(ToJson(null), Is.EqualTo("null"));
		}

		[Test]
		public void WriteTrue() {
			Assert.That(ToJson(true), Is.EqualTo("true"));
		}

		[Test]
		public void WriteFalse() {
			Assert.That(ToJson(false), Is.EqualTo("false"));
		}

		[Test]
		public void WriteFloat() {
			Assert.That(ToJson(1.5f), Is.EqualTo("1.5"));
			Assert.That(ToJson(1f), Is.EqualTo("1"));
		}

		[Test]
		public void WriteInt() {
			Assert.That(ToJson(1), Is.EqualTo("1"));
		}

		[Test]
		public void WriteString() {
			Assert.That(ToJson("test"), Is.EqualTo("\"test\""));
		}

		[Test]
		public void WriteStringWithEscapeCharacters() {
			Assert.That(ToJson("te\"st"), Is.EqualTo("\"te\\\"st\""));
			Assert.That(ToJson("te\\st"), Is.EqualTo("\"te\\\\st\""));
			Assert.That(ToJson("te/st"), Is.EqualTo("\"te\\/st\""));
			Assert.That(ToJson("te\bst"), Is.EqualTo("\"te\\bst\""));
			Assert.That(ToJson("te\fst"), Is.EqualTo("\"te\\fst\""));
			Assert.That(ToJson("te\nst"), Is.EqualTo("\"te\\nst\""));
			Assert.That(ToJson("te\rst"), Is.EqualTo("\"te\\rst\""));
			Assert.That(ToJson("te\tst"), Is.EqualTo("\"te\\tst\""));
		}

		[Test]
		public void WriteEmptyArray() {
			var test = new JsonValue(JsonType.Array);
			Assert.That(ToJson(test), Is.EqualTo("[]"));
		}

		[Test]
		public void WriteSimpleArray() {
			var test = new JsonValue {1, 2, 3, 4};
			Assert.That(ToJson(test), Is.EqualTo("[1,2,3,4]"));
		}

		[Test]
		public void WriteNestedArray() {
			var test = new JsonValue {1, new JsonValue {2, 3}, 4, 5};
			Assert.That(ToJson(test), Is.EqualTo("[1,[2,3],4,5]"));
		}

		[Test]
		public void WriteEmptyObject() {
			var test = new JsonValue(JsonType.Object);
			Assert.That(ToJson(test), Is.EqualTo("{}"));
		}

		[Test]
		public void WriteSimpleObject() {
			var test = new JsonValue {
				{"test", 1}
			};
			Assert.That(ToJson(test), Is.EqualTo("{\"test\":1}"));
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
			Assert.That(ToJson(test), Is.EqualTo("{\"test\":{\"test2\":2}}"));
		}

		[Test]
		public void WriteControlCharacters() {
			for (int i = 0; i < 32; ++i) {
				string str = ((char)i).ToString();

				string expected = $"\"\\u{i:X4}\"";
				switch (i) {
					case 8:  expected = "\"\\b\""; break;
					case 9:  expected = "\"\\t\""; break;
					case 10: expected = "\"\\n\""; break;
					case 12: expected = "\"\\f\""; break;
					case 13: expected = "\"\\r\""; break;
				}
				
				Assert.That(ToJson(new JsonValue(str)), Is.EqualTo(expected));
			}
		}
	}

	[TestFixture]
	public class JsonWriterPrettyPrint {
		static string ToJson(JsonValue json) {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var jsonWriter = new JsonWriter(sw, true);
				jsonWriter.Write(json);
			}
			return sb.ToString();
		}
		
		[Test]
		public void WriteNullWritesNull() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.WriteNull();
			}
			Assert.That(sb.ToString(), Is.EqualTo("null"));
		}

		[Test]
		public void WriteBoolWritesJsonTrueOrFalse() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write(true);
			}
			Assert.That(sb.ToString(), Is.EqualTo("true"));
		}

		[Test]
		public void WriteByte() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write((byte)1);
			}
			Assert.That(sb.ToString(), Is.EqualTo("1"));
		}

		[Test]
		public void WriteSByte() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write((sbyte)1);
			}
			Assert.That(sb.ToString(), Is.EqualTo("1"));
		}

		[Test]
		public void WriteShort() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write((short)1);
			}
			Assert.That(sb.ToString(), Is.EqualTo("1"));
		}

		[Test]
		public void WriteUShort() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write((ushort)1);
			}
			Assert.That(sb.ToString(), Is.EqualTo("1"));
		}

		[Test]
		public void WriteInt() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write(1);
			}
			Assert.That(sb.ToString(), Is.EqualTo("1"));
		}

		[Test]
		public void WriteUInt() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write(1u);
			}
			Assert.That(sb.ToString(), Is.EqualTo("1"));
		}

		[Test]
		public void WriteLong() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write(1L);
			}
			Assert.That(sb.ToString(), Is.EqualTo("1"));
		}

		[Test]
		public void WriteULong() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write(1UL);
			}
			Assert.That(sb.ToString(), Is.EqualTo("1"));
		}

		[Test]
		public void WriteFloat() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write(1.5f);
			}
			Assert.That(sb.ToString(), Is.EqualTo("1.5"));
		}
		
		[Test]
		public void WriteDouble() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write(1.5);
			}
			Assert.That(sb.ToString(), Is.EqualTo("1.5"));
		}

		[Test]
		public void WriteDecimal() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write(1.5m);
			}
			Assert.That(sb.ToString(), Is.EqualTo("1.5"));
		}
		
		[Test]
		public void WriteChar() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write('c');
			}
			Assert.That(sb.ToString(), Is.EqualTo("\"c\""));
		}
		
		[Test]
		public void WriteString() {
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb)) {
				var writer = new JsonWriter(sw, true);
				writer.Write("test");
			}
			Assert.That(sb.ToString(), Is.EqualTo("\"test\""));
		}

		[Test]
		public void WriteArrayWritesPrettyPrintedArray() {
			var test = new JsonValue { 1, 2, 3, 4 };
			Assert.That(ToJson(test), Is.EqualTo("[\n\t1,\n\t2,\n\t3,\n\t4\n]"));
		}

		[Test]
		public void WriteNestedArrayWritesPrettyPrintedArrays() {
			var test = new JsonValue { 1, new JsonValue { 2, 3 }, 4 };
			Assert.That(ToJson(test), Is.EqualTo("[\n\t1,\n\t[\n\t\t2,\n\t\t3\n\t],\n\t4\n]"));
		}

		[Test]
		public void WriteObjectWritesPrettyPrintedObject() {
			var test = new JsonValue {
				{ "test", 1 },
				{ "test2", 2 }
			};
			Assert.That(ToJson(test), Is.EqualTo("{\n\t\"test\": 1,\n\t\"test2\": 2\n}"));
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
			const string prettyJson = "{\n\t\"test\": 1,\n\t\"test2\": {\n\t\t\"test3\": 3,\n\t\t\"test4\": 4\n\t}\n}";
			Assert.That(ToJson(test), Is.EqualTo(prettyJson));
		}

		[Test]
		public void PrettyPrintDeeplyNestedArray() {
			// Tests that exceeding the tab cache will correctly generate the right number of tabs.
			var test = new JsonValue(JsonType.Array);

			var current = test;
			for (int i = 0; i < 21; ++i) {
				var newVal = new JsonValue(JsonType.Array);
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
			Assert.That(ToJson(test), Is.EqualTo(expected));
		}
	}
}
