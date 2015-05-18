using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum JsonType {
	None,

	Object,
	Array,

	String,
	Boolean,
	Int,
	Float,
}

class JsonValue : IDictionary<string, JsonValue>, IList<JsonValue>, IEquatable<JsonValue> {
	JsonType type = JsonType.None;

	List<JsonValue> arrayValue;
	String stringValue;
	Dictionary<string, JsonValue> objectValue;
	bool boolValue;
	float floatValue;
	int intValue;
	
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
