using System;
using NUnit.Framework;

namespace Voorhees.Tests {
	[TestFixture]
	public class JsonTokenizer_SkipToken {
		[Test]
		public void ArrayStart() {
			var doc = new Internal.DocumentCursor("[1,2,3]");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.ArrayStart);
			Assert.That(doc.Index, Is.EqualTo(1));
		}
		
		[Test]
		public void ArrayEnd() {
			var doc = new Internal.DocumentCursor("][1,2,3]");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.ArrayEnd);
			Assert.That(doc.Index, Is.EqualTo(1));
		}
		
		[Test]
		public void ObjectStart() {
			var doc = new Internal.DocumentCursor("{\"test\": 123}");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.ObjectStart);
			Assert.That(doc.Index, Is.EqualTo(1));
		}
		
		[Test]
		public void KeyValueSeparator() {
			var doc = new Internal.DocumentCursor(":123}");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.KeyValueSeparator);
			Assert.That(doc.Index, Is.EqualTo(1));
		}
		
		[Test]
		public void ObjectEnd() {
			var doc = new Internal.DocumentCursor("}{\"test\": 123}");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.ObjectEnd);
			Assert.That(doc.Index, Is.EqualTo(1));
		}
		
		[Test]
		public void Separator() {
			var doc = new Internal.DocumentCursor(",{\"test\": 123}");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.Separator);
			Assert.That(doc.Index, Is.EqualTo(1));
		}

		[Test]
		public void True() {
			var doc = new Internal.DocumentCursor("true, true");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.True);
			Assert.That(doc.Index, Is.EqualTo(4));
		}
		
		[Test]
		public void False() {
			var doc = new Internal.DocumentCursor("false, false");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.False);
			Assert.That(doc.Index, Is.EqualTo(5));
		}
		
		[Test]
		public void Null() {
			var doc = new Internal.DocumentCursor("null, null");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.Null);
			Assert.That(doc.Index, Is.EqualTo(4));
		}

		
		[Test]
		public void String() {
			var doc = new Internal.DocumentCursor("\"test\", 123");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.String);
			Assert.That(doc.Index, Is.EqualTo(6));
		}
		
		[Test]
		public void Number() {
			var doc = new Internal.DocumentCursor("-123.456e7, 123");
			var tokenizer = new JsonTokenizer(doc);
            tokenizer.SkipToken(JsonToken.Number);
			Assert.That(doc.Index, Is.EqualTo(10));
		}

		[Test]
		public void SkipsTrailingWhitespace() {
			var doc = new Internal.DocumentCursor("true    , false");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.True);
			Assert.That(doc.Index, Is.EqualTo(8));
		}
		
		[Test]
		public void SkippingTheWrongTokenThrows() {
			var tokenizer = new JsonTokenizer("true, false");
			Assert.Throws<InvalidOperationException>(() => tokenizer.SkipToken(JsonToken.ArrayStart));
		}
		
		[Test]
		public void SkippingEOFThrows() {
			var tokenizer = new JsonTokenizer("");
			Assert.Throws<InvalidOperationException>(() => tokenizer.SkipToken(JsonToken.EOF));
		}

		[Test]
		public void SkippingStringWithEscapedCharacter() {
			var doc = new Internal.DocumentCursor("\"test\\\"\", false");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.String);
			Assert.That(doc.Index, Is.EqualTo(8));
		}

		[Test]
		public void SkippingStringContainingEscapedUnicodeCharacter() {
			var doc = new Internal.DocumentCursor("\"\\u597D\", false");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.String);
			Assert.That(doc.Index, Is.EqualTo(8));
		}

		[Test]
		public void SkippingStringsDisallowsControlCharacters() {
			for (int i = 0; i < 0x20; i++) {
				string controlChar = char.ConvertFromUtf32(i);
				Assert.Throws<InvalidJsonException>(() => {
					new JsonTokenizer($"\"{controlChar}\"").SkipToken(JsonToken.String);
				});
			}

			Assert.Throws<InvalidJsonException>(() => {
				new JsonTokenizer($"\"{char.ConvertFromUtf32(0x7F)}\"").SkipToken(JsonToken.String);
			});

			for (int i = 0x80; i <= 0x9F; i++) {
				string controlChar = char.ConvertFromUtf32(i);
				Assert.Throws<InvalidJsonException>(() => {
					new JsonTokenizer($"\"{controlChar}\"").SkipToken(JsonToken.String);
				});
			}
		}
    }

	[TestFixture]
	public class JsonTokenizer_ConsumeNumber {
		[Test]
		public void NotANumberNext() {
			var tokenizer = new JsonTokenizer("true");
			Assert.Throws<InvalidOperationException>(() => tokenizer.ConsumeNumber());
		}
		
		[Test]
		public void LeadingZeroMustHaveDecimalOrExponent() {
			var tokenizer = new JsonTokenizer("0123");
			Assert.Throws<InvalidJsonException>(() => tokenizer.ConsumeNumber());
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
			string tokenString = new(new JsonTokenizer(json).ConsumeNumber());
			Assert.That(tokenString, Is.EqualTo(json));
		}
	}

	[TestFixture]
	public class JsonTokenizer_ConsumeString {
		[Test]
		public void BasicString() {
			var tokenizer = new JsonTokenizer("\"test\"");
			Assert.That(tokenizer.ConsumeString(), Is.EqualTo("test"));
		}

		[Test]
		public void EscapedQuotes() {
			var tokenizer = new JsonTokenizer("\"\\\"\"");
			Assert.That(tokenizer.ConsumeString(), Is.EqualTo("\""));
		}
		
		[Test]
		public void EscapedBackslash() {
			var tokenizer = new JsonTokenizer("\"\\\\\"");
			Assert.That(tokenizer.ConsumeString(), Is.EqualTo("\\"));
		}
		
		[Test]
		public void EscapedForwardSlash() {
			var tokenizer = new JsonTokenizer("\"\\/\"");
			Assert.That(tokenizer.ConsumeString(), Is.EqualTo("/"));
		}
		
		[Test]
		public void EscapedBackspace() {
			var tokenizer = new JsonTokenizer("\"\\b\"");
			Assert.That(tokenizer.ConsumeString(), Is.EqualTo("\b"));
		}
		
		[Test]
		public void EscapedFormFeed() {
			var tokenizer = new JsonTokenizer("\"\\f\"");
			Assert.That(tokenizer.ConsumeString(), Is.EqualTo("\f"));
		}
		
		[Test]
		public void EscapedLineFeed() {
			var tokenizer = new JsonTokenizer("\"\\n\"");
			Assert.That(tokenizer.ConsumeString(), Is.EqualTo("\n"));
		}
		
		[Test]
		public void EscapedCarriageReturn() {
			var tokenizer = new JsonTokenizer("\"\\r\"");
			Assert.That(tokenizer.ConsumeString(), Is.EqualTo("\r"));
		}
		
		[Test]
		public void EscapedHorizontalTab() {
			var tokenizer = new JsonTokenizer("\"\\t\"");
			Assert.That(tokenizer.ConsumeString(), Is.EqualTo("\t"));
		}
		
		[Test]
		public void EscapedUnicode() {
			var tokenizer = new JsonTokenizer("\"\\u597D\"");
			Assert.That(tokenizer.ConsumeString(), Is.EqualTo("好"));
		}

		[Test]
		public void InvalidEscapeCode() {
			var tokenizer = new JsonTokenizer("\"\\g\"");
			Assert.Throws<InvalidJsonException>(() => tokenizer.ConsumeString());
		}

		[Test]
		public void MixedRegularAndEscapedChars() {
			var tokenizer = new JsonTokenizer("\"¿ni\\u597Dma?\"");
			Assert.That(tokenizer.ConsumeString(), Is.EqualTo("¿ni好ma?"));
		}

		[Test]
		public void DisallowsControlCharacters() {
			for (int i = 0; i < 0x20; i++) {
				string controlChar = char.ConvertFromUtf32(i);
				Assert.Throws<InvalidJsonException>(() => { JsonReader.Read($"\"{controlChar}\""); });
			}

			Assert.Throws<InvalidJsonException>(() => { JsonReader.Read($"\"{char.ConvertFromUtf32(0x7F)}\""); });

			for (int i = 0x80; i <= 0x9F; i++) {
				string controlChar = char.ConvertFromUtf32(i);
				Assert.Throws<InvalidJsonException>(() => { JsonReader.Read($"\"{controlChar}\""); });
			}
		}

		[Test]
		public void AdvancingToRandomCharactersThrows() {
			Assert.Throws<InvalidJsonException>(() => {
				// ReSharper disable once ObjectCreationAsStatement
				new JsonTokenizer("fail");
			});
		}
		
		[Test]
		public void ParsesUnicodeSurrogatePairsCorrectly() {
			var tokenizer = new JsonTokenizer("\"\\ud83d\\ude80\"");
			Assert.That(tokenizer.ConsumeString(), Is.EqualTo("🚀"));
		}
		
		[Test]
		public void ParsesEmojiCorrectly() {
			var tokenizer = new JsonTokenizer("\"🚀\"");
			Assert.That(tokenizer.ConsumeString(), Is.EqualTo("🚀"));
		}

		[Test]
		public void DoesNotReadPastEndOfString() {
			var doc = new Internal.DocumentCursor("{\"test\": 3}");
			var tokenizer = new JsonTokenizer(doc);
			tokenizer.SkipToken(JsonToken.ObjectStart);
			string str = tokenizer.ConsumeString();
			Assert.Multiple(() => {
				Assert.That(str, Is.EqualTo("test"));
				Assert.That(doc.Index, Is.EqualTo(7));
			});
		}

		[Test]
		public void DoubleQuotedString()  {
			var doc = new Internal.DocumentCursor("\"\\\"test\\\"\",");
            var tokenizer = new JsonTokenizer(doc);
            Assert.Multiple(() => {
                Assert.That(tokenizer.ConsumeString(), Is.EqualTo("\"test\""));
                Assert.That(tokenizer.NextToken, Is.EqualTo(JsonToken.Separator));
            });
        }
    }
}