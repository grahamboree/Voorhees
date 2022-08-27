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
            // Because of C#'s function overloading rules, we need to explicitly specify the function body here even though it's identical to above.
            // This forces it to use the ToJson overload that takes a JsonValue.  ToJson<T>(T, bool) will never call ToJson(JsonValue, JsonTokenWriter)
            // because the type of the caller is unknown at compile-time when overload resolution happens.
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder)) {
                var jsonWriter = new JsonTokenWriter(stringWriter, prettyPrint);
                ToJson(val, jsonWriter);
            }
            return stringBuilder.ToString();
        }

        public static void ToJson<T>(T obj, JsonTokenWriter tokenWriter) {
            WriteValue(obj, typeof(T), obj?.GetType(), tokenWriter);
        }

        public static void ToJson(JsonValue val, JsonTokenWriter tokenWriter) {
            WriteJsonValue(val, tokenWriter);
        }
        #endregion
        
        #region Reading
        public static T FromJson<T>(string jsonString) {
            return FromJson<T>(new JsonTokenReader(jsonString));
        }

        public static T FromJson<T>(JsonTokenReader tokenReader) {
            var result = (T)FromJson(tokenReader, typeof(T));
            
            // Make sure there's no additional json in the buffer.
            if (tokenReader.NextToken != JsonToken.EOF) {
                throw new InvalidJsonException($"{tokenReader.LineColString} Expected end of file");
            }
            return result;
        }

        public static JsonValue FromJson(string jsonString) {
            return FromJson(new JsonTokenReader(jsonString));
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
