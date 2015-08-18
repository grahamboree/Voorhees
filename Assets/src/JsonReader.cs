
using System.Collections.Generic;

public class JsonReader {
	public static JsonValue Read(string json) {
		JsonValue result = new JsonValue();
		Stack<JsonValue> valueStack = new Stack<JsonValue>();

		for (int i = 0; i < json.Length; ++i) {
			if (json[i] == '[') {
				var array = new JsonValue();
				array.Type = JsonType.Array;
				valueStack.Push(array);
			} else if (json[i] == ']') {
				var array = valueStack.Pop();
				if (valueStack.Count > 0) {
					valueStack.Peek().Add(array);
				} else {
					result = array;
				}
			} else if (json[i] == '"') {
				// string
				i++;
				int startIndex = i;
				while (!(json[i] == '"' && (json[i - 1] != '\\' || i >= 2 || json[i - 2] == '\\'))) {
					i++;
				}
				var value = new JsonValue(json.Substring(startIndex, i - startIndex));
				if (valueStack.Count > 0) {
					valueStack.Peek().Add(value);
				} else {
					result = value;
				}
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
				JsonValue value;
				if (int.TryParse(numberString, out intVal)) {
					value = intVal;
				} else if (float.TryParse(numberString, out floatVal)) {
					value = floatVal;
				} else {
					throw new System.Exception("string '" + numberString + "' is not a number");
				}
				--i;

				if (valueStack.Count > 0) {
					valueStack.Peek().Add(value);
				} else {
					result = value;
				}
			} else if (json.Length - i >= 4 && json.Substring(i, 4) == "true") {
				var value = new JsonValue(true);
				if (valueStack.Count > 0) {
					valueStack.Peek().Add(value);
				} else {
					result = value;
				}
			} else if (json.Length - i >= 5 && json.Substring(i, 5) == "false") {
				var value = new JsonValue(false);
				if (valueStack.Count > 0) {
					valueStack.Peek().Add(value);
				} else {
					result = value;
				}
			} else if (json.Length - i >= 4 && json.Substring(i, 4) == "null") {
				var value = new JsonValue();
				if (valueStack.Count > 0) {
					valueStack.Peek().Add(value);
				} else {
					result = value;
				}
			}
		}
		return result;
	}
}
