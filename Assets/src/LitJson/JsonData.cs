#if false

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;


namespace LitJson {
	/// Generic type to hold JSON data (objects, arrays, and so on). This is 
	/// the default type returned by JsonMapper.ToObject().
	public class JsonData : IJsonWrapper, IEquatable<JsonData>, IEnumerable<JsonData> {
		#region Fields
		private List<JsonData> arrayValue;
		private Dictionary<string, JsonData> inst_object;

		private bool booleanValue;
		private float floatValue;
		private int intValue;
		private string stringValue;
		private JsonType type = JsonType.None;

		private string cachedJsonString;
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

		public JsonData(string str) {
			type = JsonType.String;
			stringValue = str;
		}
		#endregion

		#region Implicit Conversions from other types to JsonData
		public static implicit operator JsonData(bool data) { return new JsonData(data); }
		public static implicit operator JsonData(float data) { return new JsonData(data); }
		public static implicit operator JsonData(int data) { return new JsonData(data); }
		public static implicit operator JsonData(string data) { return new JsonData(data); }
		#endregion

		#region Explicit Conversions from JsonData to other types
		public static explicit operator bool(JsonData data) {
			if (!data.IsBoolean) {
				throw new InvalidCastException("Instance of JsonData doesn't hold a boolean");
			}

			return data.booleanValue;
		}

		public static explicit operator float(JsonData data) {
			if (!data.IsFloat) {
				throw new InvalidCastException("Instance of JsonData doesn't hold a float");
			}

			return data.floatValue;
		}

		public static explicit operator int(JsonData data) {
			if (!data.IsInt) {
				throw new InvalidCastException("Instance of JsonData doesn't hold an int");
			}

			return data.intValue;
		}

		public static explicit operator string(JsonData data) {
			if (!data.IsString) {
				throw new InvalidCastException("Instance of JsonData doesn't hold a string");
			}

			return data.stringValue;
		}
		#endregion

		#region IDictionary
		public JsonData this[string prop_name] { get { return inst_object[prop_name]; } set { inst_object[prop_name] = value; }
		}
		public ICollection<string> Keys { get { return EnsureDictionary().Keys; } }

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

		#region IEnumerable
		IEnumerator<JsonData> IEnumerable<JsonData>.GetEnumerator() { return EnsureList().GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return EnsureCollection().GetEnumerator(); }
		#endregion

		#region IJsonWrapper Methods
		public bool IsArray { get { return type == JsonType.Array; } }
		public bool IsBoolean { get { return type == JsonType.Boolean; } }
		public bool IsFloat { get { return type == JsonType.Float; } }
		public bool IsInt { get { return type == JsonType.Int; } }
		public bool IsObject { get { return type == JsonType.Object; } }
		public bool IsString { get { return type == JsonType.String; } }

		public JsonType Type { get { return type; } set { type = value; } }

		public bool BooleanValue {
			get {
				return (bool)this;
			}
			set {
				type = JsonType.Boolean;
				booleanValue = value;
				cachedJsonString = null;
			}
		}
		public float FloatValue {
			get {
				return (float)this;
			}
			set {
				type = JsonType.Float;
				floatValue = value;
				cachedJsonString = null;
			}
		}
		public int IntValue {
			get {
				return (int)this;
			}
			set {
				type = JsonType.Int;
				intValue = value;
				cachedJsonString = null;
			}
		}
		public string StringValue {
			get {
				return (string)this;
			}
			set {
				type = JsonType.String;
				stringValue = value;
				cachedJsonString = null;
			}
		}
		#endregion

		#region IList
		public int Count { get { return EnsureList().Count; } }
		public bool IsReadOnly { get { return EnsureList().IsReadOnly; } }

		public void Add(JsonData value) {
			Add(value);
		}

		public bool Contains(JsonData value) {
			return EnsureList().Contains(value);
		}

		public int IndexOf(JsonData value) {
			return EnsureList().IndexOf(value);
		}

		public void Insert(int index, JsonData value) {
			EnsureList().Insert(index, value);
			cachedJsonString = null;
		}

		public bool Remove(JsonData value) {
			bool retval = EnsureList().Remove(value);
			cachedJsonString = null;
			return retval;
		}

		public void RemoveAt(int index) {
			EnsureList().RemoveAt(index);
			cachedJsonString = null;
		}

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

		public void CopyTo(JsonData[] array, int index) {
			EnsureList().CopyTo(array, index);
		}

		public JsonData this[int index] {
			get {
				return EnsureList()[index];
			}
			set {
				EnsureList()[index] = value;
				cachedJsonString = null;
			}
		}
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

		private IDictionary<string, JsonData> EnsureDictionary() {
			if (type == JsonType.Object) {
				return inst_object;
			}

			if (type != JsonType.None) {
				throw new InvalidOperationException("Instance of JsonData is not a dictionary");
			}

			type = JsonType.Object;
			inst_object = new Dictionary<string, JsonData>();

			return inst_object;
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

#if false
		private static void WriteJson(JsonData data, JsonWriter writer) {
			if (data == null) {
				writer.Write(null);
				return;
			}

			if (data.IsString) {
				writer.Write(data.StringValue);
				return;
			}

			if (data.IsBoolean) {
				writer.Write(data.BooleanValue);
				return;
			}

			if (data.IsFloat) {
				writer.Write(data.FloatValue);
				return;
			}

			if (data.IsInt) {
				writer.Write(data.IntValue);
				return;
			}

			if (data.IsArray) {
				writer.WriteArrayStart();
				foreach (var elem in data) {
					WriteJson(elem, writer);
				}
				writer.WriteArrayEnd();
				return;
			}

			if (data.IsObject) {
				writer.WriteObjectStart();

				// TODO 
				/*
				foreach (DictionaryEntry entry in ((IDictionary) data)) {
					writer.WritePropertyName((string)entry.Key);
					WriteJson(entry.Value, writer);
				}
				*/ 
				writer.WriteObjectEnd();

				return;
			}
		}
#endif
		#endregion

		#region IEquatable
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
		#endregion

		public string ToJson() {
			if (cachedJsonString != null)
				return cachedJsonString;

			// TODO
#if false
			StringWriter sw = new StringWriter();
			JsonWriter writer = new JsonWriter(sw);
			writer.Validate = false;

			WriteJson(this, writer);
			cachedJsonString = sw.ToString();
#endif

			return cachedJsonString;
		}

		// TODO
#if false
		public void ToJson(JsonWriter writer) {
			bool old_validate = writer.Validate;

			writer.Validate = false;

			WriteJson(this, writer);

			writer.Validate = old_validate;
		}
#endif

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
		
		//int ICollection.Count { get { return Count; } }
		//bool ICollection.IsSynchronized { get { return EnsureCollection().IsSynchronized; } }
		//object ICollection.SyncRoot { get { return EnsureCollection().SyncRoot; } }
		
		
		#if false
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
		#endif
		/*public JsonData this[string prop_name] {
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
		}*/

		
		
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
		
		/*
		public void Add(object value) {
			JsonData data = ToJsonData(value);
			cachedJsonString = null;
			EnsureList().Add(data);
		}*/

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

		/*
		bool IJsonWrapper.IsArray { get { return IsArray; } }
		bool IJsonWrapper.IsBoolean { get { return IsBoolean; } }
		bool IJsonWrapper.IsFloat { get { return IsFloat; } }
		bool IJsonWrapper.IsInt { get { return IsInt; } }
		bool IJsonWrapper.IsObject { get { return IsObject; } }
		bool IJsonWrapper.IsString { get { return IsString; } }
		*/
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
#endif