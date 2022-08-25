using System;
using System.Collections;
using System.Collections.Generic;

namespace Voorhees {
   /// JSON data type
   public enum JsonType : byte {
      Unspecified,
      
      Null,

      Object,
      Array,

      String,
      Boolean,
      
      // Json doesn't distinguish between number types, but it's often
      // useful to represent them as either ints or floats
      Int,
      Float
   }

   /// A union-type representing a value that can exist in a JSON document.
   /// Distinguishes between floating point and integral values even though JSON treats them both as the "number" type.
   /// Provides IList and IDictionary interfaces for easy enumeration of JSON arrays and objects.
   public class JsonValue : IDictionary<string, JsonValue>, IList<JsonValue>, IEquatable<JsonValue> {
      public JsonType Type { get; private set; }

      #region Constructors
      public JsonValue(bool boolean) {
         Type = JsonType.Boolean;
         boolValue = boolean;
      }

      public JsonValue(float number) {
         Type = JsonType.Float;
         floatValue = number;
      }

      public JsonValue(int number) {
         Type = JsonType.Int;
         intValue = number;
      }

      public JsonValue(string str) {
         if (str == null) {
            Type = JsonType.Null;
         } else {
            Type = JsonType.String;
            stringValue = str;
         }
      }

      public JsonValue(JsonType type = JsonType.Unspecified) {
         Type = type;
         switch (type) {
            case JsonType.Object:
               objectValue = new Dictionary<string, JsonValue>();
               break;
            case JsonType.Array:
               arrayValue = new List<JsonValue>();
               break;
         }
      }
      #endregion

      #region Implicit Conversions to JsonValue from other types
      public static implicit operator JsonValue(bool data) { return new JsonValue(data); }
      public static implicit operator JsonValue(float data) { return new JsonValue(data); }
      public static implicit operator JsonValue(int data) { return new JsonValue(data); }
      public static implicit operator JsonValue(string data) { return new JsonValue(data); }
      #endregion

      #region Explicit Conversions from JsonData to other types
      public static explicit operator bool(JsonValue data) {
         if (data.Type != JsonType.Boolean) {
            throw new InvalidCastException("Instance of JsonData doesn't hold a boolean");
         }
         return data.boolValue;
      }

      public static explicit operator float(JsonValue data) {
         if (data.Type != JsonType.Float) {
            throw new InvalidCastException("Instance of JsonData doesn't hold a float");
         }
         return data.floatValue;
      }

      public static explicit operator int(JsonValue data) {
         if (data.Type != JsonType.Int) {
            throw new InvalidCastException("Instance of JsonData doesn't hold an int");
         }
         return data.intValue;
      }

      public static explicit operator string(JsonValue data) {
         if (data.Type != JsonType.String) {
            throw new InvalidCastException("Instance of JsonData doesn't hold a string");
         }
         return data.stringValue;
      }
      #endregion

      #region IEnumerable
      IEnumerator<JsonValue> IEnumerable<JsonValue>.GetEnumerator() => EnsureArray().GetEnumerator();
      IEnumerator<KeyValuePair<string, JsonValue>> IEnumerable<KeyValuePair<string, JsonValue>>.GetEnumerator() => EnsureObject().GetEnumerator();
      public IEnumerator GetEnumerator() {
         if (Type == JsonType.Array) {
            return ((IEnumerable<JsonValue>)this).GetEnumerator();
         }
         return ((IEnumerable<KeyValuePair<string, JsonValue>>)this).GetEnumerator();
      }
      #endregion

      #region ICollection<JsonValue>
      public void Add(JsonValue item) => EnsureArray().Add(item);
      public bool Contains(JsonValue item) => EnsureArray().Contains(item);
      public void CopyTo(JsonValue[] array, int arrayIndex) => EnsureArray().CopyTo(array, arrayIndex);
      public bool Remove(JsonValue item) => EnsureArray().Remove(item);
      public bool IsReadOnly => EnsureArray().IsReadOnly;

      public void Clear() {
         switch (Type) {
            case JsonType.Object: objectValue.Clear(); break;
            case JsonType.Array: arrayValue.Clear(); break;
            default: throw new InvalidOperationException("Instance of JsonValue is not an array or object");
         }
      }

