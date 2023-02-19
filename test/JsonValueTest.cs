using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Voorhees.Tests {
	[TestFixture]
	class JsonValue_Undefined {
		[Test]
		public void ConstructingAnEmptyJsonValueSetsItsTypeToUnspecified() {
			Assert.That(new JsonValue().Type, Is.EqualTo(JsonValueType.Unspecified));
		}
		
		[Test]
		public void EqualsIsAlwaysFalse() {
			var one = new JsonValue();
			var two = new JsonValue();
			var three = new JsonValue(1);
			Assert.Multiple(() => {
				Assert.That(one, Is.Not.EqualTo(two));
				Assert.That(one, Is.Not.EqualTo(three));
				Assert.That(three, Is.Not.EqualTo(one));
			});
		}
	}
	
	[TestFixture]
	class JsonValue_Boolean {
		[Test]
		public void ConstructingWithBoolSetsTypeToBoolean() {
			Assert.Multiple(() => {
				Assert.That(new JsonValue(true).Type, Is.EqualTo(JsonValueType.Boolean));
				Assert.That(new JsonValue(false).Type, Is.EqualTo(JsonValueType.Boolean));
			});
		}

		[Test]
		public void ImplicitConversionCreatesBoolValue() {
			JsonValue test = false;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Boolean));
			});
		}

		[Test]
		public void ExplicitConversionOperatorReturnsUnderlyingValue() {
			JsonValue test = false;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Boolean));
				Assert.That((bool) test, Is.False);
			});
		}

		[Test]
		public void ExplicitConversionForNonBoolTypeThrows() {
			JsonValue test = 1.0f;
			Assert.Throws<InvalidCastException>(() => {
				bool _ = (bool)test;
			});
		}

		[Test]
		public void EqualsTestsEquality() {
			JsonValue one = true;
			JsonValue two = true;
			JsonValue three = false;
			Assert.Multiple(() => {
				Assert.That(one.Equals(two), Is.True);
				Assert.That(one.Equals(three), Is.False);
			});
		}
		
		[Test]
		public void TryingToCallClearThrows() {
			JsonValue test = true;
			Assert.Throws<InvalidOperationException>(() => test.Clear());
		}
		
		[Test]
		public void TryingToCallCountThrows() {
			JsonValue test = true;
			Assert.Throws<InvalidOperationException>(() => {
				int _ = test.Count;
			});
		}
	}
	
	[TestFixture]
	class JsonValue_Double {
		[Test]
		public void ConstructingWithFloatSetsTypeToDouble() {
			Assert.That(new JsonValue(1.0).Type, Is.EqualTo(JsonValueType.Double));
		}

		[Test]
		public void ImplicitConversionCreatesFloatDouble() {
			JsonValue test = 1.0;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Double));
			});
		}

		[Test]
		public void ExplicitConversionOperatorReturnsUnderlyingValue() {
			JsonValue test = 1.0;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Double));
				Assert.That((double) test, Is.EqualTo(1.0));
			});
		}

		[Test]
		public void ExplicitConversionOperatorConvertsToFloat() {
			JsonValue test = 1.0;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Double));
				Assert.That((float) test, Is.EqualTo(1.0f));
			});
		}

		[Test]
		public void ExplicitConversionOperatorConvertsToDecimal() {
			JsonValue test = 1.0;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Double));
				Assert.That((decimal) test, Is.EqualTo(1.0m));
			});
		}

		[Test]
		public void ExplicitConversionForNonDoubleTypeThrows() {
			JsonValue test = true;
			Assert.Throws<InvalidCastException>(() => {
				double _ = (double)test;
			});
		}

		[Test]
		public void EqualsTestsEquality() {
			JsonValue one = 1.0;
			JsonValue two = 1.0;
			JsonValue three = 2.0;
			Assert.Multiple(() => {
				Assert.That(one, Is.EqualTo(two));
				Assert.That(one, Is.Not.EqualTo(three));
			});
		}
	}
	
	[TestFixture]
	class JsonValue_Integer {
		[Test]
		public void ConstructingWithIntSetsTypeToInt() {
			Assert.That(new JsonValue(1).Type, Is.EqualTo(JsonValueType.Int));
		}

		[Test]
		public void ImplicitConversionCreatesIntValue() {
			JsonValue test = 1;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Int));
			});
		}

		[Test]
		public void ExplicitConversionOperatorReturnsUnderlyingValue() {
			JsonValue test = 1;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)test, Is.EqualTo(1));
			});
		}

		[Test]
		public void ExplicitConversionOperatorConvertsToSByte() {
			JsonValue test = 1;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((sbyte)test, Is.EqualTo((sbyte)1));
			});
		}

		[Test]
		public void ExplicitConversionOperatorConvertsToByte() {
			JsonValue test = 1;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((byte)test, Is.EqualTo((byte)1));
			});
		}

		[Test]
		public void ExplicitConversionOperatorConvertsToShort() {
			JsonValue test = 1;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((short)test, Is.EqualTo((short)1));
			});
		}

		[Test]
		public void ExplicitConversionOperatorConvertsToUShort() {
			JsonValue test = 1;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((ushort)test, Is.EqualTo((ushort)1));
			});
		}

		[Test]
		public void ExplicitConversionOperatorConvertsToUInt() {
			JsonValue test = 1;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((uint)test, Is.EqualTo(1U));
			});
		}

		[Test]
		public void ExplicitConversionOperatorConvertsToLong() {
			JsonValue test = 1;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((long)test, Is.EqualTo(1L));
			});
		}

		[Test]
		public void ExplicitConversionOperatorConvertsToULong() {
			JsonValue test = 1;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((ulong)test, Is.EqualTo(1ul));
			});
		}

		// Because char is really an integral type https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/types
		[Test]
		public void ExplicitConversionOperatorConvertsToChar() {
			JsonValue test = 1;
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((char)test, Is.EqualTo((char)1));
			});
		}

		[Test]
		public void ExplicitConversionForNonIntTypeThrows() {
			JsonValue test = true;
			Assert.Throws<InvalidCastException>(() => {
				int _ = (int)test;
			});
		}

		[Test]
		public void EqualsTestsEquality() {
			JsonValue one = 1;
			JsonValue two = 1;
			JsonValue three = 2;
			Assert.Multiple(() => {
				Assert.That(one, Is.EqualTo(two));
				Assert.That(one, Is.Not.EqualTo(three));
			});
		}

		[Test]
		public void AttemptingToEnumerateThrows() {
			Assert.Throws<InvalidOperationException>(() => {
				var _ = new JsonValue(1).GetEnumerator();
			});
		}
	}
	
	[TestFixture]
	class JsonValue_String {
		[Test]
		public void ConstructingWithStringSetsTypeToString() {
			Assert.That(new JsonValue("test").Type, Is.EqualTo(JsonValueType.String));
		}

		[Test]
		public void ImplicitConversionCreatesIntValue() {
			JsonValue test = "test";
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.String));
			});
		}

		[Test]
		public void ExplicitConversionOperatorReturnsUnderlyingValue() {
			JsonValue test = "test";
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.String));
				Assert.That((string) test, Is.EqualTo("test"));
			});
		}

		[Test]
		public void ExplicitConversionForNonStringTypeThrows() {
			JsonValue test = true;
			Assert.Throws<InvalidCastException>(() => {
				string _ = (string)test;
			});
		}

		[Test]
		public void EqualsTestsEquality() {
			JsonValue one = "one";
			JsonValue two = "one";
			JsonValue three = "two";
			Assert.Multiple(() => {
				Assert.That(one.Equals(two), Is.True);
				Assert.That(one.Equals(three), Is.False);
			});
		}
	}
	
	[TestFixture]
	class JsonValue_Null {
		[Test]
		public void ConstructingWithNullSetsItToNull() {
			Assert.That(new JsonValue(null).Type, Is.EqualTo(JsonValueType.Null));
		}

		[Test]
		public void EqualsTestsEquality() {
			var one = new JsonValue(null);
			var two = new JsonValue(null);
			var three = new JsonValue(3);
			Assert.Multiple(() => {
				Assert.That(one.Equals(null), Is.True);
				Assert.That(one.Equals(two), Is.True);
				Assert.That(one.Equals(three), Is.False);
			});
		}
	}

	[TestFixture]
	class JsonValue_Array {
		[Test]
		public void InitializingWithArraySetsTheTypeToArray() {
			Assert.That(new JsonValue {1, 2, 3}.Type, Is.EqualTo(JsonValueType.Array));
		}
		
		[Test]
		public void ArrayLiteralCreatesArrayInstance() {
			var test = new JsonValue {
				3,
				2,
				1,
				"blastoff!"
			};
			Assert.Multiple(() => {
				Assert.That(test.Type, Is.EqualTo(JsonValueType.Array));
				Assert.That(test[0].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That(test[1].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That(test[2].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That(test[3].Type, Is.EqualTo(JsonValueType.String));
				Assert.That((int)test[0], Is.EqualTo(3));
				Assert.That((int)test[1], Is.EqualTo(2));
				Assert.That((int)test[2], Is.EqualTo(1));
				Assert.That((string)test[3], Is.EqualTo("blastoff!"));
			});
		}

		[Test]
		public void SimilarArraysAreEqual() {
			// array
			var one = new JsonValue {1, 2, 3};
			var two = new JsonValue {1, 2, 3};
			var three = new JsonValue {3, 2, 1};
			var four = new JsonValue {3, 2, 1, 5};
			Assert.Multiple(() => {
				Assert.That(one, Is.EqualTo(two));
				Assert.That(one, Is.Not.EqualTo(three));
				
				// Same array but with one extra element
				Assert.That(three, Is.Not.EqualTo(four));
			});
		}

		[Test]
		public void Contains() {
			var test = new JsonValue {1, 2, 3};
			Assert.Multiple(() => {
				Assert.That(test.Count, Is.EqualTo(3));
				Assert.That(test.Contains(1), Is.True);
				Assert.That(test.Contains(4), Is.False);
			});
		}

		[Test]
		public void CopyToCopiesValues() {
			var test = new JsonValue {1, 2, 3};
			var values = new JsonValue[3];
			test.CopyTo(values, 0);
			Assert.Multiple(() => {
				Assert.That((int) values[0], Is.EqualTo((int) test[0]));
				Assert.That((int) values[1], Is.EqualTo((int) test[1]));
				Assert.That((int) values[2], Is.EqualTo((int) test[2]));
			});
			
		}

		[Test]
		public void RemoveDeletesElementFromArray() {
			var test = new JsonValue {1, 2, 3};
			test.Remove(2);
			Assert.Multiple(() => {
				Assert.That(test, Has.Count.EqualTo(2));
				Assert.That((int) test[0], Is.EqualTo(1));
				Assert.That((int) test[1], Is.EqualTo(3));
			});
		}

		[Test]
		public void ArraysAreNotReadOnly() {
			Assert.That(new JsonValue {1, 2, 3}.IsReadOnly, Is.False);
		}

		[Test]
		public void IndexOfReturnsTheIndexOfTheFirstMatchingElement() {
			var test = new JsonValue {1, 2, 3, 1};
			Assert.Multiple(() => {
				Assert.That(test.IndexOf(2), Is.EqualTo(1));
				Assert.That(test.IndexOf(1), Is.EqualTo(0));
			});
		}

		[Test]
		public void InsertAddsAnItemAtTheSpecifiedIndex() {
			var test = new JsonValue {1, 3};
			test.Insert(1, 2);
			Assert.Multiple(() => {
				Assert.That((int) test[1], Is.EqualTo(2));
				Assert.That(test.Count, Is.EqualTo(3));
			});
		}

		[Test]
		public void RemoveAtErasesItemAtIndex() {
			var test = new JsonValue {1, 2, 3};
			test.RemoveAt(1);
			Assert.Multiple(() => {
				Assert.That(test, Has.Count.EqualTo(2));
				Assert.That((int) test[0], Is.EqualTo(1));
				Assert.That((int) test[1], Is.EqualTo(3));
			});
		}

		[Test]
		public void ClearRemovesAllItems() {
			var test = new JsonValue {1, 2, 3};
			test.Clear();
			Assert.That(test.Count, Is.EqualTo(0));
		}

		[Test]
		public void AddAppendsElements() {
			var test = new JsonValue {1};
			test.Add(5);
			Assert.Multiple(() => {
				Assert.That(test.Count, Is.EqualTo(2));
				Assert.That((int) test[0], Is.EqualTo(1));
				Assert.That((int) test[1], Is.EqualTo(5));
			});
		}

		[Test]
		public void IndexedEntriesAreAssignable() {
			var test = new JsonValue {1, 2, 3};
			test[0] = 10;
			Assert.Multiple(() => {
				Assert.That(test.Count, Is.EqualTo(3));
				Assert.That((int) test[0], Is.EqualTo(10));
				Assert.That((int) test[1], Is.EqualTo(2));
				Assert.That((int) test[2], Is.EqualTo(3));
			});
		}

		[Test]
		public void GetEnumeratorAllowsIteratingThroughTheArray() {
			var test = new JsonValue {1, 2, 3};
			IEnumerator<JsonValue> iterator = ((IEnumerable<JsonValue>)test).GetEnumerator();
			Assert.Multiple(() => {
				Assert.That(iterator, Is.Not.Null);
				
				Assert.That(iterator.MoveNext(), Is.True);
				Assert.That(iterator.Current, Is.Not.Null);
				Assert.That(iterator.Current.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)iterator.Current, Is.EqualTo(1));

				Assert.That(iterator.MoveNext(), Is.True);
				Assert.That(iterator.Current, Is.Not.Null);
				Assert.That(iterator.Current.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)iterator.Current, Is.EqualTo(2));
				
				Assert.That(iterator.MoveNext(), Is.True);
				Assert.That(iterator.Current, Is.Not.Null);
				Assert.That(iterator.Current.Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)iterator.Current, Is.EqualTo(3));
				
				Assert.That(iterator.MoveNext(), Is.False);
			});
		}

		[Test]
		public void UntypedGetEnumeratorAllowsIteratingThroughTheArray() {
			var test = new JsonValue {1, 2, 3};
			IEnumerator iterator = test.GetEnumerator();
			Assert.Multiple(() => {
				Assert.That(iterator, Is.Not.Null);
				
				Assert.That(iterator.MoveNext(), Is.True);
				Assert.That(iterator.Current, Is.Not.Null);
				Assert.That(iterator.Current.GetType(), Is.EqualTo(typeof(JsonValue)));
				Assert.That(((JsonValue)iterator.Current).Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)(JsonValue)iterator.Current, Is.EqualTo(1));
				
				Assert.That(iterator.MoveNext(), Is.True);
				Assert.That(iterator.Current, Is.Not.Null);
				Assert.That(iterator.Current.GetType(), Is.EqualTo(typeof(JsonValue)));
				Assert.That(((JsonValue)iterator.Current).Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)(JsonValue)iterator.Current, Is.EqualTo(2));
				
				Assert.That(iterator.MoveNext(), Is.True);
				Assert.That(iterator.Current, Is.Not.Null);
				Assert.That(iterator.Current.GetType(), Is.EqualTo(typeof(JsonValue)));
				Assert.That(((JsonValue)iterator.Current).Type, Is.EqualTo(JsonValueType.Int));
				Assert.That((int)(JsonValue)iterator.Current, Is.EqualTo(3));
				
				Assert.That(iterator.MoveNext(), Is.False);
			});
		}
		
		[Test]
		public void ClearRemovesAllElements() {
			var test = new JsonValue {1, 2, 3};
			Assert.Multiple(() => {
				Assert.That(test.Count, Is.EqualTo(3));
				test.Clear();
				Assert.That(test.Count, Is.EqualTo(0));
			});
		}

		[Test]
		public void AttemptingToCallArrayMethodsOnANonArrayValueThrows() {
			JsonValue test = 1;
			Assert.Throws<InvalidOperationException>(() => {
				test.Remove(new JsonValue(1));
			});
		}
	}

	[TestFixture]
	class JsonValue_Object {
		[Test]
		public void ConstructingWithDictionaryLiteralCreatesObject() {
			var test = new JsonValue {
				{"three", 3},
				{"two", 2},
				{"one", 1},
				{"blast", "off!"}
			};
			Assert.That(test.Type, Is.EqualTo(JsonValueType.Object));
		}

		[Test]
		public void ContainsKeyReturnsTrueIfObjectHasPropertyWithTheGivenKey() {
			var test = new JsonValue {
				{"three", 3},
				{"two", 2},
				{"one", 1},
				{"blast", "off!"}
			};
			
			Assert.Multiple(() => {
				Assert.That(test.ContainsKey("three"), Is.True);
				Assert.That(test.ContainsKey("two"), Is.True);
				Assert.That(test.ContainsKey("one"), Is.True);
				Assert.That(test.ContainsKey("blast"), Is.True);
			});
		}

		[Test]
		public void PropertyValuesAreProperlyConstructedFromDictionaryLiteral() {
			var test = new JsonValue {
				{ "three", 3 },
				{ "two", 2 },
				{ "one", 1 },
				{ "blast", "off!" }
			};

			Assert.Multiple(() => {
				Assert.That(test["three"].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That(test["two"].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That(test["one"].Type, Is.EqualTo(JsonValueType.Int));
				Assert.That(test["blast"].Type, Is.EqualTo(JsonValueType.String));

				Assert.That((int)test["three"], Is.EqualTo(3));
				Assert.That((int)test["two"], Is.EqualTo(2));
				Assert.That((int)test["one"], Is.EqualTo(1));
				Assert.That((string)test["blast"], Is.EqualTo("off!"));
			});
		}

		[Test]
		public void EquivalentObjectsAreEqual() {
			var one = new JsonValue {{"one", 1}, {"two", 2}};
			var two = new JsonValue {{"one", 1}, {"two", 2}};
			Assert.That(one.Equals(two), Is.True);
		}
		
		[Test]
		public void ObjectsWithDifferentCountAreNotEqual() {
			var one = new JsonValue {{"one", 1}, {"two", 2}};
			var two = new JsonValue {{"one", 1}, {"two", 2}, {"three", 3}};
			Assert.That(one.Equals(two), Is.False);
		}
		
		[Test]
		public void ObjectWithDifferentPropertyValuesAreNotEqual() {
			var one = new JsonValue {{"one", 1}, {"two", 2}};
			var two = new JsonValue {{"one", "uno"}, {"two", "dos"}};
			Assert.That(one.Equals(two), Is.False);
		}

		[Test]
		public void ObjectsWithDifferentPropertyNamesAreNotEqual() {
			var one = new JsonValue {{"one", 1}, {"two", 2}};
			var two = new JsonValue {{"one", 1}, {"three", 3}};
			Assert.That(one.Equals(two), Is.False);
		}

		[Test]
		public void RemoveErasesThePropertyWithTheGivenKey() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};

			test.Remove(new KeyValuePair<string, JsonValue>("one", 1));
			Assert.That(test.Count, Is.EqualTo(2));
			Assert.Throws<KeyNotFoundException>(() => {
				string _ = test["one"].ToString();
			});
		}

		[Test]
		public void IndexingNonObjectsThrows() {
			JsonValue intVal = 4;
			Assert.Throws<InvalidOperationException>(() => {
				JsonValue _ = intVal["one"];
			});
		}

		[Test]
		public void CopyToCopiesObjectPropertiesToAKeyValuePairArray() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};

			var dest = new KeyValuePair<string, JsonValue>[3];
			test.CopyTo(dest, 0);
			Assert.Multiple(() => {
				for (int i = 0; i < 3; ++i) {
					Assert.That(test[dest[i].Key].Equals(dest[i].Value));
				}
			});
		}

		[Test]
		public void ContainsFindsKeyValuePairsInTheObject() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};
			Assert.That(test.Contains(new KeyValuePair<string, JsonValue>("two", 2)), Is.True);
			Assert.That(test.Contains(new KeyValuePair<string, JsonValue>("two", 5)), Is.False);
		}
		
		[Test]
		public void ContainsKeyChecksForObjectPropertyNames() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};
			Assert.That(test.ContainsKey("two"), Is.True);
			Assert.That(test.ContainsKey("five"), Is.False);
		}

		[Test]
		public void AddInsertsANewKeyValuePairIntoTheObject() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};

			test.Add(new KeyValuePair<string, JsonValue>("four", 4));

			Assert.That(test.ContainsKey("four"), Is.True);
			Assert.That((int) test["four"], Is.EqualTo(4));
		}

		[Test]
		public void IndexingReturnsTheValueAssociatedWithAKey() {
			var test = new JsonValue {
				{"one", 1}
			};
			Assert.That((int) test["one"], Is.EqualTo(1));
		}
		
		[Test]
		public void IndexedValuesAreAssignable() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};

			Assert.That((int) test["one"], Is.EqualTo(1));
			test["one"] = 5;
			Assert.That((int) test["one"], Is.EqualTo(5));
		}

		[Test]
		public void KeysReturnsACollectionOfPropertyNames() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};

			ICollection<string> keys = test.Keys;
			Assert.That(keys.Count, Is.EqualTo(3));
			Assert.That(keys.Contains("one"), Is.True);
			Assert.That(keys.Contains("two"), Is.True);
			Assert.That(keys.Contains("three"), Is.True);
		}
		
		[Test]
		public void ValuesReturnsACollectionOfPropertyValues() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};

			ICollection<JsonValue> keys = test.Values;
			Assert.That(keys.Count, Is.EqualTo(3));
			Assert.That(keys.Contains(1), Is.True);
			Assert.That(keys.Contains(2), Is.True);
			Assert.That(keys.Contains(3), Is.True);
		}
		
		[Test]
		public void TryGetValueFindsKeyValuePairs() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};
			Assert.That(test.TryGetValue("one", out var val), Is.True);
			Assert.That((int) val, Is.EqualTo(1));
		}
		
		[Test]
		public void TryGetValueSetsOutVarToNullWhenKeyNotFound() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};
			Assert.That(test.TryGetValue("seven", out var val), Is.False);
			Assert.That(val, Is.Null);
		}

		[Test]
		public void RemoveErasesKeyValuePairsFromObject() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};
			
			test.Remove("two");
			Assert.That(test.Count, Is.EqualTo(2));
			Assert.That(test.ContainsKey("two"), Is.False);
		}

		[Test]
		public void GetEnumeratorAllowsIteratingThroughTheKeyValuePairs() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};
			IEnumerator<KeyValuePair<string, JsonValue>> iterator = ((IEnumerable<KeyValuePair<string, JsonValue>>)test).GetEnumerator();
			Assert.Multiple(() => {
				Assert.That(iterator, Is.Not.Null);
				
				Assert.That(iterator.MoveNext(), Is.True);
				Assert.That(iterator.Current.Key, Is.EqualTo("one"));
				Assert.That(iterator.Current.Value.Equals(1), Is.True);

				Assert.That(iterator.MoveNext(), Is.True);
				Assert.That(iterator.Current.Key, Is.EqualTo("two"));
				Assert.That(iterator.Current.Value.Equals(2), Is.True);
				
				Assert.That(iterator.MoveNext(), Is.True);
				Assert.That(iterator.Current.Key, Is.EqualTo("three"));
				Assert.That(iterator.Current.Value.Equals(3), Is.True);
				
				Assert.That(iterator.MoveNext(), Is.False);
			});
		}

		[Test]
		public void UntypedGetEnumeratorAllowsIteratingThroughTheArray() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};
			IEnumerator iterator = ((IEnumerable)test).GetEnumerator();
			Assert.Multiple(() => {
				Assert.That(iterator, Is.Not.Null);
				
				Assert.That(iterator.MoveNext(), Is.True);
				Assert.That(iterator.Current, Is.Not.Null);
				Assert.That(((KeyValuePair<string, JsonValue>)iterator.Current).Key, Is.EqualTo("one"));
				Assert.That(((KeyValuePair<string, JsonValue>)iterator.Current).Value.Equals(1), Is.True);
				
				Assert.That(iterator.MoveNext(), Is.True);
				Assert.That(iterator.Current, Is.Not.Null);
				Assert.That(((KeyValuePair<string, JsonValue>)iterator.Current).Key, Is.EqualTo("two"));
				Assert.That(((KeyValuePair<string, JsonValue>)iterator.Current).Value.Equals(2), Is.True);

				Assert.That(iterator.MoveNext(), Is.True);
				Assert.That(iterator.Current, Is.Not.Null);
				Assert.That(((KeyValuePair<string, JsonValue>)iterator.Current).Key, Is.EqualTo("three"));
				Assert.That(((KeyValuePair<string, JsonValue>)iterator.Current).Value.Equals(3), Is.True);
				
				Assert.That(iterator.MoveNext(), Is.False);
			});
		}

		[Test]
		public void ClearRemovesAllElements() {
			var test = new JsonValue {
				{"one", 1},
				{"two", 2},
				{"three", 3}
			};
			Assert.Multiple(() => {
				Assert.That(test.Count, Is.EqualTo(3));
				test.Clear();
				Assert.That(test.Count, Is.EqualTo(0));
			});
		}
	}
}
