/**
 * JsonData.cs
 *   Generic type to hold JSON data (objects, arrays, and so on). This is
 *   the default type returned by JsonMapper.ToObject().
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace LitJson {
	public class JsonData : IJsonWrapper, IEquatable<JsonData> {
		#region Fields
		private IList<JsonData> arrayValue;
		private IDictionary<string, JsonData> inst_object;

		private bool booleanValue;
		private float floatValue;
		private int intValue;
		private string stringValue;
		private JsonType type;

		private string cachedJsonString;

		// Used to implement the IOrderedDictionary interface
		private IList<KeyValuePair<string, JsonData>> object_list;
		#endregion

		#region Properties
		public int Count { get { return EnsureCollection().Count; } }
		public ICollection<string> Keys { get { EnsureDictionary(); return inst_object.Keys; } }
		#endregion

		#region ICollection Properties
		//int ICollection.Count { get { return Count; } }
		//bool ICollection.IsSynchronized { get { return EnsureCollection().IsSynchronized; } }
		//object ICollection.SyncRoot { get { return EnsureCollection().SyncRoot; } }
		#endregion

		#region IDictionary Properties
		//bool IDictionary.IsFixedSize { get { return EnsureDictionary().IsFixedSize; } }
		//bool IDictionary.IsReadOnly { get { return EnsureDictionary().IsReadOnly; } }

		/*ICollection IDictionary.Keys {
			get {
				EnsureDictionary();
				IList<string> keys = new List<string>();

				foreach (KeyValuePair<string, JsonData> entry in
						 object_list) {
					keys.Add(entry.Key);
				}

				return (ICollection)keys;
			}
		}

		ICollection IDictionary.Values {
			get {
				EnsureDictionary();
				IList<JsonData> values = new List<JsonData>();

				foreach (KeyValuePair<string, JsonData> entry in
						 object_list) {
					values.Add(entry.Value);
				}

				return (ICollection)values;
			}
		}*/
		#endregion

		#region IJsonWrapper Properties
		/*
		bool IJsonWrapper.IsArray { get { return IsArray; } }
		bool IJsonWrapper.IsBoolean { get { return IsBoolean; } }
		bool IJsonWrapper.IsFloat { get { return IsFloat; } }
		bool IJsonWrapper.IsInt { get { return IsInt; } }
		bool IJsonWrapper.IsObject { get { return IsObject; } }
		bool IJsonWrapper.IsString { get { return IsString; } }
		*/
		#endregion

		#region IDictionary Indexer
		/*object IDictionary.this[object key] {
			get {
				return EnsureDictionary()[key];
			}
			set {
				if (!(key is String))
					throw new ArgumentException("The key has to be a string");

				JsonData data = ToJsonData(value);
				this[(string)key] = data;
			}
		}*/
		#endregion

		#region IOrderedDictionary Indexer
		/*object IOrderedDictionary.this[int idx] {
			get {
				EnsureDictionary();
				return object_list[idx].Value;
			}
			set {
				EnsureDictionary();
				JsonData data = ToJsonData(value);

				KeyValuePair<string, JsonData> old_entry = object_list[idx];

				inst_object[old_entry.Key] = data;

				KeyValuePair<string, JsonData> entry = new KeyValuePair<string, JsonData>(old_entry.Key, data);

				object_list[idx] = entry;
			}
		}*/
		#endregion

		#region IList Indexer
		/*
		object this[int index] {
			get {
				return EnsureList()[index];
			}
			set {
				EnsureList();
				JsonData data = ToJsonData(value);

				this[index] = data;
			}
		}
		*/
		#endregion

		#region Public Indexers
		public JsonData this[string prop_name] {
			get {
				EnsureDictionary();
				return inst_object[prop_name];
			}
			set {
				EnsureDictionary();

				KeyValuePair<string, JsonData> entry =
					new KeyValuePair<string, JsonData>(prop_name, value);

				if (inst_object.ContainsKey(prop_name)) {
					for (int i = 0; i < object_list.Count; i++) {
						if (object_list[i].Key == prop_name) {
							object_list[i] = entry;
							break;
						}
					}
				} else {
					object_list.Add(entry);
				}

				inst_object[prop_name] = value;

				cachedJsonString = null;
			}
		}

		public JsonData this[int index] {
			get {
				EnsureCollection();

				if (type == JsonType.Array) {
					return arrayValue[index];
				}

				return object_list[index].Value;
			}
			set {
				EnsureCollection();

				if (type == JsonType.Array) {
					arrayValue[index] = value;
				} else {
					KeyValuePair<string, JsonData> entry = object_list[index];
					KeyValuePair<string, JsonData> new_entry = new KeyValuePair<string, JsonData>(entry.Key, value);

					object_list[index] = new_entry;
					inst_object[entry.Key] = value;
				}

				cachedJsonString = null;
			}
		}
		#endregion

		#region Constructors
		public JsonData() { }

		public JsonData(bool boolean) {
			type = JsonType.Boolean;
			booleanValue = boolean;
		}

		public JsonData(float number) {
			type = JsonType.Float;
			floatValue = number;
		}

		public JsonData(int number) {
			type = JsonType.Int;
			intValue = number;
		}

		/*
		public JsonData(object obj) {
			if (obj is Boolean) {
				type = JsonType.Boolean;
				booleanValue = (bool)obj;
				return;
			}

			if (obj is loat) {
				type = JsonType.Double;
				floatValue = (double)obj;
				return;
			}

			if (obj is Int32) {
				type = JsonType.Int;
				intValue = (int)obj;
				return;
			}

			if (obj is Int64) {
				type = JsonType.Long;
				longValue = (long)obj;
				return;
			}

			if (obj is String) {
				type = JsonType.String;
				stringValue = (string)obj;
				return;
			}

			throw new ArgumentException("Unable to wrap the given object with JsonData");
		}
		*/

		public JsonData(string str) {
			type = JsonType.String;
			stringValue = str;
		}
		#endregion

		#region Implicit Conversions
		public static implicit operator JsonData(Boolean data) { return new JsonData(data); }
		public static implicit operator JsonData(float data) { return new JsonData(data); }
		public static implicit operator JsonData(Int32 data) { return new JsonData(data); }
		public static implicit operator JsonData(Int64 data) { return new JsonData(data); }
		public static implicit operator JsonData(String data) { return new JsonData(data); }
		#endregion

		#region Explicit Conversions
		public static explicit operator bool(JsonData data) {
			if (!data.IsBoolean) {
				throw new InvalidCastException("Instance of JsonData doesn't hold a boolean");
			}

			return data.booleanValue;
		}

		public static explicit operator float(JsonData data) {
			if (data.type != JsonType.Float) {
				throw new InvalidCastException("Instance of JsonData doesn't hold a float");
			}

			return data.floatValue;
		}

		public static explicit operator int(JsonData data) {
			if (data.type != JsonType.Int) {
				throw new InvalidCastException("Instance of JsonData doesn't hold an int");
			}

			return data.intValue;
		}

		public static explicit operator String(JsonData data) {
			if (data.type != JsonType.String) {
				throw new InvalidCastException("Instance of JsonData doesn't hold a string");
			}

			return data.stringValue;
		}
		#endregion

		#region IDictionary Methods
		/*void IDictionary<string, JsonData>.Add(string key, JsonData data) {
			EnsureDictionary().Add(key, data);

			var entry = new KeyValuePair<string, JsonData>(key, data);
			object_list.Add(entry);

			cachedJsonString = null;
		}*/

		/*
		void IDictionary.Clear() {
			EnsureDictionary().Clear();
			object_list.Clear();
			cachedJsonString = null;
		}

		bool IDictionary.Contains(object key) { return EnsureDictionary().Contains(key); }

		IDictionaryEnumerator IDictionary.GetEnumerator() { return ((IOrderedDictionary)this).GetEnumerator(); }

		void IDictionary.Remove(object key) {
			EnsureDictionary().Remove(key);

			for (int i = 0; i < object_list.Count; i++) {
				if (object_list[i].Key == (string)key) {
					object_list.RemoveAt(i);
					break;
				}
			}

			cachedJsonString = null;
		}*/
		#endregion

		#region IEnumerable Methods
		IEnumerator<JsonData> IEnumerable<JsonData>.GetEnumerator() { return EnsureCollection().GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return EnsureCollection().GetEnumerator(); }
		#endregion

		#region IJsonWrapper Methods
		public bool IsArray { get { return type == JsonType.Array; } }
		public bool IsBoolean { get { return type == JsonType.Boolean; } }
		public bool IsFloat { get { return type == JsonType.Float; } }
		public bool IsInt { get { return type == JsonType.Int; } }
		public bool IsObject { get { return type == JsonType.Object; } }
		public bool IsString { get { return type == JsonType.String; } }

		public JsonType Type {
			get {
				return type;
			}
			set {
				type = value;
			}
		}
		public bool BooleanValue {
			get {
				if (type != JsonType.Boolean) {
					throw new InvalidOperationException("JsonData instance doesn't hold a boolean");
				}
				return booleanValue;
			}
			set {
				type = JsonType.Boolean;
				booleanValue = value;
				cachedJsonString = null;
			}
		}
		public float FloatValue {
			get {
				if (type != JsonType.Float) {
					throw new InvalidOperationException("JsonData instance doesn't hold a double");
				}
				return floatValue;
			}
			set {
				type = JsonType.Float;
				floatValue = value;
				cachedJsonString = null;
			}
		}
		public int IntValue {
			get {
				if (type != JsonType.Int) {
					throw new InvalidOperationException("JsonData instance doesn't hold an int");
				}
				return intValue;
			}
			set {
				type = JsonType.Int;
				intValue = value;
				cachedJsonString = null;
			}
		}
		public string StringValue {
			get {
				if (type != JsonType.String) {
					throw new InvalidOperationException("JsonData instance doesn't hold a string");
				}
				return stringValue;
			}
			set {
				type = JsonType.String;
				stringValue = value;
				cachedJsonString = null;
			}
		}

		string IJsonWrapper.ToJson() { return ToJson(); }
		void IJsonWrapper.ToJson(JsonWriter writer) { ToJson(writer); }
		#endregion

		#region IList Methods
		bool ICollection<JsonData>.IsReadOnly { get { return EnsureList().IsReadOnly; } }

		void ICollection<JsonData>.Add(JsonData value) {
			Add(value);
		}

		bool ICollection<JsonData>.Contains(JsonData value) {
			return EnsureList().Contains(value);
		}

		int IList<JsonData>.IndexOf(JsonData value) {
			return EnsureList().IndexOf(value);
		}

		void IList<JsonData>.Insert(int index, JsonData value) {
			EnsureList().Insert(index, value);
			cachedJsonString = null;
		}

		bool ICollection<JsonData>.Remove(JsonData value) {
			bool retval = EnsureList().Remove(value);
			cachedJsonString = null;
			return retval;
		}

		void IList<JsonData>.RemoveAt(int index) {
			EnsureList().RemoveAt(index);
			cachedJsonString = null;
		}

		// ICollection<T>
		public void Clear() {
			if (IsObject) {
				((IDictionary)this).Clear();
				return;
			}
			
			if (IsArray) {
				((IList)this).Clear();
				return;
			}
		}

		void ICollection<JsonData>.CopyTo(JsonData[] array, int index) {
			EnsureList().CopyTo(array, index);
		}
		#endregion

		#region IOrderedDictionary Methods
		/*IDictionaryEnumerator IOrderedDictionary.GetEnumerator() {
			EnsureDictionary();

			return new OrderedDictionaryEnumerator(object_list.GetEnumerator());
		}

		void IOrderedDictionary.Insert(int idx, object key, object value) {
			string property = (string)key;
			JsonData data = ToJsonData(value);

			this[property] = data;

			KeyValuePair<string, JsonData> entry = new KeyValuePair<string, JsonData>(property, data);

			object_list.Insert(idx, entry);
		}

		void IOrderedDictionary.RemoveAt(int idx) {
			EnsureDictionary();

			inst_object.Remove(object_list[idx].Key);
			object_list.RemoveAt(idx);
		}*/
		#endregion

		#region Private Methods
		private ICollection<JsonData> EnsureCollection() {
			if (type == JsonType.Array) {
				return (ICollection<JsonData>)arrayValue;
			}

			if (type == JsonType.Object) {
				return (ICollection<JsonData>)inst_object;
			}

			throw new InvalidOperationException("The JsonData instance has to be initialized first");
		}

		private IDictionary EnsureDictionary() {
			if (type == JsonType.Object) {
				return (IDictionary)inst_object;
			}

			if (type != JsonType.None) {
				throw new InvalidOperationException("Instance of JsonData is not a dictionary");
			}

			type = JsonType.Object;
			inst_object = new Dictionary<string, JsonData>();
			object_list = new List<KeyValuePair<string, JsonData>>();

			return (IDictionary)inst_object;
		}

		private IList<JsonData> EnsureList() {
			if (type == JsonType.Array) {
				return (IList<JsonData>)arrayValue;
			}

			if (type != JsonType.None) {
				throw new InvalidOperationException("Instance of JsonData is not a list");
			}

			type = JsonType.Array;
			arrayValue = new List<JsonData>();

			return (IList<JsonData>)arrayValue;
		}

		private JsonData ToJsonData(object obj) {
			if (obj == null) {
				return null;
			}

			if (obj is JsonData) {
				return (JsonData)obj;
			}

			// TODO
			//return new JsonData(obj);
			return null;
		}

		private static void WriteJson(IJsonWrapper obj, JsonWriter writer) {
			if (obj == null) {
				writer.Write(null);
				return;
			}

			if (obj.IsString) {
				writer.Write(obj.StringValue);
				return;
			}

			if (obj.IsBoolean) {
				writer.Write(obj.BooleanValue);
				return;
			}

			if (obj.IsFloat) {
				writer.Write(obj.FloatValue);
				return;
			}

			if (obj.IsInt) {
				writer.Write(obj.IntValue);
				return;
			}

			if (obj.IsArray) {
				writer.WriteArrayStart();
				foreach (object elem in (IList) obj) {
					WriteJson((JsonData)elem, writer);
				}
				writer.WriteArrayEnd();

				return;
			}

			if (obj.IsObject) {
				writer.WriteObjectStart();

				foreach (DictionaryEntry entry in ((IDictionary) obj)) {
					writer.WritePropertyName((string)entry.Key);
					WriteJson((JsonData)entry.Value, writer);
				}
				writer.WriteObjectEnd();

				return;
			}
		}
		#endregion

		public void Add(object value) {
			JsonData data = ToJsonData(value);
			cachedJsonString = null;
			EnsureList().Add(data);
		}

		public bool Equals(JsonData x) {
			if (x == null) {
				return false;
			}

			if (x.type != this.type) {
				return false;
			}

			switch (this.type) {
			case JsonType.None:
				return true;

			case JsonType.Object:
				return this.inst_object.Equals(x.inst_object);

			case JsonType.Array:
				return this.arrayValue.Equals(x.arrayValue);

			case JsonType.String:
				return this.stringValue.Equals(x.stringValue);

			case JsonType.Int:
				return this.intValue.Equals(x.intValue);

			case JsonType.Float:
				return this.floatValue.Equals(x.floatValue);

			case JsonType.Boolean:
				return this.booleanValue.Equals(x.booleanValue);
			}

			return false;
		}

		public JsonType GetJsonType() {
			return type;
		}

		public void SetJsonType(JsonType type) {
			if (this.type == type)
				return;

			switch (type) {
			case JsonType.None:
				break;

			case JsonType.Object:
				inst_object = new Dictionary<string, JsonData>();
				object_list = new List<KeyValuePair<string, JsonData>>();
				break;

			case JsonType.Array:
				arrayValue = new List<JsonData>();
				break;

			case JsonType.String:
				stringValue = default (String);
				break;

			case JsonType.Int:
				intValue = default (Int32);
				break;

			case JsonType.Float:
				floatValue = default (float);
				break;

			case JsonType.Boolean:
				booleanValue = default (Boolean);
				break;
			}

			this.type = type;
		}

		public string ToJson() {
			if (cachedJsonString != null)
				return cachedJsonString;

			StringWriter sw = new StringWriter();
			JsonWriter writer = new JsonWriter(sw);
			writer.Validate = false;

			WriteJson(this, writer);
			cachedJsonString = sw.ToString();

			return cachedJsonString;
		}

		public void ToJson(JsonWriter writer) {
			bool old_validate = writer.Validate;

			writer.Validate = false;

			WriteJson(this, writer);

			writer.Validate = old_validate;
		}

		public override string ToString() {
			switch (type) {
			case JsonType.Array:
				return "JsonData array";

			case JsonType.Boolean:
				return booleanValue.ToString();

			case JsonType.Float:
				return floatValue.ToString();

			case JsonType.Int:
				return intValue.ToString();

			case JsonType.Object:
				return "JsonData object";

			case JsonType.String:
				return stringValue;
			}

			return "Uninitialized JsonData";
		}
	}

	/*
	internal class OrderedDictionaryEnumerator : IDictionaryEnumerator {
		IEnumerator<KeyValuePair<string, JsonData>> list_enumerator;

		public object Current { get { return Entry; } }

		public DictionaryEntry Entry {
			get {
				KeyValuePair<string, JsonData> curr = list_enumerator.Current;
				return new DictionaryEntry(curr.Key, curr.Value);
			}
		}

		public object Key { get { return list_enumerator.Current.Key; } }
		public object Value { get { return list_enumerator.Current.Value; } }

		public OrderedDictionaryEnumerator(IEnumerator<KeyValuePair<string, JsonData>> enumerator) {
			list_enumerator = enumerator;
		}

		public bool MoveNext() { return list_enumerator.MoveNext(); }
		public void Reset() { list_enumerator.Reset(); }
	}*/
}
