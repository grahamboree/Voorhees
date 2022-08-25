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

        public static T FromJson<T>(JsonTokenizer tokenizer) {
            return (T) FromJson(tokenizer, typeof(T));
        }
        #endregion
    }
}
