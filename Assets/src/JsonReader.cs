
public class JsonReader {
	public static JsonValue Read(string json) {
		JsonValue value = new JsonValue();
		value = int.Parse(json);
		return value;
	}
}
