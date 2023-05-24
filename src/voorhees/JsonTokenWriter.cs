using System;
using System.Globalization;
using System.IO;

namespace Voorhees {
    /// Writes JSON tokens to a TextWriter.
    /// Can optionally write tokens in pretty printing mode.
    public class JsonTokenWriter {
        public JsonTokenWriter(TextWriter textWriter, bool prettyPrint) {
            this.prettyPrint = prettyPrint;
            this.textWriter = textWriter;
        }

        #region Null
        public void WriteNull() {
            WriteIndent();
            // Write char by char to avoid string copies.
            textWriter.Write('n');
            textWriter.Write('u');
            textWriter.Write('l');
            textWriter.Write('l');
        }
        #endregion

        #region Boolean
        public void Write(bool val) {
            WriteIndent();
            // Write char by char to avoid string copies.
            if (val) {
                textWriter.Write('t');
                textWriter.Write('r');
                textWriter.Write('u');
                textWriter.Write('e');
            } else {
                textWriter.Write('f');
                textWriter.Write('a');
                textWriter.Write('l');
                textWriter.Write('s');
                textWriter.Write('e');
            }
        }
        #endregion

        #region Number
        // Integral types
        public void Write(byte val) {
            WriteIndent();
            // Using TryFormat and passing the resulting span to the textWriter avoids strings and copies. 
            Span<char> buffer = stackalloc char[4];
            if (val.TryFormat(buffer, out int charsWritten, "D", CultureInfo.InvariantCulture)) {
                textWriter.Write(buffer[..charsWritten]);
            } else {
                // If for some reason the formatting fails, fall back to the built-in writer
                textWriter.Write(val);
            }
        }
        
        public void Write(sbyte val) {
            WriteIndent();
            // Using TryFormat and passing the resulting span to the textWriter avoids strings and copies. 
            Span<char> buffer = stackalloc char[4];
            if (val.TryFormat(buffer, out int charsWritten, "D", CultureInfo.InvariantCulture)) {
                textWriter.Write(buffer[..charsWritten]);
            } else {
                // If for some reason the formatting fails, fall back to the built-in writer
                textWriter.Write(val);
            }
        }

        public void Write(short val) {
            WriteIndent();
            // Using TryFormat and passing the resulting span to the textWriter avoids strings and copies. 
            Span<char> buffer = stackalloc char[8];
            if (val.TryFormat(buffer, out int charsWritten, "D", CultureInfo.InvariantCulture)) {
                textWriter.Write(buffer[..charsWritten]);
            } else {
                // If for some reason the formatting fails, fall back to the built-in writer
                textWriter.Write(val);
            }
        }

        public void Write(ushort val) {
            WriteIndent();
            // Using TryFormat and passing the resulting span to the textWriter avoids strings and copies. 
            Span<char> buffer = stackalloc char[8];
            if (val.TryFormat(buffer, out int charsWritten, "D", CultureInfo.InvariantCulture)) {
                textWriter.Write(buffer[..charsWritten]);
            } else {
                // If for some reason the formatting fails, fall back to the built-in writer
                textWriter.Write(val);
            }
        }
        public void Write(int val) {
            WriteIndent();
            // Using TryFormat and passing the resulting span to the textWriter avoids strings and copies. 
            Span<char> buffer = stackalloc char[16];
            if (val.TryFormat(buffer, out int charsWritten, "D", CultureInfo.InvariantCulture)) {
                textWriter.Write(buffer[..charsWritten]);
            } else {
                // If for some reason the formatting fails, fall back to the built-in writer
                textWriter.Write(val);
            }
        }
        public void Write(uint val) {
            WriteIndent(); 
            // Using TryFormat and passing the resulting span to the textWriter avoids strings and copies. 
            Span<char> buffer = stackalloc char[16];
            if (val.TryFormat(buffer, out int charsWritten, "D", CultureInfo.InvariantCulture)) {
                textWriter.Write(buffer[..charsWritten]);
            } else {
                // If for some reason the formatting fails, fall back to the built-in writer
                textWriter.Write(val);
            }
        }
        public void Write(long val) {
            WriteIndent(); 
            // Using TryFormat and passing the resulting span to the textWriter avoids strings and copies. 
            Span<char> buffer = stackalloc char[32];
            if (val.TryFormat(buffer, out int charsWritten, "D", CultureInfo.InvariantCulture)) {
                textWriter.Write(buffer[..charsWritten]);
            } else {
                // If for some reason the formatting fails, fall back to the built-in writer
                textWriter.Write(val);
            }
        }
        public void Write(ulong val) {
            WriteIndent(); 
            // Using TryFormat and passing the resulting span to the textWriter avoids strings and copies. 
            Span<char> buffer = stackalloc char[32];
            if (val.TryFormat(buffer, out int charsWritten, "D", CultureInfo.InvariantCulture)) {
                textWriter.Write(buffer[..charsWritten]);
            } else {
                // If for some reason the formatting fails, fall back to the built-in writer
                textWriter.Write(val);
            }
        }

        // Floating point types
        public void Write(float val) {
            WriteIndent();
            // Using TryFormat and passing the resulting span to the textWriter avoids strings and copies. 
            Span<char> buffer = stackalloc char[32];
            if (val.TryFormat(buffer, out int charsWritten, "G", CultureInfo.InvariantCulture)) {
                textWriter.Write(buffer[..charsWritten]);
            } else {
                // If for some reason the formatting fails, fall back to using ToString()
                textWriter.Write(val.ToString(CultureInfo.InvariantCulture).AsSpan());
            }
        }
        
