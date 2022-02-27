using System;
using System.Collections.Generic;
using System.Globalization;

namespace Voorhees {
    public class JsonConfig {
        public static JsonConfig CurrentConfig = new JsonConfig();
        
        /////////////////////////////////////////////////
        
        public delegate void ExporterFunc<in T>(T objectToSerialize, JsonOutputStream os);
        public delegate T ImporterFunc<out T>(JsonValue jsonData);
        public delegate T LowLevelImporterFunc<out T>(JsonTokenizer jsonData);
        
        /////////////////////////////////////////////////

        public bool PrettyPrint;
        
        /////////////////////////////////////////////////

        public void RegisterExporter<T>(ExporterFunc<T> exporter) {
            customExporters[typeof(T)] = (obj, os) => exporter((T) obj, os);
        }

        public void UnRegisterExporter<T>() {
            customExporters.Remove(typeof(T));
        }

        public void UnRegisterAllExporters() {
            customExporters.Clear();
        }

        public void RegisterImporter<T>(ImporterFunc<T> importer) {
            customImporters[typeof(T)] = json => importer(json);
        }

        public void UnRegisterImporter<T>() {
            customImporters.Remove(typeof(T));
        }

        public void UnRegisterAllImporters() {
            customImporters.Clear();
        }

        /////////////////////////////////////////////////

        internal delegate void ExporterFunc(object obj, JsonOutputStream os);
        internal static readonly Dictionary<Type, ExporterFunc> builtInExporters = new Dictionary<Type, ExporterFunc>();
        internal readonly Dictionary<Type, ExporterFunc> customExporters = new Dictionary<Type, ExporterFunc>();

        internal delegate object ImporterFunc(JsonValue input);
        internal delegate object LowLevelImporterFunc(JsonTokenizer input);
        internal static readonly Dictionary<Type, LowLevelImporterFunc> builtInImporters =
            new Dictionary<Type, LowLevelImporterFunc>();
        internal readonly Dictionary<Type, ImporterFunc> customImporters = new Dictionary<Type, ImporterFunc>();

        /////////////////////////////////////////////////

        static JsonConfig() {
            builtInExporters[typeof(DateTime)] = (obj, os) =>
                os.Write(((DateTime) obj).ToString("o"));
            builtInExporters[typeof(DateTimeOffset)] = (obj, os) =>
                os.Write(((DateTimeOffset) obj).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", DateTimeFormatInfo.InvariantInfo));
            
            builtInImporters[typeof(byte)] = json => byte.Parse(json.ConsumeNumber());
            builtInImporters[typeof(sbyte)] = json => sbyte.Parse(json.ConsumeNumber());
            builtInImporters[typeof(short)] = json => short.Parse(json.ConsumeNumber());
            builtInImporters[typeof(ushort)] = json => ushort.Parse(json.ConsumeNumber());
            builtInImporters[typeof(int)] = json => int.Parse(json.ConsumeNumber());
            builtInImporters[typeof(uint)] = json => uint.Parse(json.ConsumeNumber());
            builtInImporters[typeof(long)] = json => long.Parse(json.ConsumeNumber());
            builtInImporters[typeof(ulong)] = json => ulong.Parse(json.ConsumeNumber());

            builtInImporters[typeof(float)] = json => float.Parse(json.ConsumeNumber());
            builtInImporters[typeof(double)] = json => double.Parse(json.ConsumeNumber());
            builtInImporters[typeof(decimal)] = json => decimal.Parse(json.ConsumeNumber());
            
            builtInImporters[typeof(char)] = json => {
                string stringVal = json.ConsumeString();
                if (stringVal.Length > 1) {
                    throw new FormatException($"Trying to map a string of length > 1 to a char: \"{stringVal}\"");
                }
                return stringVal[0];
            };

            builtInImporters[typeof(DateTime)] = json => DateTime.Parse(json.ConsumeString());
            builtInImporters[typeof(DateTimeOffset)] = json => DateTimeOffset.Parse(json.ConsumeString());
        }
    }
}
