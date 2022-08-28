using System.Runtime.CompilerServices;

namespace Voorhees.Internal {
    /// A cursor in a document string that tracks a position 
    /// forward through the document as well as current line and column 
    /// numbers.  Only moves forward through the document.
    public class DocumentCursor {
        public readonly string Document;
        readonly int DocLength;

        /// Index of the current character in the entire json document.
        public int Index;

        /// The character the cursor is currently pointing to.
        public char CurrentChar;
        
        public int NumCharsLeft {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => DocLength - Index;
        }
        
        public bool AtEOF {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Index >= DocLength;
        }

        /////////////////////////////////////////////////

        /// <summary>
        /// Create a document cursor at the start of the given document
        /// </summary>
        /// <param name="document">The document to read</param>
        public DocumentCursor(string document) {
            Document = document;
            DocLength = Document.Length;
            Index = 0;
            CurrentChar = !AtEOF ? Document[Index] : '\0';
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
            Index += numChars < NumCharsLeft ? numChars : NumCharsLeft;
            CurrentChar = Index < DocLength ? Document[Index] : '\0';
        }

        /// Advances the read position forward one character.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance() {
            if (Index >= DocLength) {
                return;
            }
            Index++;
            CurrentChar = Index < DocLength ? Document[Index] : '\0';
        }

        /// <summary>
        /// Generates a string containing info about the current line and column numbers that's useful for
        /// prepending to error messages and exceptions.
        /// </summary>
        /// <returns>string containing line and column info for the current cursor position</returns>
        public override string ToString() {
            // Compute the line and column number.
            // We could keep track of this as we move through the document
            // but we don't need it unless we're throwing an exception, so it's
            // fine to just compute this on-demand.
            int line = 1;
            int column = 1;
            for (int i = 0; i < Index; i++) {
                if (Document[i] == '\n') {
                    line++;
                    column = 0;
                }
                column++;
            }
            return $"line: {line} col: {column}";
        }
    }
}