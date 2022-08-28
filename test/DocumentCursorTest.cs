using NUnit.Framework;

namespace Voorhees.Tests {
    [TestFixture]
    public class DocumentCursorTest {
        [Test]
        public void TracksSeenNewlines() {
            var doc = new Internal.DocumentCursor(" \n \n");
            doc.Advance(2);
            Assert.Multiple(() => {
                Assert.That(doc.Index, Is.EqualTo(2));
                Assert.That(doc.ToString(), Is.EqualTo("line: 2 col: 1"));
            });
        }
        
        [Test]
        public void UpdatesColumnValue() {
            var doc = new Internal.DocumentCursor("    ");
            doc.Advance(2);
            Assert.Multiple(() => {
                Assert.That(doc.Index, Is.EqualTo(2));
                Assert.That(doc.ToString(), Is.EqualTo("line: 1 col: 3"));
            });
        }
        
        [Test]
        public void DoesNotReadPastEndOfDocument() {
            var doc = new Internal.DocumentCursor("  ");
            doc.Advance(5);
            Assert.Multiple(() => {
                Assert.That(doc.Index, Is.EqualTo(2));
                Assert.That(doc.ToString(), Is.EqualTo("line: 1 col: 3"));
            });
        }
    }
}