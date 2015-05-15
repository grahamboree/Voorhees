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

class JsonValue : Dictionary<string, JsonValue>, IList<JsonValue>, IEquatable<JsonValue> {
	IList<JsonValue> arrayValue;
	JsonType type = JsonType.None;
	

	#region IEnumerable
	public new IEnumerator<JsonValue> GetEnumerator() { return arrayValue.GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	#endregion

	#region ICollection<JsonValue>
	public void Add(JsonValue item) {
		arrayValue.Add(item);
	}

	public new void Clear() {
		base.Clear();
		arrayValue.Clear();
	}

	public bool Contains(JsonValue item) {
		return arrayValue.Contains(item);
	}

	public void CopyTo(JsonValue[] array, int arrayIndex) {
		arrayValue.CopyTo(array, arrayIndex);
	}

	public bool Remove(JsonValue item) {
		return arrayValue.Remove(item);
	}

	public new int Count {
		get {
			if (type == JsonType.Object) {
				return base.Count;
			}
			// TODO return base.Count if this is a dictionary.
			return EnsureList().Count;
		}
	}

	public bool IsReadOnly {
		get { return arrayValue.IsReadOnly; }
	}
	#endregion

	#region IList<JsonValue>
	public int IndexOf(JsonValue item) {
		return arrayValue.IndexOf(item);
	}

	public void Insert(int index, JsonValue item) {
		arrayValue.Insert(index, item);
	}

	public void RemoveAt(int index) {
		arrayValue.RemoveAt(index);
	}

	public JsonValue this[int index] {
		get { return arrayValue[index]; }
		set { arrayValue[index] = value; }
	}
	#endregion

	#region IEquatable<JsonValue>
	public bool Equals(JsonValue other) {
		return false;
	}
	#endregion

	#region Private methods.
	private IDictionary<string, JsonValue> EnsureDictionary() {
		if (type == JsonType.Object) {
			return inst_object;
		}

		if (type != JsonType.None) {
			throw new InvalidOperationException("Instance of JsonValue is not a dictionary");
		}

		type = JsonType.Object;

		return inst_object;
	}

	private IList<JsonValue> EnsureList() {
		if (type == JsonType.Array) {
			return (IList<JsonData>)arrayValue;
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
