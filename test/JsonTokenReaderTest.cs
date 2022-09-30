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
			var tokenReader = new JsonTokenReader("true, false");
			Assert.Throws<InvalidOperationException>(() => tokenReader.SkipToken(JsonToken.ArrayStart));
		}
		
		[Test]
		public void SkippingEOFThrows() {
			var tokenReader = new JsonTokenReader("");
			Assert.Throws<InvalidOperationException>(() => tokenReader.SkipToken(JsonToken.EOF));
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
					new JsonTokenReader($"\"{controlChar}\"").SkipToken(JsonToken.String);
				});
			}

			Assert.Throws<InvalidJsonException>(() => {
				new JsonTokenReader($"\"{char.ConvertFromUtf32(0x7F)}\"").SkipToken(JsonToken.String);
			});

			for (int i = 0x80; i <= 0x9F; i++) {
				string controlChar = char.ConvertFromUtf32(i);
				Assert.Throws<InvalidJsonException>(() => {
					new JsonTokenReader($"\"{controlChar}\"").SkipToken(JsonToken.String);
				});
			}
		}
    }

	[TestFixture]
	public class JsonTokenReader_ConsumeNumber {
		[Test]
		public void NotANumberNext() {
			var tokenReader = new JsonTokenReader("true");
			Assert.Throws<InvalidOperationException>(() => tokenReader.ConsumeNumber());
		}
		
		[Test]
		public void LeadingZeroMustHaveDecimalOrExponent() {
			var tokenReader = new JsonTokenReader("0123");
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
			string tokenString = new(new JsonTokenReader(json).ConsumeNumber());
			Assert.That(tokenString, Is.EqualTo(json));
		}
	}

	[TestFixture]
	public class JsonTokenReader_ConsumeString {
		[Test]
		public void BasicString() {
			var tokenReader = new JsonTokenReader("\"test\"");
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("test"));
		}

		[Test]
		public void EscapedQuotes() {
			var tokenReader = new JsonTokenReader("\"\\\"\"");
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\""));
		}
		
		[Test]
		public void EscapedBackslash() {
			var tokenReader = new JsonTokenReader("\"\\\\\"");
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\\"));
		}
		
		[Test]
		public void EscapedForwardSlash() {
			var tokenReader = new JsonTokenReader("\"\\/\"");
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("/"));
		}
		
		[Test]
		public void EscapedBackspace() {
			var tokenReader = new JsonTokenReader("\"\\b\"");
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\b"));
		}
		
		[Test]
		public void EscapedFormFeed() {
			var tokenReader = new JsonTokenReader("\"\\f\"");
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\f"));
		}
		
		[Test]
		public void EscapedLineFeed() {
			var tokenReader = new JsonTokenReader("\"\\n\"");
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\n"));
		}
		
		[Test]
		public void EscapedCarriageReturn() {
			var tokenReader = new JsonTokenReader("\"\\r\"");
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\r"));
		}
		
		[Test]
		public void EscapedHorizontalTab() {
			var tokenReader = new JsonTokenReader("\"\\t\"");
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("\t"));
		}
		
		[Test]
		public void EscapedUnicode() {
			var tokenReader = new JsonTokenReader("\"\\u597D\"");
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("好"));
		}

		[Test]
		public void InvalidEscapeCode() {
			var tokenReader = new JsonTokenReader("\"\\g\"");
			Assert.Throws<InvalidJsonException>(() => tokenReader.ConsumeString());
		}

		[Test]
		public void MixedRegularAndEscapedChars() {
			var tokenReader = new JsonTokenReader(new StringReader("\"¿ni\\u597Dma?\""));
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("¿ni好ma?"));
		}

		[Test]
		public void DisallowsControlCharacters() {
			for (int i = 0; i < 0x20; i++) {
				string controlChar = char.ConvertFromUtf32(i);
				Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson($"\"{controlChar}\""); });
			}

			Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson($"\"{char.ConvertFromUtf32(0x7F)}\""); });

			for (int i = 0x80; i <= 0x9F; i++) {
				string controlChar = char.ConvertFromUtf32(i);
				Assert.Throws<InvalidJsonException>(() => { JsonMapper.FromJson($"\"{controlChar}\""); });
			}
		}

		[Test]
		public void AdvancingToRandomCharactersThrows() {
			Assert.Throws<InvalidJsonException>(() => {
				// ReSharper disable once ObjectCreationAsStatement
				new JsonTokenReader("fail");
			});
		}
		
		[Test]
		public void ParsesUnicodeSurrogatePairsCorrectly() {
			var tokenReader = new JsonTokenReader("\"\\ud83d\\ude80\"");
			Assert.That(tokenReader.ConsumeString(), Is.EqualTo("🚀"));
		}
		
		[Test]
		public void ParsesEmojiCorrectly() {
			var tokenReader = new JsonTokenReader("\"🚀\"");
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