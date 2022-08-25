using System.IO;
using System.Text;

namespace Voorhees {
    public static partial class JsonMapper {
        #region Writing
        public static string ToJson<T>(T obj, bool prettyPrint = false) {
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder)) {
                var jsonWriter = new JsonWriter(stringWriter, prettyPrint);
                ToJson(obj, jsonWriter);
            }
            return stringBuilder.ToString();
        }

        public static string ToJson(JsonValue val, bool prettyPrint = false) {
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder)) {
                var jsonWriter = new JsonWriter(stringWriter, prettyPrint);
                ToJson(val, jsonWriter);
            }
            return stringBuilder.ToString();
        }

        public static void ToJson<T>(T obj, JsonWriter writer) {
            WriteValueAsJson(obj, typeof(T), obj?.GetType(), writer);
        }

        public static void ToJson(JsonValue val, JsonWriter writer) {
            WriteJsonValueAsJson(val, writer);
        }
        #endregion
        
        #region Reading
        public static T FromJson<T>(string jsonString) {
            return (T) FromJson(new JsonTokenizer(jsonString), typeof(T));
        }

        public static JsonValue FromJson(string jsonString) {
            return FromJson(new JsonTokenizer(jsonString));
        }

        public static T FromJson<T>(JsonTokenizer tokenizer) {
            var result = (T)FromJson(tokenizer, typeof(T));
            
            // Make sure there's no additional json in the buffer.
            if (tokenizer.NextToken != JsonToken.EOF) {
                throw new InvalidJsonException($"{tokenizer.LineColString} Expected end of file");
            }
            return result;
        }

        public static JsonValue FromJson(JsonTokenizer tokenizer) {
            var result = ReadJsonValue(tokenizer);

            // Make sure there's no additional json in the buffer.
            if (tokenizer.NextToken != JsonToken.EOF) {
                throw new InvalidJsonException($"{tokenizer.LineColString} Expected end of file");
            }
            return result;
        }
        #endregion
    }
}
