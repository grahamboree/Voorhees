using System;
using System.Collections;
using System.Collections.Generic;

public enum JsonType {
	None,

	Object,
	Array,

	String,
	Boolean,
	Int,
	Float,
}


public class JsonValue : IDictionary<string, JsonValue>, IList<JsonValue>, IEquatable<JsonValue> {
	#region Fields
	JsonType type = JsonType.None;

	List<JsonValue> arrayValue;
	Dictionary<string, JsonValue> objectValue;

	string stringValue;
	bool boolValue;
	float floatValue;
	int intValue;
	#endregion

	#region Type Properties
	public bool IsObject { get { return type == JsonType.Object; } }
	public bool IsArray { get { return type == JsonType.Array; } }
	public bool IsString { get { return type == JsonType.String; } }
	public bool IsBoolean { get { return type == JsonType.Boolean; } }
	public bool IsInt { get { return type == JsonType.Int; } }
	public bool IsFloat { get { return type == JsonType.Float; } }

	public JsonType Type {
		get {
			return type;
		}
		set {
			if (type != value) {
				type = value;

				objectValue = null;
				arrayValue = null;
				floatValue = 0;
				intValue = 0;
				boolValue = false;
				stringValue = "";
				switch (type) {
					case JsonType.Object:
						objectValue = new Dictionary<string, JsonValue>();
						break;
					case JsonType.Array:
						arrayValue = new List<JsonValue>();
						break;
				}
			}
		}
	}
	#endregion

	#region Constructors
	public JsonValue() { }

	public JsonValue(bool boolean) {
		type = JsonType.Boolean;
		boolValue = boolean;
	}

	public JsonValue(float number) {
		type = JsonType.Float;
		floatValue = number;
	}

	public JsonValue(int number) {
		type = JsonType.Int;
		intValue = number;
	}

	public JsonValue(string str) {
		type = JsonType.String;
		stringValue = str;
	}
	#endregion

	#region Implicit Conversions from other types to JsonValue
	public static implicit operator JsonValue(bool data) { return new JsonValue(data); }
	public static implicit operator JsonValue(float data) { return new JsonValue(data); }
	public static implicit operator JsonValue(int data) { return new JsonValue(data); }
	public static implicit operator JsonValue(string data) { return new JsonValue(data); }
	#endregion

	#region Explicit Conversions from JsonData to other types
	public static explicit operator bool (JsonValue data) {
		if (!data.IsBoolean) {
			throw new InvalidCastException("Instance of JsonData doesn't hold a boolean");
		}

		return data.boolValue;
	}

	public static explicit operator float (JsonValue data) {
		if (!data.IsFloat) {
			throw new InvalidCastException("Instance of JsonData doesn't hold a float");
		}

		return data.floatValue;
	}

	public static explicit operator int (JsonValue data) {
		if (!data.IsInt) {
			throw new InvalidCastException("Instance of JsonData doesn't hold an int");
		}

		return data.intValue;
	}

	public static explicit operator string (JsonValue data) {
		if (!data.IsString) {
			throw new InvalidCastException("Instance of JsonData doesn't hold a string");
		}

		return data.stringValue;
	}
	#endregion

	#region IEnumerable
	public IEnumerator<JsonValue> GetEnumerator() { return EnsureList().GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	IEnumerator<KeyValuePair<string, JsonValue>> IEnumerable<KeyValuePair<string, JsonValue>>.GetEnumerator() { return objectValue.GetEnumerator(); }
	#endregion

	#region ICollection<JsonValue>
	public void Add(JsonValue item) {
		EnsureList().Add(item);
	}

	public void Clear() {
		try {
			EnsureList().Clear();
		} catch (Exception) {
			EnsureObject().Clear();
		}
	}

	public bool Contains(JsonValue item) {
		return EnsureList().Contains(item);
	}

	public void CopyTo(JsonValue[] array, int arrayIndex) {
		EnsureList().CopyTo(array, arrayIndex);
	}

	public bool Remove(JsonValue item) {
		return EnsureList().Remove(item);
	}

	public int Count {
		get {
			try {
				return EnsureObject().Count;
			} catch (Exception) {
				return EnsureList().Count;
			}
		}
	}

	public bool IsReadOnly { get { return EnsureList().IsReadOnly; } }
	#endregion

	#region IList<JsonValue>
	public int IndexOf(JsonValue item) {
		return EnsureList().IndexOf(item);
	}

	public void Insert(int index, JsonValue item) {
		EnsureList().Insert(index, item);
	}

	public void RemoveAt(int index) {
		EnsureList().RemoveAt(index);
	}

	public JsonValue this[int index] {
		get { return EnsureList()[index]; }
		set {
			if (type != JsonType.Array) {
				type = JsonType.Array;
				arrayValue = new List<JsonValue>();
			}
			arrayValue[index] = value;
		}
	}
	#endregion

	#region IEquatable<JsonValue>
	public bool Equals(JsonValue other) {
		if (type != other.type) {
			return false;
		}
		switch (type) {
			case JsonType.None:
				return true;
			case JsonType.Object:
				return objectValue.Equals(other.objectValue);
			case JsonType.Array:
				return arrayValue.Equals(other.arrayValue);
			case JsonType.String:
				return stringValue.Equals(other.stringValue);
			case JsonType.Boolean:
				return boolValue.Equals(other.boolValue);
			case JsonType.Int:
				return intValue.Equals(other.intValue);
			case JsonType.Float:
				return floatValue.Equals(other.floatValue);
		}
		return false;
	}
	#endregion

	#region ICollection<KeyValuePair<string, JsonValue>>
	public bool Remove(KeyValuePair<string, JsonValue> value) { return EnsureObject().Remove(value); }
	public void CopyTo(KeyValuePair<string, JsonValue>[] array, int arrayIndex) { EnsureObject().CopyTo(array, arrayIndex); }
	public bool Contains(KeyValuePair<string, JsonValue> value) { return EnsureObject().Contains(value); }
	public void Add(KeyValuePair<string, JsonValue> value) { EnsureObject().Add(value); }
	#endregion

	#region IDictionary<string, JsonValue>
	public JsonValue this[string key] {
		get { return EnsureObject()[key]; }
		set { EnsureObject()[key] = value; }
	}
	public ICollection<string> Keys { get { return EnsureObject().Keys; } }
	public ICollection<JsonValue> Values { get { return EnsureObject().Values; } }
	public void Add(string key, JsonValue value) { EnsureObject().Add(key, value); }
	public bool TryGetValue(string key, out JsonValue value) { return EnsureObject().TryGetValue(key, out value); }
	public bool Remove(string key) { return EnsureObject().Remove(key); }
	public bool ContainsKey(string key) { return EnsureObject().ContainsKey(key); }
	#endregion

	#region Private methods.
	private IDictionary<string, JsonValue> EnsureObject() {
		if (type == JsonType.None) {
			type = JsonType.Object;
			objectValue = new Dictionary<string, JsonValue>();
		}

		if (type == JsonType.Object) {
			return objectValue;
		}

		throw new InvalidOperationException("Instance of JsonValue is not a dictionary");
	}

	private IList<JsonValue> EnsureList() {
		if (type == JsonType.Array) {
			return arrayValue;
		}

		if (type != JsonType.None) {
			throw new InvalidOperationException("Instance of JsonValue is not a list");
		}

		type = JsonType.Array;
		arrayValue = new List<JsonValue>();

		return arrayValue;
	}
	#endregion
}
