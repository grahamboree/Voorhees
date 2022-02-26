using System;
using System.Collections.Generic;
using System.Globalization;

namespace Voorhees {
    public class JsonConfig {
        public static JsonConfig CurrentConfig = new JsonConfig();
        
        /////////////////////////////////////////////////
        
        public delegate void ExporterFunc<in T>(T objectToSerialize, JsonOutputStream os);
        public delegate T ImporterFunc<out T>(JsonValue jsonData);
        
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
        internal static readonly Dictionary<Type, ImporterFunc> builtInImporters = new Dictionary<Type, ImporterFunc>();
        internal readonly Dictionary<Type, ImporterFunc> customImporters = new Dictionary<Type, ImporterFunc>();

        /////////////////////////////////////////////////

        static JsonConfig() {
            builtInExporters[typeof(DateTime)] = (obj, os) =>
                os.Write(((DateTime) obj).ToString("o"));
            builtInExporters[typeof(DateTimeOffset)] = (obj, os) =>
                os.Write(((DateTimeOffset) obj).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", DateTimeFormatInfo.InvariantInfo));
            
            builtInImporters[typeof(byte)] = jsonValue => Convert.ToByte((int)jsonValue);
            builtInImporters[typeof(sbyte)] = jsonValue => Convert.ToSByte((int)jsonValue);
            builtInImporters[typeof(short)] = jsonValue => Convert.ToInt16((int)jsonValue);
            builtInImporters[typeof(ushort)] = jsonValue => Convert.ToUInt16((int)jsonValue);
            builtInImporters[typeof(int)] = jsonValue => (int)jsonValue;
            builtInImporters[typeof(uint)] = jsonValue => Convert.ToUInt32((int)jsonValue);
            builtInImporters[typeof(long)] = jsonValue => Convert.ToInt64((int)jsonValue);
            builtInImporters[typeof(ulong)] = jsonValue => Convert.ToUInt64((int)jsonValue);
            
            builtInImporters[typeof(float)] = jsonValue => jsonValue.IsFloat ? (float)jsonValue : (int)jsonValue;
            builtInImporters[typeof(double)] = jsonValue => Convert.ToDouble(jsonValue.IsFloat ? (float)jsonValue : (int)jsonValue);
            builtInImporters[typeof(decimal)] = jsonValue => Convert.ToDecimal(jsonValue.IsFloat ? (float)jsonValue : (int)jsonValue);
            
            //FormatException
            //builtInImporters[typeof(char)] = jsonValue => ((string)jsonValue)[0];
            builtInImporters[typeof(char)] = jsonValue => {
                var stringVal = (string)jsonValue;
                if (stringVal.Length > 1) {
                    throw new FormatException($"Trying to map a string of length > 1 to a char: \"{stringVal}\"");
                }
                return ((string)jsonValue)[0];
            };
            
            builtInImporters[typeof(DateTime)] = jsonValue => Convert.ToDateTime((string)jsonValue, DateTimeFormatInfo.InvariantInfo);
            builtInImporters[typeof(DateTimeOffset)] = jsonValue => DateTimeOffset.Parse((string)jsonValue);
        }
    }
}
