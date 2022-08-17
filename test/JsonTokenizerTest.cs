using NUnit.Framework;

namespace Voorhees.Tests {
	[TestFixture]
	public class JsonTokenizer_SkipToken {
		[Test]
		public void ArrayStart() {
			var tokenizer = new JsonTokenizer("[1,2,3]");
			tokenizer.SkipToken(JsonToken.ArrayStart);
			Assert.That(tokenizer.Cursor, Is.EqualTo(1));
		}
		
		[Test]
		public void ArrayEnd() {
			var tokenizer = new JsonTokenizer("][1,2,3]");
			tokenizer.SkipToken(JsonToken.ArrayEnd);
			Assert.That(tokenizer.Cursor, Is.EqualTo(1));
		}
		
		[Test]
		public void ObjectStart() {
			var tokenizer = new JsonTokenizer("{\"test\": 123}");
			tokenizer.SkipToken(JsonToken.ObjectStart);
			Assert.That(tokenizer.Cursor, Is.EqualTo(1));
		}
		
		[Test]
		public void KeyValueSeparator() {
			var tokenizer = new JsonTokenizer(":123}");
			tokenizer.SkipToken(JsonToken.KeyValueSeparator);
			Assert.That(tokenizer.Cursor, Is.EqualTo(1));
		}
		
		[Test]
		public void ObjectEnd() {
			var tokenizer = new JsonTokenizer("}{\"test\": 123}");
			tokenizer.SkipToken(JsonToken.ObjectEnd);
			Assert.That(tokenizer.Cursor, Is.EqualTo(1));
		}
		
		[Test]
		public void Separator() {
			var tokenizer = new JsonTokenizer(",{\"test\": 123}");
			tokenizer.SkipToken(JsonToken.Separator);
			Assert.That(tokenizer.Cursor, Is.EqualTo(1));
		}

		[Test]
		public void True() {
			var tokenizer = new JsonTokenizer("true, true");
			tokenizer.SkipToken(JsonToken.True);
			Assert.That(tokenizer.Cursor, Is.EqualTo(4));
		}
		
		[Test]
		public void False() {
			var tokenizer = new JsonTokenizer("false, false");
			tokenizer.SkipToken(JsonToken.False);
			Assert.That(tokenizer.Cursor, Is.EqualTo(5));
		}
		
		[Test]
		public void Null() {
			var tokenizer = new JsonTokenizer("null, null");
			tokenizer.SkipToken(JsonToken.Null);
			Assert.That(tokenizer.Cursor, Is.EqualTo(4));
		}

		
		[Test]
		public void String() {
			var tokenizer = new JsonTokenizer("\"test\", 123");
			tokenizer.SkipToken(JsonToken.String);
			Assert.That(tokenizer.Cursor, Is.EqualTo(6));
		}
		
		[Test]
		public void Number() {
			var tokenizer = new JsonTokenizer("-123.456e7, 123");
            tokenizer.SkipToken(JsonToken.Number);
			Assert.That(tokenizer.Cursor, Is.EqualTo(10));
		}

		[Test]
		public void SkipsTrailingWhitespace() {
			var tokenizer = new JsonTokenizer("true    , false");
			tokenizer.SkipToken(JsonToken.True);
			Assert.That(tokenizer.Cursor, Is.EqualTo(8));
		}
		
		[Test]
		public void SkippingTheWrongTokenThrows() {
			var tokenizer = new JsonTokenizer("true, false");
			Assert.Throws<InvalidOperationException>(() => tokenizer.SkipToken(JsonToken.ArrayStart));
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
		public void ValidNumberFormats() {
			TestString("123");
			TestString("-123");
			TestString("1.234");
			TestString("-1.234");
			TestString("0.123");
			TestString("-0.123");
			TestString("0e3");
			TestString("0e+3");
			TestString("0e-3");
			TestString("-0e-3");
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
				new JsonTokenizer("asdf");
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
	}
}