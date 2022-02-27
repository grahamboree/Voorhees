using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Voorhees {
    public class JsonOutputStream {
        public JsonOutputStream(bool prettyPrint = false) { this.prettyPrint = prettyPrint; }
        
        public void WriteNull() { tabs(); sb.Append("null"); }

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

        #region Json Boolean
        public void Write(bool val) { tabs(); sb.Append(val ? "true" : "false"); }
        #endregion

        #region Json Number
        // Integral types
        public void Write(byte val)   { tabs(); sb.Append(val); }
        public void Write(sbyte val)  { tabs(); sb.Append(val); }
        public void Write(short val)  { tabs(); sb.Append(val); }
        public void Write(ushort val) { tabs(); sb.Append(val); }
        public void Write(int val)    { tabs(); sb.Append(val); }
        public void Write(uint val)   { tabs(); sb.Append(val); }
        public void Write(long val)   { tabs(); sb.Append(val); }
        public void Write(ulong val)  { tabs(); sb.Append(val); }

        // Floating point types
        public void Write(float val)   { tabs(); sb.Append(val.ToString(CultureInfo.InvariantCulture)); }
        public void Write(double val)  { tabs(); sb.Append(val.ToString(CultureInfo.InvariantCulture)); }
        public void Write(decimal val) { tabs(); sb.Append(val.ToString(CultureInfo.InvariantCulture)); }
        #endregion

        #region Json String
        public void Write(string val) { tabs(); WriteString(val); } 
        public void Write(char val) { Write(val.ToString()); }
        #endregion

        #region Json Array
        public void WriteArrayStart() { tabs(); sb.Append(prettyPrint ? "[\n" : "["); indentLevel++; }
        public void WriteArraySeparator() { sb.Append(prettyPrint ? ",\n" : ","); }
        public void WriteArrayListTerminator() { if (prettyPrint) { sb.Append("\n"); } }
        public void WriteArrayEnd() { indentLevel--; tabs(); sb.Append("]"); }
        #endregion

        #region Json Object
        public void WriteObjectStart() { tabs(); sb.Append(prettyPrint ? "{\n" : "{"); indentLevel++; }
        public void WriteObjectKeyValueSeparator() { sb.Append(prettyPrint ? ": " : ":"); skipNextTabs = true; }
        public void WriteObjectEnd() { indentLevel--; tabs(); sb.Append("}"); }
        #endregion

        /// Get the json stream contents.
        public override string ToString() { return sb.ToString(); }

        /////////////////////////////////////////////////

        static readonly List<string> tabCache;
        readonly StringBuilder sb = new StringBuilder();
        bool skipNextTabs;
        readonly bool prettyPrint;
        int indentLevel;

        /////////////////////////////////////////////////

        static JsonOutputStream() {
            // 20 levels to start with.
            tabCache = new List<string> {
                "\t",
                "\t\t",
                "\t\t\t",
                "\t\t\t\t",
                "\t\t\t\t\t",
                "\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t",
            };
        }

        void WriteString(string val) {
            bool simpleString = true;
            foreach (char c in val) {
                simpleString &= c >= 32 && c != '"' && c != '/' && c != '\\';
            }

            if (simpleString) {
                sb.Append("\"" + val + "\"");
                return;
            }

            sb.Append("\"");
            foreach (char c in val) {
                switch (c) {
                    case '\\': sb.Append("\\\\"); break;
                    case '\"': sb.Append("\\\""); break;
                    case '/':  sb.Append("\\/");  break;
                    case '\b': sb.Append("\\b");  break;
                    case '\f': sb.Append("\\f");  break;
                    case '\n': sb.Append("\\n");  break;
                    case '\r': sb.Append("\\r");  break;
                    case '\t': sb.Append("\\t");  break;
                    default: {
                        if (c < 32) {
                            sb.Append($"\\u{(int)c:X4}");
                        } else {
                            sb.Append(c);
                        }
                    } break;
                }
            }
            sb.Append("\"");
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

            // Add more entries to the tabs cache if necessary.
            for (int i = tabCache.Count; i < indentLevel; ++i) {
                string tabs = "";
                for (int j = 0; j <= i; ++j) {
                    tabs += '\t';
                }
                tabCache.Add(tabs);
            }

            sb.Append(tabCache[indentLevel - 1]);
        }
    }
}

