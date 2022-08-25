﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Voorhees {
    // Writes JSON text to a TextWriter
    public class JsonWriter {
        public JsonWriter(TextWriter textWriter, bool prettyPrint) {
            this.prettyPrint = prettyPrint;
            writer = textWriter;
        }
        
        public void Write(JsonValue val) {
            if (val == null) {
                WriteNull();
                return;
            }

            switch (val.Type) {
                case JsonType.Int:     Write((int) val); break;
                case JsonType.Float:   Write((float) val); break;
                case JsonType.Boolean: Write((bool) val); break;
                case JsonType.String:  Write((string) val); break;
                case JsonType.Null:    WriteNull(); break;
                case JsonType.Array: {
                    WriteArrayStart();

                    for (int i = 0; i < val.Count; ++i) {
                        Write(val[i]);

                        if (i < val.Count - 1) {
                            WriteArraySeparator();
                        } else {
                            WriteArrayOrObjectBodyTerminator();
                        }
                    }

                    WriteArrayEnd();
                } break;
                case JsonType.Object: {
                    WriteObjectStart();

                    bool first = true;
                    foreach (var objectPair in val as IEnumerable<KeyValuePair<string, JsonValue>>) {
                        if (!first) {
                            WriteArraySeparator();
                        }
                        first = false;

                        Write(objectPair.Key);
                        WriteObjectKeyValueSeparator();
                        skipNextTabs = true;
                        Write(objectPair.Value);
                    }

                    if (val.Count > 0) {
                        WriteArrayOrObjectBodyTerminator();
                    }

                    WriteObjectEnd();
                } break;
                case JsonType.Unspecified: 
                default:
                    throw new InvalidOperationException("Can't write JsonValue instance because it is of unspecified type");
            }
        }

        public void WriteNull() { WriteIndent(); writer.Write("null"); }

        #region Json Boolean
        public void Write(bool val) { WriteIndent(); writer.Write(val ? "true" : "false"); }
        #endregion

        #region Json Number
        // Integral types
        public void Write(byte val)   { WriteIndent(); writer.Write(val); }
        public void Write(sbyte val)  { WriteIndent(); writer.Write(val); }
        public void Write(short val)  { WriteIndent(); writer.Write(val); }
        public void Write(ushort val) { WriteIndent(); writer.Write(val); }
        public void Write(int val)    { WriteIndent(); writer.Write(val); }
        public void Write(uint val)   { WriteIndent(); writer.Write(val); }
        public void Write(long val)   { WriteIndent(); writer.Write(val); }
        public void Write(ulong val)  { WriteIndent(); writer.Write(val); }

        // Floating point types
        public void Write(float val)   { WriteIndent(); writer.Write(val.ToString(CultureInfo.InvariantCulture)); }
        public void Write(double val)  { WriteIndent(); writer.Write(val.ToString(CultureInfo.InvariantCulture)); }
        public void Write(decimal val) { WriteIndent(); writer.Write(val.ToString(CultureInfo.InvariantCulture)); }
        #endregion

        #region Json String
        public void Write(string val) { WriteIndent(); WriteString(val); } 
        public void Write(char val)   { WriteIndent(); WriteString(val.ToString()); }
        #endregion

        #region Json Array
        public void WriteArrayStart() { WriteIndent(); writer.Write(prettyPrint ? "[\n" : "["); indentLevel++; }
        public void WriteArraySeparator() { writer.Write(prettyPrint ? ",\n" : ","); }
        public void WriteArrayEnd() { indentLevel--; WriteIndent(); writer.Write("]"); }
        #endregion

        #region Json Object
        public void WriteObjectStart() { WriteIndent(); writer.Write(prettyPrint ? "{\n" : "{"); indentLevel++; }
        public void WriteObjectKeyValueSeparator() { writer.Write(prettyPrint ? ": " : ":"); skipNextTabs = true; }
        public void WriteObjectEnd() { indentLevel--; WriteIndent(); writer.Write("}"); }
        #endregion
        
        public void WriteArrayOrObjectBodyTerminator() { if (prettyPrint) { writer.Write("\n"); } }
        
        /////////////////////////////////////////////////
        
        readonly TextWriter writer;
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
                writer.Write(buffer);
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
                writer.Write(buffer);
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
            writer.Write(buffer);
        }
    }
}
