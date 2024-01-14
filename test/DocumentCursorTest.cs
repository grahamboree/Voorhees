using System.IO;
using NUnit.Framework;

namespace Voorhees.Tests {
    [TestFixture]
    public class DocumentCursorTest {
        [Test]
        public void TracksSeenNewlines() {
            var doc = new Internal.DocumentCursor(new StringReader(" \n \n"));
            doc.Advance(2);
            Assert.Multiple(() => {
                Assert.That(doc.Index, Is.EqualTo(2));
                Assert.That(doc.ToString(), Is.EqualTo("line: 2 col: 1"));
            });
        }
        
        [Test]
        public void UpdatesColumnValue() {
            var doc = new Internal.DocumentCursor(new StringReader("    "));
            doc.Advance(2);
            Assert.Multiple(() => {
                Assert.That(doc.Index, Is.EqualTo(2));
                Assert.That(doc.ToString(), Is.EqualTo("line: 1 col: 3"));
            });
        }
        
        [Test]
        public void DoesNotReadPastEndOfDocument() {
            var doc = new Internal.DocumentCursor(new StringReader("  "));
            doc.Advance(5);
            Assert.Multiple(() => {
                Assert.That(doc.Index, Is.EqualTo(2));
                Assert.That(doc.ToString(), Is.EqualTo("line: 1 col: 3"));
            });
        }

        [Test]
        public void ReadingANewLineIncrementsLineNumberAndResetsColumnNumberToOne() {
            var doc = new Internal.DocumentCursor(new StringReader("a\nb"));
            doc.Advance(2);
            Assert.Multiple(() => {
                Assert.That(doc.Line, Is.EqualTo(2));
                Assert.That(doc.Column, Is.EqualTo(1));
            });
        }
    }
}