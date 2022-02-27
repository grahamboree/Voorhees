using System;
using System.Collections.Generic;
using System.Globalization;

namespace Voorhees {
    public class Voorhees {
        public static Voorhees Instance = new Voorhees();
        
        /////////////////////////////////////////////////
        
        public delegate void ExporterFunc<in T>(T objectToSerialize, JsonOutputStream os);
        public delegate T ImporterFunc<out T>(JsonValue jsonData);
        public delegate T LowLevelImporterFunc<out T>(JsonTokenizer jsonData);

        /////////////////////////////////////////////////

        public void RegisterExporter<T>(ExporterFunc<T> exporter) {
            CustomExporters[typeof(T)] = (obj, os) => exporter((T) obj, os);
        }

        public void UnRegisterExporter<T>() {
            CustomExporters.Remove(typeof(T));
        }

        public void UnRegisterAllExporters() {
            CustomExporters.Clear();
        }

        public void RegisterImporter<T>(ImporterFunc<T> importer) {
            CustomImporters[typeof(T)] = json => importer(json);
        }

        public void RegisterImporter<T>(LowLevelImporterFunc<T> importer) {
            LowLevelCustomImporters[typeof(T)] = json => importer(json);
        }

        public void UnRegisterImporter<T>() {
            CustomImporters.Remove(typeof(T));
            LowLevelCustomImporters.Remove(typeof(T));
        }

        public void UnRegisterAllImporters() {
            CustomImporters.Clear();
            LowLevelCustomImporters.Clear();
        }

        /////////////////////////////////////////////////

        internal delegate void ExporterFunc(object obj, JsonOutputStream os);
        internal static readonly Dictionary<Type, ExporterFunc> BuiltInExporters = new Dictionary<Type, ExporterFunc>();
        internal readonly Dictionary<Type, ExporterFunc> CustomExporters = new Dictionary<Type, ExporterFunc>();
        
        internal delegate object ImporterFunc(JsonValue input);
        internal static readonly Dictionary<Type, LowLevelImporterFunc> BuiltInImporters = new Dictionary<Type, LowLevelImporterFunc>();
        
        internal delegate object LowLevelImporterFunc(JsonTokenizer input);
        internal readonly Dictionary<Type, ImporterFunc> CustomImporters = new Dictionary<Type, ImporterFunc>();
        internal readonly Dictionary<Type, LowLevelImporterFunc> LowLevelCustomImporters = new Dictionary<Type, LowLevelImporterFunc>();

        /////////////////////////////////////////////////

        static Voorhees() {
            BuiltInExporters[typeof(DateTime)] = (obj, os) =>
                os.Write(((DateTime) obj).ToString("o"));
            BuiltInExporters[typeof(DateTimeOffset)] = (obj, os) =>
                os.Write(((DateTimeOffset) obj).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", DateTimeFormatInfo.InvariantInfo));
            
            BuiltInImporters[typeof(byte)] = json => byte.Parse(json.ConsumeNumber());
            BuiltInImporters[typeof(sbyte)] = json => sbyte.Parse(json.ConsumeNumber());
            BuiltInImporters[typeof(short)] = json => short.Parse(json.ConsumeNumber());
            BuiltInImporters[typeof(ushort)] = json => ushort.Parse(json.ConsumeNumber());
            BuiltInImporters[typeof(int)] = json => int.Parse(json.ConsumeNumber());
            BuiltInImporters[typeof(uint)] = json => uint.Parse(json.ConsumeNumber());
            BuiltInImporters[typeof(long)] = json => long.Parse(json.ConsumeNumber());
            BuiltInImporters[typeof(ulong)] = json => ulong.Parse(json.ConsumeNumber());

            BuiltInImporters[typeof(float)] = json => float.Parse(json.ConsumeNumber());
            BuiltInImporters[typeof(double)] = json => double.Parse(json.ConsumeNumber());
            BuiltInImporters[typeof(decimal)] = json => decimal.Parse(json.ConsumeNumber());
            
            BuiltInImporters[typeof(char)] = json => {
                string stringVal = json.ConsumeString();
                if (stringVal.Length > 1) {
                    throw new FormatException($"Trying to map a string of length > 1 to a char: \"{stringVal}\"");
                }
                return stringVal[0];
            };

            BuiltInImporters[typeof(DateTime)] = json => DateTime.Parse(json.ConsumeString());
            BuiltInImporters[typeof(DateTimeOffset)] = json => DateTimeOffset.Parse(json.ConsumeString());
        }
    }
}
