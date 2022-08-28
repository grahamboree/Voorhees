using System.Runtime.CompilerServices;

namespace Voorhees.Internal {
    /// A cursor in a document string that tracks a position 
    /// forward through the document as well as current line and column 
    /// numbers.  Only moves forward through the document.
    public class DocumentCursor {
        public readonly string Document;

        /// Index of the current character in the entire json document.
        public int Index;
        /// Current line number in the json document. 1-indexed
        public int Line;
        /// Current column number in the json document.  1-indexed
        public int Column;

        public int NumCharsLeft => Document.Length - Index;
        public bool AtEOF => Index >= Document.Length;
        public char CurrentChar => Document[Index];

        /////////////////////////////////////////////////

        /// <summary>
        /// Create a document cursor at the start of the given document
        /// </summary>
        /// <param name="document">The document to read</param>
        public DocumentCursor(string document) {
            Document = document;
            Index = 0;
            Line = 1;
            Column = 1;
        }
        
        /// Advances to the next non-whitespace character.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceToNextNonWhitespaceChar() {
            while (Index < DocLength && CurrentChar is ' ' or '\n' or '\r' or '\t') {
                Advance();
            }
        }

        /// <summary>
        /// Advances the read position forward  by a specified number of characters
        /// </summary>
        /// <param name="numChars">Characters to advance by</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceBy(int numChars) {
            for (int i = 0; i < numChars; ++i) {
                Advance();
            }
        }

        /// Advances the read position forward one character.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance() {
            if (Index >= Document.Length) {
                return;
            }
            if (Document[Index] == '\n') {
                Line++;
                Column = 0;
            }
            Index++;
            Column++;
        }

        /// <summary>
        /// Generates a string containing info about the current line and column numbers that's useful for
        /// prepending to error messages and exceptions.
        /// </summary>
        /// <returns>string containing line and column info for the current cursor position</returns>
        public override string ToString() {
            return $"line: {Line} col: {Column}";
        }
    }
}