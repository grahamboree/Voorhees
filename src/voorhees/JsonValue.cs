#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Voorhees {
   /// A union-type representing a value in a JSON document.
   /// Provides IList and IDictionary interfaces for easy enumeration of JSON arrays and objects.
   /// Distinguishes between floating point and integral values even though JSON treats them both as doubles.
   public class JsonValue : IDictionary<string, JsonValue>, IList<JsonValue>, IEquatable<JsonValue> {
      public JsonValueType Type { get; private set; }

      #region Constructors
      public JsonValue(bool boolean) {
         Type = JsonValueType.Boolean;
         numberValue = boolean ? 1.0 : 0.0;
      }

      public JsonValue(double number) {
         Type = JsonValueType.Double;
         numberValue = number;
      }

      public JsonValue(int number) {
         Type = JsonValueType.Int;
         numberValue = number;
      }

      public JsonValue(string? str) {
         if (str == null) {
            Type = JsonValueType.Null;
         } else {
            Type = JsonValueType.String;
            stringValue = str;
         }
      }

      public JsonValue(JsonValueType type = JsonValueType.Unspecified) {
         Type = type;
         if (type == JsonValueType.Object) {
            ObjectValue = new Dictionary<string, JsonValue>();
         } else if (type == JsonValueType.Array) {
            arrayValue = new List<JsonValue>();
         }
      }
      #endregion

      #region Implicit Conversions to JsonValue from other types
      public static implicit operator JsonValue(bool data) { return new JsonValue(data); }
      public static implicit operator JsonValue(double data) { return new JsonValue(data); }
      public static implicit operator JsonValue(int data) { return new JsonValue(data); }
      public static implicit operator JsonValue(string data) { return new JsonValue(data); }
      #endregion

      #region Explicit Conversions from JsonData to other types
      public static explicit operator bool(JsonValue data) {
         if (data.Type != JsonValueType.Boolean) {
            throw new InvalidCastException("Instance of JsonData doesn't hold a boolean");
         }
         return data.numberValue == 1.0;
      }

      public static explicit operator double(JsonValue data) {
         if (data.Type != JsonValueType.Double) {
            throw new InvalidCastException("Instance of JsonData doesn't hold a number");
         }
         return data.numberValue;
      }

      public static explicit operator float(JsonValue data) { return (float)(double)data; }
      public static explicit operator decimal(JsonValue data) { return (decimal)(double)data; }
      
      public static explicit operator int(JsonValue data) {
         if (data.Type != JsonValueType.Int) {
            throw new InvalidCastException("Instance of JsonData doesn't hold a number");
         }
         return (int)data.numberValue;
      }
      
      public static explicit operator uint(JsonValue data) { return (uint)(int)data; }
      public static explicit operator sbyte(JsonValue data) { return (sbyte)(int)data; }
      public static explicit operator byte(JsonValue data) { return (byte)(int)data; }
      public static explicit operator short(JsonValue data) { return (short)(int)data; }
      public static explicit operator ushort(JsonValue data) { return (ushort)(int)data; }
      public static explicit operator long(JsonValue data) { return (int)data; }
      public static explicit operator ulong(JsonValue data) { return (ulong)(int)data; }
      
      public static explicit operator string?(JsonValue data) {
         return data.Type == JsonValueType.String ? data.stringValue
            : throw new InvalidCastException("Instance of JsonData doesn't hold a string");
      }

      public static explicit operator char(JsonValue data) {
         if (data.Type == JsonValueType.String) {
            return ((string?)data)![0];
         }
         if (data.Type == JsonValueType.Int) {
            return (char)(int)data.numberValue;
         }
         throw new InvalidCastException("Instance of JsonData doesn't hold a string or number");
      }
      #endregion

      #region IEnumerable
      IEnumerator<JsonValue> IEnumerable<JsonValue>.GetEnumerator() => EnsureArray().GetEnumerator();
      IEnumerator<KeyValuePair<string, JsonValue>> IEnumerable<KeyValuePair<string, JsonValue>>.GetEnumerator() => EnsureObject().GetEnumerator();
      public IEnumerator GetEnumerator() {
         switch (Type) {
            case JsonValueType.Array:
               return ((IEnumerable<JsonValue>)this).GetEnumerator();
            case JsonValueType.Object:
               return ((IEnumerable<KeyValuePair<string, JsonValue>>)this).GetEnumerator();
            default:
               throw new InvalidOperationException($"Can't enumerate JsonValue of type {Type}");
         }
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
            case JsonValueType.Object: ObjectValue!.Clear(); break;
            case JsonValueType.Array: arrayValue!.Clear(); break;
            default: throw new InvalidOperationException("Instance of JsonValue is not an array or object");
         }
      }

      public int Count {
         get {
            switch (Type) {
               case JsonValueType.Object: return ObjectValue!.Count;
               case JsonValueType.Array: return arrayValue!.Count;
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
      public bool Equals(JsonValue? other) {
         if (other == null) {
            return Type == JsonValueType.Null;
         }

         if (Type != other.Type) {
            return false;
         }

         switch (Type) {
            case JsonValueType.Null: return true;
            case JsonValueType.String: return stringValue == other.stringValue;
            case JsonValueType.Boolean: return (numberValue == 1.0) == (other.numberValue == 1.0);
            case JsonValueType.Int: return (int)numberValue == (int)other.numberValue;
            case JsonValueType.Double: return numberValue == other.numberValue;
            case JsonValueType.Object: {
               if (ObjectValue!.Count != other.ObjectValue!.Count) {
                  return false;
               }

               foreach (var kvp in ObjectValue) {
                  if (!other.ObjectValue.TryGetValue(kvp.Key, out var bValue)) {
                     return false; // key missing in b
                  }

                  if (!kvp.Value.Equals(bValue)) {
                     return false; // value is different
                  }
               }

               return true;
            }
            case JsonValueType.Array: {
               if (arrayValue!.Count != other.arrayValue!.Count) {
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
      public bool TryGetValue(string key, [MaybeNullWhen(false)] out JsonValue value) => EnsureObject().TryGetValue(key, out value);
      public bool Remove(string key) => EnsureObject().Remove(key);
      public bool ContainsKey(string key) => EnsureObject().ContainsKey(key);
      #endregion
      
      /////////////////////////////////////////////////
      
      readonly double numberValue;
      readonly string? stringValue;
      List<JsonValue>? arrayValue;
      
      // This needs to be internal so we can enumerate the key value pairs when writing without allocating.
      internal Dictionary<string, JsonValue>? ObjectValue;
      
      /////////////////////////////////////////////////

      IDictionary<string, JsonValue> EnsureObject() {
         if (Type == JsonValueType.Unspecified) {
            Type = JsonValueType.Object;
            ObjectValue = new Dictionary<string, JsonValue>();
         }

         if (Type == JsonValueType.Object) {
            return ObjectValue!;
         }

         throw new InvalidOperationException("Instance of JsonValue is not an object");
      }

      IList<JsonValue> EnsureArray() {
         if (Type == JsonValueType.Unspecified) {
            Type = JsonValueType.Array;
            arrayValue = new List<JsonValue>();
         }

         if (Type == JsonValueType.Array) {
            return arrayValue!;
         }

         throw new InvalidOperationException("Instance of JsonValue is not an array");
      }
   }
}