        public void Write(double val) {
            WriteIndent();
            // Using TryFormat and passing the resulting span to the textWriter avoids strings and copies. 
            Span<char> buffer = stackalloc char[32];
            if (val.TryFormat(buffer, out int charsWritten, "G", CultureInfo.InvariantCulture)) {
                textWriter.Write(buffer[..charsWritten]);
            } else {
                // If for some reason the formatting fails, fall back to using ToString()
                textWriter.Write(val.ToString(CultureInfo.InvariantCulture).AsSpan());
            }
        }
        
        public void Write(decimal val) {
            WriteIndent();
            // Using TryFormat and passing the resulting span to the textWriter avoids strings and copies. 
            Span<char> buffer = stackalloc char[256];
            if (val.TryFormat(buffer, out int charsWritten, "G", CultureInfo.InvariantCulture)) {
                textWriter.Write(buffer[..charsWritten]);
            } else {
                // If for some reason the formatting fails, fall back to using ToString()
                textWriter.Write(val.ToString(CultureInfo.InvariantCulture).AsSpan());
            }
        }
        #endregion

        #region String
        public void Write(string val) { WriteIndent(); WriteString(val); } 
        public void Write(char val)   { WriteIndent(); WriteString(val.ToString()); }
        #endregion

        #region Array
        public void WriteArrayStart() {
            WriteIndent();
            textWriter.Write('[');
            if (prettyPrint) {
                textWriter.Write('\n');
            }
            indentLevel++;
        }

        public void WriteArrayEnd() {
            indentLevel--;
            WriteIndent();
            textWriter.Write(']');
        }
        #endregion

        #region Object
        public void WriteObjectStart() {
            WriteIndent();
            textWriter.Write('{');
            if (prettyPrint) {
                textWriter.Write('\n');
            }
            indentLevel++;
        }

        public void WriteObjectKey(string key) {
            Write(key);
            textWriter.Write(':');
            if (prettyPrint) {
                textWriter.Write(' ');
            }
            skipNextTabs = true;
        }

        public void WriteObjectEnd() {
            indentLevel--;
            WriteIndent();
            textWriter.Write('}');
        }
        #endregion
        
        /// Separator between elements of an array or key value pairs in an object.
        public void WriteArrayOrObjectSeparator() {
            textWriter.Write(',');
            if (prettyPrint) {
                textWriter.Write('\n');
            }
        }

        /// Call this before writing the end token for arrays and objects.  Adds a newline in pretty printing mode. 
        public void WriteArrayOrObjectBodyTerminator() {
            if (prettyPrint) {
                textWriter.Write('\n');
            }
        }
        
        /////////////////////////////////////////////////
        
        readonly TextWriter textWriter;
        int indentLevel;
        bool skipNextTabs;
        readonly bool prettyPrint;
        
        /////////////////////////////////////////////////
        
        /// <summary>
        /// Writes string value with the proper special character sanitization required for JSON strings.
        /// </summary>
        /// <param name="val">The string value to write to the text writer</param>
        void WriteString(string val) {
            int extraLength = 0;
            foreach (char c in val) {
                switch (c) {
                    case '\\': 
                    case '\"': 
                    case '/':  
                    case '\b': 
                    case '\f': 
                    case '\n': 
                    case '\r': 
                    case '\t': 
                        extraLength++; break;
                    default: {
                        if (c < 32) {
                            extraLength += 5;
                        }
                    } break;
                }
            }
            
            if (extraLength == 0) {
                // No special characters, so no escaping necessary; just quote it.
                // Write each char in the string to avoid copies.
                textWriter.Write('\"');
                foreach (char c in val) {
                    textWriter.Write(c);
                }
                textWriter.Write('\"');
            } else {
                // There are special characters in the string we need to escape.
                textWriter.Write('\"');
                Span<char> hexBuffer = stackalloc char[4];
                foreach (char c in val) {
                    switch (c) {
                        case '\\': textWriter.Write('\\'); textWriter.Write('\\'); break;
                        case '\"': textWriter.Write('\\'); textWriter.Write('"');  break;
                        case '/':  textWriter.Write('\\'); textWriter.Write('/');  break;
                        case '\b': textWriter.Write('\\'); textWriter.Write('b');  break;
                        case '\f': textWriter.Write('\\'); textWriter.Write('f');  break;
                        case '\n': textWriter.Write('\\'); textWriter.Write('n');  break;
                        case '\r': textWriter.Write('\\'); textWriter.Write('r');  break;
                        case '\t': textWriter.Write('\\'); textWriter.Write('t');  break;
                        default: {
                            if (c < 32) {
                                textWriter.Write('\\');
                                textWriter.Write('u');
                                
                                if (((int)c).TryFormat(hexBuffer, out int charsWritten, "X4", CultureInfo.InvariantCulture)) {
                                    textWriter.Write(hexBuffer[..charsWritten]);
                                } else {
                                    // If for some reason the formatting fails, fall back to ToString()
                                    textWriter.Write(((int)c).ToString("X4"));
                                }
                            } else {
                                textWriter.Write(c);
                            }
                        } break;
                    }
                }
                textWriter.Write('\"');
            }
        }

        /// In pretty printing mode, writes tabs to indent to the correct level.
        /// If pretty printing is disable, this does nothing.
        void WriteIndent() {
            if (!prettyPrint) {
                return;
            }
            
            if (skipNextTabs) {
                skipNextTabs = false;
                return;
            }
            
            for (int i = 0; i < indentLevel; ++i) {
                textWriter.Write('\t');
            }
        }
    }
}
