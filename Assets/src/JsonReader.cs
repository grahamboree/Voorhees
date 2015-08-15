
using System.Collections.Generic;

public class JsonReader {
	public static JsonValue Read(string json) {
		JsonValue result = new JsonValue();
		//Stack<JsonValue> valueStack = new Stack<JsonValue>();

		for (int i = 0; i < json.Length; ++i) {
			if (json[i] == '"') {
				// string
				i++;
				int startIndex = i;
				while (!(json[i] == '"' && (json[i - 1] != '\\' || i >= 2 || json[i - 2] == '\\'))) {
					i++;
				}
				return json.Substring(startIndex, i - startIndex);
			} else if (json[i] == '-' || (json[i] <= '9' && json[i] >= '0')) {
				// number
				int startIndex = i;
				i++;
				while (i < json.Length && 
				       ((json[i] >= '0' && json[i] <= '9') ||
						 json[i] == '.' || json[i] == 'e' ||
						 json[i] == 'E' || json[i] == '-' ||
						 json[i] == '+'))
				{
					i++;
				}
				string numberString = json.Substring(startIndex, i - startIndex);
				int intVal;
				float floatVal;
				if (int.TryParse(numberString, out intVal)) {
					return intVal;
				} else if (float.TryParse(numberString, out floatVal)) {
					return floatVal;
				} else {
					throw new System.Exception("string '" + numberString + "' is not a number");
				}
			} else if (json.Length - i >= 4 && json.Substring(i, 4) == "true") {
				return new JsonValue(true);
			} else if (json.Length - i >= 5 && json.Substring(i, 5) == "false") {
				return new JsonValue(false);
			} else if (json.Length - i >= 4 && json.Substring(i, 4) == "null") {
				return new JsonValue();
			}
		}
		return result;
	}
}
