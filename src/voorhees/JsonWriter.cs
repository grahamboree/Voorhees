namespace Voorhees {
    public static class JsonWriter {
        public static string ToJson(JsonValue json) {
            var os = JsonConfig.CurrentConfig.PrettyPrint ? new PrettyPrintJsonOutputStream()
                : new JsonOutputStream();
            os.Write(json);
            return os.ToString();
        }
    }
}
