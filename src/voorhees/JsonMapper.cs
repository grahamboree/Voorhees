using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Voorhees {
    public partial class JsonMapper {
        public delegate void ExporterFunc<in T>(T objectToSerialize, JsonTokenWriter tokenWriter);
        public delegate T ImporterFunc<out T>(JsonTokenReader tokenReader);
        
        /////////////////////////////////////////////////

        #region Writing
        public static string ToJson<T>(T obj, bool prettyPrint = false) => defaultInstance.Write(obj, prettyPrint);
        public static void ToJson<T>(T obj, JsonTokenWriter tokenWriter) => defaultInstance.Write(obj, tokenWriter);
        public static string ToJson(JsonValue val, bool prettyPrint = false) => Write(val, prettyPrint);
        public static void ToJson(JsonValue val, JsonTokenWriter tokenWriter) => Write(val, tokenWriter);
        #endregion
        
        #region Reading
        public static T FromJson<T>(string jsonString) => defaultInstance.Read<T>(jsonString);
        public static T FromJson<T>(JsonTokenReader tokenReader) => defaultInstance.Read<T>(tokenReader);
        public static JsonValue FromJson(string jsonString) => Read(jsonString);
        public static JsonValue FromJson(JsonTokenReader tokenReader) => Read(tokenReader);
        #endregion
        
        /////////////////////////////////////////////////

        public void RegisterExporter<T>(ExporterFunc<T> exporter) => customExporters[typeof(T)] = (obj, os) => exporter((T) obj, os);
        public void UnRegisterExporter<T>() => customExporters.Remove(typeof(T));
        public void UnRegisterAllExporters() => customExporters.Clear();
        public void RegisterImporter<T>(ImporterFunc<T> importer) => customImporters[typeof(T)] = json => importer(json);
        public void UnRegisterImporter<T>() => customImporters.Remove(typeof(T));
        public void UnRegisterAllImporters() => customImporters.Clear();

        #region Writing
        public string Write<T>(T obj, bool prettyPrint = false) {
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder)) {
                var jsonWriter = new JsonTokenWriter(stringWriter, prettyPrint);
                Write(obj, jsonWriter);
            }
            return stringBuilder.ToString();
        }

        public void Write<T>(T obj, JsonTokenWriter tokenWriter) => WriteValue(obj, typeof(T), obj?.GetType(), tokenWriter);

        public static string Write(JsonValue val, bool prettyPrint = false) {
            // We need to explicitly specify the function body here even
            // though it's identical to the generic version.
            // This forces it to use the overload that takes a JsonValue.
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder)) {
                var jsonWriter = new JsonTokenWriter(stringWriter, prettyPrint);
                Write(val, jsonWriter);
            }
            return stringBuilder.ToString();
        }

        public static void Write(JsonValue val, JsonTokenWriter tokenWriter) => WriteJsonValue(val, tokenWriter);
        #endregion
        
        #region Reading
        public T Read<T>(string jsonString) => Read<T>(new JsonTokenReader(jsonString));

        public T Read<T>(JsonTokenReader tokenReader) {
            var result = ReadValueOfType<T>(tokenReader);
            
            // Make sure there's no additional json in the buffer.
            if (tokenReader.NextToken != JsonToken.EOF) {
                throw new InvalidJsonException($"{tokenReader.LineColString} Expected end of file");
            }
            return result;
        }

        public static JsonValue Read(string jsonString) => Read(new JsonTokenReader(jsonString));

        public static JsonValue Read(JsonTokenReader tokenReader) {
            var result = ReadJsonValue(tokenReader);

            // Make sure there's no additional json in the buffer.
            if (tokenReader.NextToken != JsonToken.EOF) {
                throw new InvalidJsonException($"{tokenReader.LineColString} Expected end of file");
            }
            return result;
        }
        #endregion
        
        /////////////////////////////////////////////////

        static readonly JsonMapper defaultInstance = new();
        
        delegate void ExporterFunc(object obj, JsonTokenWriter os);
        delegate object ImporterFunc(JsonTokenReader tokenReader);
        
        static readonly Dictionary<Type, ExporterFunc> builtInExporters = new();
        
        readonly Dictionary<Type, ImporterFunc> customImporters = new();
        readonly Dictionary<Type, ExporterFunc> customExporters = new();
        
        /////////////////////////////////////////////////
        
        static JsonMapper() {
            builtInExporters[typeof(DateTime)] = (obj, tokenWriter) => tokenWriter.Write(((DateTime) obj).ToString("o"));
            builtInExporters[typeof(DateTimeOffset)] = (obj, tokenWriter) =>
                tokenWriter.Write(((DateTimeOffset) obj).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", DateTimeFormatInfo.InvariantInfo));
        }
    }
}
