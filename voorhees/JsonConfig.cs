using System;
using System.Collections.Generic;
using System.Globalization;

namespace Voorhees {
    public class JsonConfig {
        public static JsonConfig CurrentConfig = new JsonConfig();
        
        /////////////////////////////////////////////////
        
        public delegate void ExporterFunc<in T>(T objectToSerialize, JsonOutputStream os);
        public delegate T ImporterFunc<in TJson, out T>(TJson jsonData);
        
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

        public void RegisterImporter<TJson, TValue>(ImporterFunc<TJson, TValue> importer) {
            RegisterImporter(customImporters, typeof(TJson), typeof(TValue), input => importer((TJson) input));
        }

        public void UnRegisterImporter<TJson, TValue>() {
            customImporters[typeof(TJson)].Remove(typeof(TValue));
        }

        public void UnRegisterAllImporters() {
            customImporters.Clear();
        }

        /////////////////////////////////////////////////

        internal delegate void ExporterFunc(object obj, JsonOutputStream os);
        internal static readonly Dictionary<Type, ExporterFunc> builtInExporters = new Dictionary<Type, ExporterFunc>();
        internal readonly Dictionary<Type, ExporterFunc> customExporters = new Dictionary<Type, ExporterFunc>();

        internal delegate object ImporterFunc(object input);
        internal static readonly Dictionary<Type, Dictionary<Type, ImporterFunc>> builtInImporters = new Dictionary<Type, Dictionary<Type, ImporterFunc>>();
        internal readonly Dictionary<Type, Dictionary<Type, ImporterFunc>> customImporters = new Dictionary<Type, Dictionary<Type, ImporterFunc>>();

        static JsonConfig() {
            builtInExporters[typeof(DateTime)] = (obj, os) => os.Write(((DateTime) obj).ToString("o"));
            builtInExporters[typeof(DateTimeOffset)] = (obj, os) => os.Write(((DateTimeOffset) obj).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", DateTimeFormatInfo.InvariantInfo));
            
            RegisterBaseImporter<int, byte>(Convert.ToByte);
            RegisterBaseImporter<int, sbyte>(Convert.ToSByte);
            RegisterBaseImporter<int, short>(Convert.ToInt16);
            RegisterBaseImporter<int, ushort>(Convert.ToUInt16);
            RegisterBaseImporter<int, uint>(Convert.ToUInt32);
            RegisterBaseImporter<int, long>(Convert.ToInt64);
            RegisterBaseImporter<int, ulong>(Convert.ToUInt64);
            RegisterBaseImporter<int, float>(Convert.ToSingle);
            RegisterBaseImporter<int, double>(Convert.ToDouble);
            RegisterBaseImporter<int, decimal>(Convert.ToDecimal);

            RegisterBaseImporter<float, double>(Convert.ToDouble);
            RegisterBaseImporter<float, decimal>(Convert.ToDecimal);

            RegisterBaseImporter<string, char>(Convert.ToChar);
            RegisterBaseImporter<string, DateTime>(input => Convert.ToDateTime(input, DateTimeFormatInfo.InvariantInfo));
            RegisterBaseImporter<string, DateTimeOffset>(DateTimeOffset.Parse);
        }
        
        static void RegisterBaseImporter<TJson, TValue>(ImporterFunc<TJson, TValue> importer) {
            RegisterImporter(builtInImporters, typeof(TJson), typeof(TValue), input => importer((TJson) input));
        }
        
        static void RegisterImporter(Dictionary<Type, Dictionary<Type, ImporterFunc>> table, Type json_type, Type value_type, ImporterFunc importer) {
            if (!table.ContainsKey(json_type)) {
                table.Add(json_type, new Dictionary<Type, ImporterFunc>());
            }

            table[json_type][value_type] = importer;
        }
    }
}
