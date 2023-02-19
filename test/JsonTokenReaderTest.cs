using System;
using System.IO;
using NUnit.Framework;

namespace Voorhees.Tests {
	[TestFixture]
	public class JsonTokenReader_SkipToken {
		[Test]
		public void ArrayStart() {
			var doc = new Internal.DocumentCursor(new StringReader("[1,2,3]"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.ArrayStart);
			Assert.That(doc.Index, Is.EqualTo(1));
		}
		
		[Test]
		public void ArrayEnd() {
			var doc = new Internal.DocumentCursor(new StringReader("][1,2,3]"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.ArrayEnd);
			Assert.That(doc.Index, Is.EqualTo(1));
		}
		
		[Test]
		public void ObjectStart() {
			var doc = new Internal.DocumentCursor(new StringReader("{\"test\": 123}"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.ObjectStart);
			Assert.That(doc.Index, Is.EqualTo(1));
		}
		
		[Test]
		public void KeyValueSeparator() {
			var doc = new Internal.DocumentCursor(new StringReader(":123}"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.KeyValueSeparator);
			Assert.That(doc.Index, Is.EqualTo(1));
		}
		
		[Test]
		public void ObjectEnd() {
			var doc = new Internal.DocumentCursor(new StringReader("}{\"test\": 123}"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.ObjectEnd);
			Assert.That(doc.Index, Is.EqualTo(1));
		}
		
		[Test]
		public void Separator() {
			var doc = new Internal.DocumentCursor(new StringReader(",{\"test\": 123}"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.Separator);
			Assert.That(doc.Index, Is.EqualTo(1));
		}

		[Test]
		public void True() {
			var doc = new Internal.DocumentCursor(new StringReader("true, true"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.True);
			Assert.That(doc.Index, Is.EqualTo(4));
		}
		
		[Test]
		public void False() {
			var doc = new Internal.DocumentCursor(new StringReader("false, false"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.False);
			Assert.That(doc.Index, Is.EqualTo(5));
		}
		
		[Test]
		public void Null() {
			var doc = new Internal.DocumentCursor(new StringReader("null, null"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.Null);
			Assert.That(doc.Index, Is.EqualTo(4));
		}

		
		[Test]
		public void String() {
			var doc = new Internal.DocumentCursor(new StringReader("\"test\", 123"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.String);
			Assert.That(doc.Index, Is.EqualTo(6));
		}
		
		[Test]
		public void Number() {
			var doc = new Internal.DocumentCursor(new StringReader("-123.456e7, 123"));
			var tokenReader = new JsonTokenReader(doc);
            tokenReader.SkipToken(JsonToken.Number);
			Assert.That(doc.Index, Is.EqualTo(10));
		}

		[Test]
		public void SkipsTrailingWhitespace() {
			var doc = new Internal.DocumentCursor(new StringReader("true    , false"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.True);
			Assert.That(doc.Index, Is.EqualTo(8));
		}
		
		[Test]
		public void SkippingTheWrongTokenThrows() {
			using var json = new StringReader("true, false");
			var tokenReader = new JsonTokenReader(json);
			Assert.Throws<InvalidOperationException>(() => tokenReader.SkipToken(JsonToken.ArrayStart));
		}
		
		[Test]
		public void SkippingEOFThrows() {
			using var json = new StringReader("");
			var tokenReader = new JsonTokenReader(json);
			Assert.Throws<InvalidOperationException>(() => tokenReader.SkipToken(JsonToken.EOF));
		}

		[Test]
		public void HittingEOFWhenSkippingStringThrows() {
			Assert.Throws<InvalidJsonException>(() => {
				using var json = new StringReader("\"test");
				var tokenReader = new JsonTokenReader(json);
				tokenReader.SkipToken(JsonToken.String);
			});
		}

		[Test]
		public void SkippingStringWithEscapedCharacter() {
			var doc = new Internal.DocumentCursor(new StringReader("\"test\\\"\", false"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.String);
			Assert.That(doc.Index, Is.EqualTo(8));
		}

		[Test]
		public void SkippingStringContainingEscapedUnicodeCharacter() {
			var doc = new Internal.DocumentCursor(new StringReader("\"\\u597D\", false"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.String);
			Assert.That(doc.Index, Is.EqualTo(8));
		}

		[Test]
		public void SkippingStringsDisallowsControlCharacters() {
			for (int i = 0; i < 0x20; i++) {
				string controlChar = char.ConvertFromUtf32(i);
				Assert.Throws<InvalidJsonException>(() => {
					using var json = new StringReader($"\"{controlChar}\"");
					new JsonTokenReader(json).SkipToken(JsonToken.String);
				});
			}

			Assert.Throws<InvalidJsonException>(() => {
				using var json = new StringReader($"\"{char.ConvertFromUtf32(0x7F)}\"");
				new JsonTokenReader(json).SkipToken(JsonToken.String);
			});

			for (int i = 0x80; i <= 0x9F; i++) {
				string controlChar = char.ConvertFromUtf32(i);
				Assert.Throws<InvalidJsonException>(() => {
					using var json = new StringReader($"\"{controlChar}\"");
					new JsonTokenReader(json).SkipToken(JsonToken.String);
				});
			}
		}
    }

	[TestFixture]
	public class JsonTokenReader_ConsumeNumber {
		[Test]
		public void NotANumberNext() {
			using var json = new StringReader("true");
			var tokenReader = new JsonTokenReader(json);
			Assert.Throws<InvalidOperationException>(() => tokenReader.ConsumeNumber());
		}
		
		[Test]
		public void LeadingZeroMustHaveDecimalOrExponent() {
			using var json = new StringReader("0123");
			var tokenReader = new JsonTokenReader(json);
			Assert.Throws<InvalidJsonException>(() => tokenReader.ConsumeNumber());
		}

		[Test]
		public void Zero() {
			TestString("0");
		}

		[Test]
		public void NegativeZero() {
			TestString("-0");
		}
		
		[Test]
		public void PositiveInteger() {
			TestString("123");
		}
		
		[Test]
		public void NegativeInteger() {
			TestString("-123");
		}
		
		[Test]
		public void PositiveDecimal() {
			TestString("1.234");
		}

		[Test]
		public void NegativeDecimal() {
			TestString("-1.234");
		}

		[Test]
		public void PositiveLeadingZeroDecimal() {
			TestString("0.123");
		}
		
		[Test]
		public void NegativeLeadingZeroDecimal() {
			TestString("-0.123");
		}
		
		[Test]
		public void PositiveExponent() {
			TestString("0e3");
		}
		
		[Test]
		public void ExplicitlyPositiveExponent() {
			TestString("0e+3");
		}
		
		[Test]
		public void NegativeExponent() {
			TestString("0e-3");
		}
		
		[Test]
		public void NegativeNumberWithNegativeExponent() {
			TestString("-0e-3");
		}
		
		[Test]
		public void FractionalNumberWithExponentWithLeadingZeros() {
			TestString("123.456E+007");
		}

		static void TestString(string json) {
			using var jsonStringReader = new StringReader(json);
			string tokenString = new(new JsonTokenReader(jsonStringReader).ConsumeNumber());
			Assert.That(tokenString, Is.EqualTo(json));
		}
	}

	[TestFixture]
	public class JsonTokenReader_ConsumeString {
		[Test]
		public void BasicString() {
			using var json = new StringReader("\"test\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("test"));
		}

		[Test]
		public void EscapedQuotes() {
			using var json = new StringReader("\"\\\"\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\""));
		}
		
		[Test]
		public void EscapedBackslash() {
			using var json = new StringReader("\"\\\\\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\\"));
		}
		
		[Test]
		public void EscapedForwardSlash() {
			using var json = new StringReader("\"\\/\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("/"));
		}
		
		[Test]
		public void EscapedBackspace() {
			using var json = new StringReader("\"\\b\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\b"));
		}
		
		[Test]
		public void EscapedFormFeed() {
			using var json = new StringReader("\"\\f\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\f"));
		}
		
		[Test]
		public void EscapedLineFeed() {
			using var json = new StringReader("\"\\n\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\n"));
		}
		
		[Test]
		public void EscapedCarriageReturn() {
			using var json = new StringReader("\"\\r\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\r"));
		}
		
		[Test]
		public void EscapedHorizontalTab() {
			using var json = new StringReader("\"\\t\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\t"));
		}
		
		[Test]
		public void EscapedUnicode() {
			using var json = new StringReader("\"\\u597D\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("好"));
		}

		[Test]
		public void InvalidEscapeCode() {
			using var json = new StringReader("\"\\g\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.Throws<InvalidJsonException>(() => tokenReader.ConsumeString());
		}

		[Test]
		public void MixedRegularAndEscapedChars() {
			using var json = new StringReader("\"¿ni\\u597Dma?\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("¿ni好ma?"));
		}

		[Test]
		public void DisallowsControlCharacters() {
			for (int i = 0; i < 0x20; i++) {
				string controlChar = char.ConvertFromUtf32(i);
				string json1 = $"\"{controlChar}\"";
				Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson<JsonValue>(json1); });
			}

			string json2 = $"\"{char.ConvertFromUtf32(0x7F)}\"";
			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson<JsonValue>(json2); });

			for (int i = 0x80; i <= 0x9F; i++) {
				string controlChar = char.ConvertFromUtf32(i);
				string json3 = $"\"{controlChar}\"";
				Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson<JsonValue>(json3); });
			}
		}

		[Test]
		public void AdvancingToRandomCharactersThrows() {
			Assert.Throws<InvalidJsonException>(() => {
				using var json = new StringReader("fail");
				var _ = new JsonTokenReader(json);
			});
		}

		[Test]
		public void ConsumingANonStringTypeAsStringThrows() {
			Assert.Throws<InvalidOperationException>(() => {
				using var json = new StringReader("3");
				var tokenReader = new JsonTokenReader(json);
				string _ = tokenReader.ConsumeString();
			});
		}
		
		[Test]
		public void ParsesUnicodeSurrogatePairsCorrectly() {
			using var json = new StringReader("\"\\ud83d\\ude80\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("🚀"));
		}
		
		[Test]
		public void ParsesEmojiCorrectly() {
			using var json = new StringReader("\"🚀\"");
			var tokenReader = new JsonTokenReader(json);
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("🚀"));
		}

		[Test]
		public void DoesNotReadPastEndOfString() {
			var doc = new Internal.DocumentCursor(new StringReader("{\"test\": 3}"));
			var tokenReader = new JsonTokenReader(doc);
			tokenReader.SkipToken(JsonToken.ObjectStart);
			string str = tokenReader.ConsumeString();
			Assert.Multiple(() => {
				Assert.That(str, Is.EqualTo("test"));
				Assert.That(doc.Index, Is.EqualTo(7));
			});
		}

		[Test]
		public void DoubleQuotedString()  {
			var doc = new Internal.DocumentCursor(new StringReader("\"\\\"test\\\"\","));
            var tokenReader = new JsonTokenReader(doc);
            Assert.Multiple(() => {
                Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\"test\""));
                Assert.That(tokenReader.NextToken, Is.EqualTo(JsonToken.Separator));
            });
        }
    }
}
