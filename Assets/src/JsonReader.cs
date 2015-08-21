using System;
using System.Text;

public class InvalidJsonException : Exception {
	public InvalidJsonException(string message) : base(message) {
	}
}

public class JsonReader {
	public static JsonValue Read(string json) {
		JsonValue result = null;
		try {
			// Read the json.
			int i = 0;
			result = ReadValue(json, ref i);

			// Make sure there's no additional json in the buffer.
			SkipWhitespace(json, ref i);
			if (i <= json.Length - 1) {
				throw new InvalidJsonException("Expected end of file at column " + i + "!");
			}

		} catch (IndexOutOfRangeException) {
			throw new InvalidJsonException("Unexpected end of file!");
		}
		return result;
    }

	static JsonValue ReadNumber(string json, ref int i) {
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
		if (int.TryParse(numberString, out intVal)) {
			return intVal;
		}

		float floatVal;
		if (float.TryParse(numberString, out floatVal)) {
			return floatVal;
		} 

		throw new InvalidJsonException("string '" + numberString + "' is not a number");
	}

	static JsonValue ReadString(string json, ref int i) {
		i++; // Skip the '"'
		StringBuilder stringData = new StringBuilder();
		bool backslash = false;
		for (bool done = false; !done; ++i) {
			if (backslash) {
				backslash = false;
				switch (json[i]) {
				case '\\': stringData.Append('\\'); break;
				case '"': stringData.Append('"'); break;
				case '/': stringData.Append('/'); break;
				case 'b': stringData.Append('\b'); break;
				case 'f': stringData.Append('\f'); break;
				case 'n': stringData.Append('\n'); break;
				case 'r': stringData.Append('\r'); break;
				case 't': stringData.Append('\t'); break;
				case 'u':
						// Read 4 ints
						Int16 codePoint = Convert.ToInt16(json.Substring(i + 1, 4), 16);
						i += 4;
						stringData.Append(char.ConvertFromUtf32(codePoint));
					break;
				default:
					throw new InvalidJsonException(
						"Unknown escape character sequence: \\" + json[i] + " at column " + i + "!");
				}
			} else {
				switch (json[i]) {
				case '\\':
					backslash = true;
					break;
				case '"':
					done = true;
					break;
				default:
					stringData.Append(json[i]);
					break;
				}
			}
			
		}
		return new JsonValue(stringData.ToString());
	}

	static JsonValue ReadArray(string json, ref int i) {
		i++; // Skip the '['
		SkipWhitespace(json, ref i);

		JsonValue arrayval = new JsonValue();
		arrayval.Type = JsonType.Array;

		bool expectingValue = false;
		while (json[i] != ']') {
			expectingValue = false;
			arrayval.Add(ReadValue(json, ref i));
			SkipWhitespace(json, ref i);
			if (json[i] == ',') {
				expectingValue = true;
				i++;
				SkipWhitespace(json, ref i);
			} else if (json[i] != ']') {
				throw new InvalidJsonException("Expected end array token at column " + i + "!");
			}
		}

		if (expectingValue) {
			throw new InvalidJsonException("Unexpected end array token at column " + i + "!");
		}

		i++; // Skip the ']'
		return arrayval;
	}

	static JsonValue ReadValue(string json, ref int i) {
		SkipWhitespace(json, ref i);
		if (json[i] == '[') { // array
			return ReadArray(json, ref i);
		} else if (json[i] == '{') { // object
			return ReadObject(json, ref i);
		} else if (json[i] == '"') { // string
			return ReadString(json, ref i);
		} else if (json[i] == '-' || (json[i] <= '9' && json[i] >= '0')) { // number
			return ReadNumber(json, ref i);
		} else if (json.Length - i >= 4 && json.Substring(i, 4) == "true") { // boolean
			i += 4;
			return true;
		} else if (json.Length - i >= 5 && json.Substring(i, 5) == "false") { // boolean
			i += 5;
			return false;
		} else if (json.Length - i >= 4 && json.Substring(i, 4) == "null") { // null
			i += 4;
			return new JsonValue();
		}
		throw new InvalidJsonException("Unexpected character '" + json[i] + "' at column " + i + "!");
	}

	static JsonValue ReadObject(string json, ref int i) {
		var obj = new JsonValue();
		obj.Type = JsonType.Object;
		i++; // Skip the '{'

		SkipWhitespace(json, ref i);
		if (json[i] != '}') {
			//ReadElements(obj, json, ref i);
			while (true) {
				SkipWhitespace(json, ref i);
				
				var key = ReadString(json, ref i);
				SkipWhitespace(json, ref i);
				if (json[i] != ':') {
					throw new InvalidJsonException("Expected ':' at column " + i + "!");
				}
				++i; // Skip the ':'
				SkipWhitespace(json, ref i);
				var value = ReadValue(json, ref i);
				obj.Add((string)key, value);
				
				SkipWhitespace(json, ref i);
				
				if (json[i] == ',') {
					i++; // Skip the ','
				} else {
					break;
				}
			}
		}
		if (json[i] != '}') {
			throw new InvalidJsonException("Expected closing object token at column " + i + "!");
		}

		i++; // Skip the '}'

		return obj;
	}

	static void SkipWhitespace(string json, ref int i) {
		while (i < json.Length && Char.IsWhiteSpace(json[i])) {
			i++;
		}
	}
}
