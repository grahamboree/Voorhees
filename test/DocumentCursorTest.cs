using NUnit.Framework;

namespace Voorhees.Tests {
    [TestFixture]
    public class DocumentCursorTest {
        [Test]
        public void TracksSeenNewlines() {
            var doc = new Internal.DocumentCursor(" \n \n");
            doc.AdvanceCursorBy(2);
            Assert.Multiple(() => {
                Assert.That(doc.Cursor, Is.EqualTo(2));
                Assert.That(doc.Line, Is.EqualTo(2));
                Assert.That(doc.Column, Is.EqualTo(1));
            });
        }
        
        [Test]
        public void UpdatesColumnValue() {
            var doc = new Internal.DocumentCursor("    ");
            doc.AdvanceCursorBy(2);
            Assert.Multiple(() => {
                Assert.That(doc.Cursor, Is.EqualTo(2));
                Assert.That(doc.Line, Is.EqualTo(1));
                Assert.That(doc.Column, Is.EqualTo(3));
            });
        }
        
        [Test]
        public void DoesNotReadPastEndOfDocument() {
            var doc = new Internal.DocumentCursor("  ");
            doc.AdvanceCursorBy(5);
            Assert.Multiple(() => {
                Assert.That(doc.Cursor, Is.EqualTo(2));
                Assert.That(doc.Line, Is.EqualTo(1));
                Assert.That(doc.Column, Is.EqualTo(3));
            });
        }
    }
}