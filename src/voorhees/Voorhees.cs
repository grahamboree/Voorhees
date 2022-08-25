using System;
using System.Collections.Generic;
using System.Globalization;

namespace Voorhees {
    public class Voorhees {
        public static Voorhees Instance = new();
        
        /////////////////////////////////////////////////
        
        public delegate void ExporterFunc<in T>(T objectToSerialize, JsonTokenWriter tokenWriter);
        public delegate T ImporterFunc<out T>(JsonTokenReader tokenReader);

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

        public void UnRegisterImporter<T>() {
            CustomImporters.Remove(typeof(T));
        }

        public void UnRegisterAllImporters() {
            CustomImporters.Clear();
        }

        /////////////////////////////////////////////////

        internal delegate void ExporterFunc(object obj, JsonTokenWriter os);
        internal static readonly Dictionary<Type, ExporterFunc> BuiltInExporters = new();
        internal readonly Dictionary<Type, ExporterFunc> CustomExporters = new();
        
        internal delegate object ImporterFunc(JsonTokenReader tokenReader);
        internal static readonly Dictionary<Type, ImporterFunc> BuiltInImporters = new();
        
        internal readonly Dictionary<Type, ImporterFunc> CustomImporters = new();
        
        /////////////////////////////////////////////////

        static Voorhees() {
            BuiltInExporters[typeof(DateTime)] = (obj, tokenWriter) =>
                tokenWriter.Write(((DateTime) obj).ToString("o"));
            BuiltInExporters[typeof(DateTimeOffset)] = (obj, tokenWriter) =>
                tokenWriter.Write(((DateTimeOffset) obj).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", DateTimeFormatInfo.InvariantInfo));
            
            // TODO: These all require boxing the parsed value.  They should be hard-coded to avoid the boxing allocations.
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
