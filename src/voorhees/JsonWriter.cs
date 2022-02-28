using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Voorhees {
    public class JsonWriter {
        public static string ToJson(JsonValue json, bool prettyPrint = false) {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            var jsonWriter = new JsonWriter(sw, prettyPrint);
            jsonWriter.Write(json);
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

        static readonly List<string> tabCache;
        
        readonly TextWriter writer;
        int indentLevel;
        bool skipNextTabs;
        readonly bool prettyPrint;
        
        /////////////////////////////////////////////////

        static JsonWriter() {
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
                writer.Write("\"" + val + "\"");
                return;
            }

            writer.Write("\"");
            foreach (char c in val) {
                switch (c) {
                    case '\\': writer.Write("\\\\"); break;
                    case '\"': writer.Write("\\\""); break;
                    case '/':  writer.Write("\\/");  break;
                    case '\b': writer.Write("\\b");  break;
                    case '\f': writer.Write("\\f");  break;
                    case '\n': writer.Write("\\n");  break;
                    case '\r': writer.Write("\\r");  break;
                    case '\t': writer.Write("\\t");  break;
                    default: {
                        if (c < 32) {
                            writer.Write($"\\u{(int)c:X4}");
                        } else {
                            writer.Write(c);
                        }
                    } break;
                }
            }
            writer.Write("\"");
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

            writer.Write(tabCache[indentLevel - 1]);
        }
    }
}
