using System.Runtime.CompilerServices;

namespace Voorhees.Internal {
    /// A cursor in a document string that tracks a position 
    /// forward through the document as well as line and column 
    /// number information
    public class DocumentCursor {
        public readonly string Document;

        /// Index of the current character in the entire json document. 0-indexed
        public int Cursor;
        /// Index of the current line in the json document. 1-indexed
        public int Line;
        /// Index of the current character on the current line in the json document.  1-indexed
        public int Column;

        public int CharsLeft => Document.Length - Cursor;

        /////////////////////////////////////////////////

        public DocumentCursor(string document) {
            Document = document;
            Cursor = 0;
            Line = 1;
            Column = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceToNextNonWhitespaceChar() {
            while (Cursor < Document.Length && char.IsWhiteSpace(Document[Cursor])) {
                StepCursor();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceCursorBy(int numChars) {
            for (int i = 0; i < numChars; ++i) {
                StepCursor();
            }
        }

        public DocumentCursor Clone() {
            return (DocumentCursor)MemberwiseClone();
        }

        /// <summary>
        /// Generates a string containing info about the current line and column numbers that's useful for
        /// prepending to error messages and exceptions.
        /// </summary>
        /// <returns>string containing line and column info for the current cursor position</returns>
        public override string ToString() {
            return $"line: {Line} col: {Column}";
        }

        /////////////////////////////////////////////////

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void StepCursor() {
            if (Cursor < Document.Length) {
                if (Document[Cursor] == '\n') {
                    Line++;
                    Column = 0;
                }
                Cursor++;
                Column++;
            }
        }
    }
}