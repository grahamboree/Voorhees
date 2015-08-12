
using System.Collections.Generic;

public class JsonReader {
	public static JsonValue Read(string json) {
		JsonValue result = new JsonValue();
		//Stack<JsonValue> valueStack = new Stack<JsonValue>();

		for (int i = 0; i < json.Length; ++i) {
			if (json[i] == '"') {
				i++;
				int startIndex = i;
				while (!(json[i] == '"' && (json[i - 1] != '\\' || i >= 2 || json[i - 2] == '\\'))) {
					i++;
				}
				return json.Substring(startIndex, i - startIndex);
			} else if (json[i] == '-' || (json[i] <= '9' && json[i] >= '0')) {
				int startIndex = i;
				i++;
				while (i < json.Length && 
				       ((json[i] <= '9' && json[i] >= '0') ||
						 json[i] == '.' ||
						 json[i] == 'e' ||
						 json[i] == 'E' ||
						 json[i] == '-' ||
						 json[i] == '+'))
				{
					i++;
				}
				// number
				string numberString = json.Substring(startIndex, i - startIndex);
				int intVal;
				float floatVal;
				if (int.TryParse(numberString, out intVal)) {
					return intVal;
				} else if (float.TryParse(numberString, out floatVal)) {
					return floatVal;
				} else {
					throw new System.Exception("numer is not a number");
				}
			}
		}
		return result;
	}
}
