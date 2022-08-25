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

        public static void ToJson<T>(T obj, JsonWriter writer) {
            WriteValueAsJson(obj, typeof(T), obj?.GetType(), writer);
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
