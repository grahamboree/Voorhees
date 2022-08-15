using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Voorhees {
    public class JsonWriter {
        public static string ToJson(JsonValue json, bool prettyPrint = false) {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb)) {
                var jsonWriter = new JsonWriter(sw, prettyPrint);
                jsonWriter.Write(json);
            }
            return sb.ToString();
        }

        /////////////////////////////////////////////////

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
                case JsonType.Array: {
                    WriteArrayStart();

                    for (int i = 0; i < val.Count; ++i) {
                        Write(val[i]);

                        if (i < val.Count - 1) {
                            WriteArraySeparator();
                        } else {
                            WriteArrayListTerminator();
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
                        WriteArrayListTerminator();
                    }

                    WriteObjectEnd();
                } break;
                case JsonType.Null:
                default: WriteNull(); break;
            }
        }

        public void WriteNull() { tabs(); writer.Write("null"); }

        #region Json Boolean
        public void Write(bool val) { tabs(); writer.Write(val ? "true" : "false"); }
        #endregion

        #region Json Number
        // Integral types
        public void Write(byte val)   { tabs(); writer.Write(val); }
        public void Write(sbyte val)  { tabs(); writer.Write(val); }
        public void Write(short val)  { tabs(); writer.Write(val); }
        public void Write(ushort val) { tabs(); writer.Write(val); }
        public void Write(int val)    { tabs(); writer.Write(val); }
        public void Write(uint val)   { tabs(); writer.Write(val); }
        public void Write(long val)   { tabs(); writer.Write(val); }
        public void Write(ulong val)  { tabs(); writer.Write(val); }

        // Floating point types
        public void Write(float val)   { tabs(); writer.Write(val.ToString(CultureInfo.InvariantCulture)); }
        public void Write(double val)  { tabs(); writer.Write(val.ToString(CultureInfo.InvariantCulture)); }
        public void Write(decimal val) { tabs(); writer.Write(val.ToString(CultureInfo.InvariantCulture)); }
        #endregion

        #region Json String
        public void Write(string val) { tabs(); WriteString(val); } 
        public void Write(char val) { Write(val.ToString()); }
        #endregion

        #region Json Array
        public void WriteArrayStart() { tabs(); writer.Write(prettyPrint ? "[\n" : "["); indentLevel++; }
        public void WriteArraySeparator() { writer.Write(prettyPrint ? ",\n" : ","); }
        public void WriteArrayListTerminator() { if (prettyPrint) { writer.Write("\n"); } }
        public void WriteArrayEnd() { indentLevel--; tabs(); writer.Write("]"); }
        #endregion

        #region Json Object
        public void WriteObjectStart() { tabs(); writer.Write(prettyPrint ? "{\n" : "{"); indentLevel++; }
        public void WriteObjectKeyValueSeparator() { writer.Write(prettyPrint ? ": " : ":"); skipNextTabs = true; }
        public void WriteObjectEnd() { indentLevel--; tabs(); writer.Write("}"); }
        #endregion
        
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
                                // TODO: This might be faster if we avoided the ToString() ?
                                string hex = ((int)c).ToString("X4");
                                hex.CopyTo(buffer.Slice(bufferIndex, 4));
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

        void tabs() {
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
