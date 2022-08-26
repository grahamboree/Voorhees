using System;
using System.Globalization;
using System.IO;

namespace Voorhees {
    // Writes JSON tokens to a TextWriter
    public class JsonTokenWriter {
        public JsonTokenWriter(TextWriter textWriter, bool prettyPrint) {
            this.prettyPrint = prettyPrint;
            this.textWriter = textWriter;
        }

        public void WriteNull() { WriteIndent(); textWriter.Write("null"); }

        #region Boolean
        public void Write(bool val) { WriteIndent(); textWriter.Write(val ? "true" : "false"); }
        #endregion

        #region Number
        // Integral types
        public void Write(byte val)   { WriteIndent(); textWriter.Write(val); }
        public void Write(sbyte val)  { WriteIndent(); textWriter.Write(val); }
        public void Write(short val)  { WriteIndent(); textWriter.Write(val); }
        public void Write(ushort val) { WriteIndent(); textWriter.Write(val); }
        public void Write(int val)    { WriteIndent(); textWriter.Write(val); }
        public void Write(uint val)   { WriteIndent(); textWriter.Write(val); }
        public void Write(long val)   { WriteIndent(); textWriter.Write(val); }
        public void Write(ulong val)  { WriteIndent(); textWriter.Write(val); }

        // Floating point types
        public void Write(float val)   { WriteIndent(); textWriter.Write(val.ToString(CultureInfo.InvariantCulture)); }
        public void Write(double val)  { WriteIndent(); textWriter.Write(val.ToString(CultureInfo.InvariantCulture)); }
        public void Write(decimal val) { WriteIndent(); textWriter.Write(val.ToString(CultureInfo.InvariantCulture)); }
        #endregion

        #region String
        public void Write(string val) { WriteIndent(); WriteString(val); } 
        public void Write(char val)   { WriteIndent(); WriteString(val.ToString()); }
        #endregion

        #region Array
        public void WriteArrayStart() { WriteIndent(); textWriter.Write(prettyPrint ? "[\n" : "["); indentLevel++; }
        public void WriteArraySeparator() { textWriter.Write(prettyPrint ? ",\n" : ","); }
        public void WriteArrayEnd() { indentLevel--; WriteIndent(); textWriter.Write("]"); }
        #endregion

        #region Object
        public void WriteObjectStart() { WriteIndent(); textWriter.Write(prettyPrint ? "{\n" : "{"); indentLevel++; }
        public void WriteObjectKeyValueSeparator() { textWriter.Write(prettyPrint ? ": " : ":"); skipNextTabs = true; }
        public void WriteObjectEnd() { indentLevel--; WriteIndent(); textWriter.Write("}"); }
        #endregion
        
        public void WriteArrayOrObjectBodyTerminator() { if (prettyPrint) { textWriter.Write("\n"); } }
        
        /////////////////////////////////////////////////
        
        readonly TextWriter textWriter;
        int indentLevel;
        bool skipNextTabs;
        readonly bool prettyPrint;
        
        /////////////////////////////////////////////////
        
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
                Span<char> buffer = stackalloc char[val.Length + 2];
                buffer[0] = '\"';
                val.AsSpan().CopyTo(buffer[1..]);
                buffer[val.Length + 1] = '\"';
                textWriter.Write(buffer);
            } else {
                // There are special characters in the string we need to escape.
                Span<char> buffer = stackalloc char[val.Length + 2 + extraLength];
                int bufferIndex = 0;
                buffer[bufferIndex++] = '\"';
                foreach (char c in val) {
                    switch (c) {
                        case '\\': buffer[bufferIndex++] = '\\'; buffer[bufferIndex++] = '\\'; break;
                        case '\"': buffer[bufferIndex++] = '\\'; buffer[bufferIndex++] = '"';  break;
                        case '/':  buffer[bufferIndex++] = '\\'; buffer[bufferIndex++] = '/';  break;
                        case '\b': buffer[bufferIndex++] = '\\'; buffer[bufferIndex++] = 'b';  break;
                        case '\f': buffer[bufferIndex++] = '\\'; buffer[bufferIndex++] = 'f';  break;
                        case '\n': buffer[bufferIndex++] = '\\'; buffer[bufferIndex++] = 'n';  break;
                        case '\r': buffer[bufferIndex++] = '\\'; buffer[bufferIndex++] = 'r';  break;
                        case '\t': buffer[bufferIndex++] = '\\'; buffer[bufferIndex++] = 't';  break;
                        default: {
                            if (c < 32) {
                                buffer[bufferIndex++] = '\\';
                                buffer[bufferIndex++] = 'u';
                                // TODO: This might be faster if it avoided the ToString()?
                                string hex = ((int)c).ToString("X4");
                                hex.AsSpan().CopyTo(buffer.Slice(bufferIndex, 4));
                                bufferIndex += 4;
                            } else {
                                buffer[bufferIndex++] = c;
                            }
                        } break;
                    }
                }
                buffer[bufferIndex] = '\"';
                textWriter.Write(buffer);
            }
        }

        void WriteIndent() {
            if (!prettyPrint) {
                return;
            }
            
            if (skipNextTabs) {
                skipNextTabs = false;
                return;
            }

            if (indentLevel == 0) {
                return;
            }
            
            Span<char> buffer = stackalloc char[indentLevel];
            for (int i = 0; i < indentLevel; ++i) {
                buffer[i] = '\t';
            }
            textWriter.Write(buffer);
        }
    }
}
