using System.Linq;
using System.Text;

public class JsonWriter {
	public static string ToJson(JsonValue json) {
		StringBuilder result = new StringBuilder();
		WriteValue(result, json);
		return result.ToString();
	}

	static void WriteValue(StringBuilder result, JsonValue value) {
		switch (value.Type) {
			case JsonType.Array:
				result.Append('[');
				for (int i = 0; i < value.Count; ++i) {
					if (i != 0) {
						result.Append(", ");
					}
					WriteValue(result, value[i]);
				}
				result.Append(']');
				break;
			case JsonType.Object:
				result.Append('{');

				// TODO remove this copy for performance.
				var keys = value.Keys.ToList();

				for (int i = 0; i < keys.Count; ++i) {
					if (i != 0) {
						result.Append(", ");
					}
					result.Append('\"');
					result.Append(keys[i]);
					result.Append('\"');
					result.Append(": ");
					WriteValue(result, value[keys[i]]);
				}
				result.Append('}');
				break;
			case JsonType.Float: result.Append((float)value); break;
			case JsonType.Int: result.Append((int)value); break;
			case JsonType.Boolean: result.Append((bool)value ? "true" : "false"); break;
			case JsonType.Null: result.Append("null"); break;
			case JsonType.String:
				result.Append("\"");
				result.Append((string)value);
				result.Append("\""); break;
		}
	}
}
