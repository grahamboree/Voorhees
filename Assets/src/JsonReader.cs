using System;
using System.Collections.Generic;

public class InvalidJsonException : System.Exception {
	public InvalidJsonException(string message) : base(message) {
	}
}

public class JsonReader {
	public static JsonValue Read(string json) {
		JsonValue result = new JsonValue();
		Stack<JsonValue> valueStack = new Stack<JsonValue>();

		bool expectingComma = false;

		Action<JsonValue> addToStack = (val) => {
			if (valueStack.Count > 0) {
				valueStack.Peek().Add(val);
				expectingComma = true;
			} else {
				result = val;
			}
		};

		for (int i = 0; i < json.Length; ++i) {
			while (i < json.Length && Char.IsWhiteSpace(json[i])) {
				i++;
			}
			if (expectingComma) {
				if (json[i] == ',') {
					expectingComma = false;
				} else if (json[i] == ']') {
					expectingComma = false;
					addToStack(valueStack.Pop());
				} else {
					throw new InvalidJsonException("Expected comma, but found none at column " + i + "!");
				}
			} else if (json[i] == '[') { // start array
				var array = new JsonValue();
				array.Type = JsonType.Array;
				valueStack.Push(array);
			} else if (json[i] == ']') { // end array
				addToStack(valueStack.Pop());
			} else if (json[i] == '"') { // string
				i++;
				int startIndex = i;
				while (!(json[i] == '"' && (json[i - 1] != '\\' || i >= 2 || json[i - 2] == '\\'))) {
					i++;
				}
				addToStack(new JsonValue(json.Substring(startIndex, i - startIndex)));
			} else if (json[i] == '-' || (json[i] <= '9' && json[i] >= '0')) { // number
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
					throw new InvalidJsonException("string '" + numberString + "' is not a number");
				}
				--i; // Because we read past the end of the number.
				addToStack(value);
			} else if (json.Length - i >= 4 && json.Substring(i, 4) == "true") { // boolean
				i += 3;
				addToStack(new JsonValue(true));
			} else if (json.Length - i >= 5 && json.Substring(i, 5) == "false") { // boolean
				i += 4;
				addToStack(new JsonValue(false));
			} else if (json.Length - i >= 4 && json.Substring(i, 4) == "null") { // null
				i += 3;
				addToStack(new JsonValue());
			}
		}
		return result;
	}
}
