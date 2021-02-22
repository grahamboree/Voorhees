using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Voorhees {
    public class JsonOutputStream {
        public virtual void WriteNull() { sb.Append("null"); }

        public virtual void Write(JsonValue val) {
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
                default: throw new ArgumentOutOfRangeException();
            }
        }

        #region Json Boolean
        public virtual void Write(bool val) { sb.Append(val ? "true" : "false"); }
        #endregion

        #region Json Number
        // Integral types
        public virtual void Write(byte val)   { sb.Append(val); }
        public virtual void Write(sbyte val)  { sb.Append(val); }
        public virtual void Write(short val)  { sb.Append(val); }
        public virtual void Write(ushort val) { sb.Append(val); }
        public virtual void Write(int val)    { sb.Append(val); }
        public virtual void Write(uint val)   { sb.Append(val); }
        public virtual void Write(long val)   { sb.Append(val); }
        public virtual void Write(ulong val)  { sb.Append(val); }

        // Floating point types
        public virtual void Write(float val)   { sb.Append(val.ToString(CultureInfo.InvariantCulture)); }
        public virtual void Write(double val)  { sb.Append(val.ToString(CultureInfo.InvariantCulture)); }
        public virtual void Write(decimal val) { sb.Append(val.ToString(CultureInfo.InvariantCulture)); }
        #endregion

        #region Json String
        public virtual void Write(string val) { WriteString(val); }
        public virtual void Write(char val)   { Write("" + val); }
        #endregion

        #region Json Array
        public virtual void WriteArrayStart() { sb.Append("["); }
        public virtual void WriteArraySeparator() { sb.Append(","); }
        public virtual void WriteArrayListTerminator() { }
        public virtual void WriteArrayEnd() { sb.Append("]"); }
        #endregion

        #region Json Object
        public virtual void WriteObjectStart() { sb.Append("{"); }
        public virtual void WriteObjectKeyValueSeparator() { sb.Append(":"); }
        public virtual void WriteObjectEnd() { sb.Append("}"); }
        #endregion

        public void WriteNewline() { sb.Append("\n"); }

        /// Get the json stream contents.
        public override string ToString() { return sb.ToString(); }

        /////////////////////////////////////////////////

        protected readonly StringBuilder sb = new StringBuilder();
        protected bool skipNextTabs;

        /////////////////////////////////////////////////


        protected void WriteString(string val) {
            sb.Append(StringToJsonString(val));
        }

        /// Properly escapes special characters and wraps the string in quotes. 
        internal static string StringToJsonString(string val) {
            // Replace \ first because other characters expand into sequences that contain \
            return "\"" + val
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("/", "\\/")
                .Replace("\b", "\\b")
                .Replace("\f", "\\f")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                + "\"";
        }
    }

    public class PrettyPrintJsonOutputStream : JsonOutputStream {
        public PrettyPrintJsonOutputStream() {
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

        public override void WriteNull() { tabs(); sb.Append("null"); }

        #region Json Boolean
        public override void Write(bool val) { tabs(); sb.Append(val ? "true" : "false"); }
        #endregion

        #region Json Number
        // Integral types
        public override void Write(byte val)   { tabs(); sb.Append(val); }
        public override void Write(sbyte val)  { tabs(); sb.Append(val); }
        public override void Write(short val)  { tabs(); sb.Append(val); }
        public override void Write(ushort val) { tabs(); sb.Append(val); }
        public override void Write(int val)    { tabs(); sb.Append(val); }
        public override void Write(uint val)   { tabs(); sb.Append(val); }
        public override void Write(long val)   { tabs(); sb.Append(val); }
        public override void Write(ulong val)  { tabs(); sb.Append(val); }

        // Floating point types
        public override void Write(float val)   { tabs(); sb.Append(val.ToString(CultureInfo.InvariantCulture)); }
        public override void Write(double val)  { tabs(); sb.Append(val.ToString(CultureInfo.InvariantCulture)); }
        public override void Write(decimal val) { tabs(); sb.Append(val.ToString(CultureInfo.InvariantCulture)); }
        #endregion

        #region Json String
        public override void Write(string val) { tabs(); WriteString(val); } 
        public override void Write(char val) { Write("" + val); }
        #endregion

        #region Json Array
        public override void WriteArrayStart() { tabs(); sb.Append("[\n"); indentLevel++; }
        public override void WriteArraySeparator() { sb.Append(",\n"); }
        public override void WriteArrayListTerminator() { WriteNewline(); }
        public override void WriteArrayEnd() { indentLevel--; tabs(); sb.Append("]"); }
        #endregion

        #region Json Object
        public override void WriteObjectStart() { tabs(); sb.Append("{\n"); indentLevel++; }
        public override void WriteObjectKeyValueSeparator() { sb.Append(": "); }
        public override void WriteObjectEnd() { indentLevel--; tabs(); sb.Append("}"); }
        #endregion

        /////////////////////////////////////////////////

        int indentLevel;
        readonly List<string> tabCache;

        /////////////////////////////////////////////////

        // Append the right number of tabs for the current indent level.
        void tabs() {
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
                for (int j = 0; j < i; ++j) {
                    tabs += "\t";
                }
                tabCache.Add(tabs);
            }

            sb.Append(tabCache[indentLevel - 1]);
        }
    }
}