      public int Count {
         get {
            switch (Type) {
               case JsonType.Object: return objectValue.Count;
               case JsonType.Array: return arrayValue.Count;
               default: throw new InvalidOperationException("Instance of JsonValue is not an array or object");
            }
         }
      }
      #endregion

      #region IList<JsonValue>
      public int IndexOf(JsonValue item) => EnsureArray().IndexOf(item);
      public void Insert(int index, JsonValue item) => EnsureArray().Insert(index, item);
      public void RemoveAt(int index) => EnsureArray().RemoveAt(index);
      public JsonValue this[int index] {
         get => EnsureArray()[index];
         set => EnsureArray()[index] = value;
      }
      #endregion

      #region IEquatable<JsonValue>
      public bool Equals(JsonValue other) {
         if (other == null) {
            return Type == JsonType.Null;
         }

         if (Type != other.Type) {
            return false;
         }

         switch (Type) {
            case JsonType.Null: return true;
            case JsonType.String: return stringValue == other.stringValue;
            case JsonType.Boolean: return boolValue == other.boolValue;
            case JsonType.Int: return intValue == other.intValue;
            case JsonType.Float: return floatValue == other.floatValue;
            case JsonType.Object: {
               if (objectValue.Count != other.objectValue.Count) {
                  return false;
               }

               foreach (var kvp in objectValue) {
                  if (!other.objectValue.TryGetValue(kvp.Key, out var bValue)) {
                     return false; // key missing in b
                  }

                  if (!kvp.Value.Equals(bValue)) {
                     return false; // value is different
                  }
               }

               return true;
            }
            case JsonType.Array: {
               if (arrayValue.Count != other.arrayValue.Count) {
                  return false;
               }

               for (int i = 0; i < arrayValue.Count; ++i) {
                  if (!arrayValue[i].Equals(other.arrayValue[i])) {
                     return false;
                  }
               }
               return true;
            }
         }
         return false;
      }
      #endregion

      #region ICollection<KeyValuePair<string, JsonValue>>
      public bool Remove(KeyValuePair<string, JsonValue> value) => EnsureObject().Remove(value);
      public void CopyTo(KeyValuePair<string, JsonValue>[] array, int arrayIndex) => EnsureObject().CopyTo(array, arrayIndex);
      public bool Contains(KeyValuePair<string, JsonValue> value) => EnsureObject().Contains(value);
      public void Add(KeyValuePair<string, JsonValue> value) => EnsureObject().Add(value);
      #endregion

      #region IDictionary<string, JsonValue>
      public JsonValue this[string key] {
         get => EnsureObject()[key];
         set => EnsureObject()[key] = value;
      }
      public ICollection<string> Keys => EnsureObject().Keys;
      public ICollection<JsonValue> Values => EnsureObject().Values;
      public void Add(string key, JsonValue value) => EnsureObject().Add(key, value);
      public bool TryGetValue(string key, out JsonValue value) => EnsureObject().TryGetValue(key, out value);
      public bool Remove(string key) => EnsureObject().Remove(key);
      public bool ContainsKey(string key) => EnsureObject().ContainsKey(key);
      #endregion
      
      /////////////////////////////////////////////////
      
      readonly bool boolValue;
      readonly int intValue;
      readonly float floatValue;
      readonly string stringValue;
      List<JsonValue> arrayValue;
      Dictionary<string, JsonValue> objectValue;
      
      /////////////////////////////////////////////////

      IDictionary<string, JsonValue> EnsureObject() {
         if (Type == JsonType.Unspecified) {
            Type = JsonType.Object;
            objectValue = new Dictionary<string, JsonValue>();
         }

         if (Type == JsonType.Object) {
            return objectValue;
         }

         throw new InvalidOperationException("Instance of JsonValue is not an object");
      }

      IList<JsonValue> EnsureArray() {
         if (Type == JsonType.Unspecified) {
            Type = JsonType.Array;
            arrayValue = new List<JsonValue>();
         }

         if (Type == JsonType.Array) {
            return arrayValue;
         }

         throw new InvalidOperationException("Instance of JsonValue is not an array");
      }
   }
}
