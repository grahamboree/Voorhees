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
        internal delegate object ImporterFunc(JsonTokenReader tokenReader);
        
        internal static readonly Dictionary<Type, ImporterFunc> BuiltInImporters = new();
        internal static readonly Dictionary<Type, ExporterFunc> BuiltInExporters = new();
        
        internal readonly Dictionary<Type, ImporterFunc> CustomImporters = new();
        internal readonly Dictionary<Type, ExporterFunc> CustomExporters = new();
        
        /////////////////////////////////////////////////

        static Voorhees() {
            BuiltInExporters[typeof(DateTime)] = (obj, tokenWriter) =>
                tokenWriter.Write(((DateTime) obj).ToString("o"));
            BuiltInExporters[typeof(DateTimeOffset)] = (obj, tokenWriter) =>
                tokenWriter.Write(((DateTimeOffset) obj).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", DateTimeFormatInfo.InvariantInfo));
            
            BuiltInImporters[typeof(DateTime)] = tokenReader => DateTime.Parse(tokenReader.ConsumeString());
            BuiltInImporters[typeof(DateTimeOffset)] = tokenReader => DateTimeOffset.Parse(tokenReader.ConsumeString());
        }
    }
}
