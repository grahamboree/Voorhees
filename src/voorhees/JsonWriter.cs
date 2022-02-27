namespace Voorhees {
    public static class JsonWriter {
        public static string ToJson(JsonValue json, bool prettyPrint = false) {
            var os = new JsonOutputStream(prettyPrint);
            os.Write(json);
            return os.ToString();
        }
    }
}
