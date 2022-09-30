using System.IO;
using System.Runtime.CompilerServices;

namespace Voorhees.Internal {
    /// A cursor in a document string that tracks a position 
    /// forward through the document as well as current line and column 
    /// numbers.  Only moves forward through the document a
    /// single character at a time and does not provide random access.
    public class DocumentCursor {
        /// Index of the current character in the entire json document.
        public int Index;

        // These are 1-indexed.
        /// The json line number currently being read
        public int Line = 1;
        /// The column number of the json character being read
        public int Column = 1;

        /// The character the cursor is currently pointing to.
        public char CurrentChar;
        
        /// Where the json is being read from.
        readonly TextReader jsonDataStream;
        
        public bool AtEOF {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => jsonDataStream.Peek() == -1;
        }

        /////////////////////////////////////////////////

        /// <summary>
        /// Create a document cursor at the start of the given document
        /// </summary>
        /// <param name="document">The document to read</param>
        public DocumentCursor(TextReader document) {
            jsonDataStream = document;
            Index = 0;

            int readChar = jsonDataStream.Peek();
            
            CurrentChar = readChar == -1 ? '\0' : (char) readChar;
        }
        
        /// Advances to the next non-whitespace character.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceToNextNonWhitespaceChar() {
            while (!AtEOF && CurrentChar is ' ' or '\n' or '\r' or '\t') {
                Advance();
            }
        }

        /// <summary>
        /// Advances the read position forward  by a specified number of characters
        /// </summary>
        /// <param name="numChars">Characters to advance by</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int numChars = 1) {
            for (int i = 0; i < numChars; ++i) {
                jsonDataStream.Read(); // Skip past the current char.
                int readChar = jsonDataStream.Peek();
                Column++;
                Index++;

                if (readChar == -1) {
                    CurrentChar = '\0';
                    return;
                }
                CurrentChar = (char)readChar;
                
                if (CurrentChar == '\n') {
                    Line++;
                    Column = 0;
                }
            }
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