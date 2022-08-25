using System.IO;
using System.Text;

namespace Voorhees {
    public static partial class JsonMapper {
        #region Writing
        public static string ToJson<T>(T obj, bool prettyPrint = false) {
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder)) {
                var jsonWriter = new JsonTokenWriter(stringWriter, prettyPrint);
                ToJson(obj, jsonWriter);
            }
            return stringBuilder.ToString();
        }

        public static string ToJson(JsonValue val, bool prettyPrint = false) {
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder)) {
                var jsonWriter = new JsonTokenWriter(stringWriter, prettyPrint);
                ToJson(val, jsonWriter);
            }
            return stringBuilder.ToString();
        }

        public static void ToJson<T>(T obj, JsonTokenWriter tokenWriter) {
            WriteValueAsJson(obj, typeof(T), obj?.GetType(), tokenWriter);
        }

        public static void ToJson(JsonValue val, JsonTokenWriter tokenWriter) {
            WriteJsonValueAsJson(val, tokenWriter);
        }
        #endregion
        
        #region Reading
        public static T FromJson<T>(string jsonString) {
            return (T) FromJson(new JsonTokenReader(jsonString), typeof(T));
        }

        public static JsonValue FromJson(string jsonString) {
            return FromJson(new JsonTokenReader(jsonString));
        }

        public static T FromJson<T>(JsonTokenReader tokenReader) {
            var result = (T)FromJson(tokenReader, typeof(T));
            
            // Make sure there's no additional json in the buffer.
            if (tokenReader.NextToken != JsonToken.EOF) {
                throw new InvalidJsonException($"{tokenReader.LineColString} Expected end of file");
            }
            return result;
        }

        public static JsonValue FromJson(JsonTokenReader tokenReader) {
            var result = ReadJsonValue(tokenReader);

            // Make sure there's no additional json in the buffer.
            if (tokenReader.NextToken != JsonToken.EOF) {
                throw new InvalidJsonException($"{tokenReader.LineColString} Expected end of file");
            }
            return result;
        }
        #endregion
    }
}
