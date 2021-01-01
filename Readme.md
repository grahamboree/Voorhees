Voorhees
========

*A terrifyingly fast C# JSON micro-framework*

---

Current Status
------
[![Build Status](https://travis-ci.org/grahamboree/voorhees.svg?branch=master)](https://travis-ci.org/grahamboree/voorhees)[![Build status](https://ci.appveyor.com/api/projects/status/6nl98purvr2186iw?svg=true)](https://ci.appveyor.com/project/grahamboree/voorhees)[![Coverage Status](https://coveralls.io/repos/github/grahamboree/voorhees/badge.svg?branch=master)](https://coveralls.io/github/grahamboree/voorhees?branch=master)

[Kanban board](https://github.com/grahamboree/Voorhees/projects/1)

**Features**
* Read and write json data into a structured object graph with minimal allocations.
* Map complex objects to and from json values

**In-Development**
* Multidimensional array mapping support

**Planned**
* Add mapping option to read and de-serialize in one pass so it's not allocating a bunch of intermediate JsonValue's 
* Serialize and de-serialize correct underlying value for polymorphic type references
* dll release
* Field and Property annotations to ignore when serializing
* Serialized object versioning and version data migration system
* Optionally generate pretty-printed JSON instead of condensed JSON

Why
---

Why do we need another C# JSON library in 2020?  The simple answer is, we don't really.  Voorhees was created as a distraction from 2020, but it's also designed specifically with game development in mind.  This means a specific focus on memory and CPU performance, along with the ability to control these to suit your specific needs.  

---

Reading Json Data
-----------------

Given a JSON string...
```C#
string json = "{ \"someIntValue\": 3}";
```

You can read it into a `JsonValue` structure using the static `JsonReader.Read` method.  This is useful for reading and manipulating arbitrary data.
```C#
using Voorhees;

JsonValue jsonValue = JsonReader.Read(json);
Console.WriteLine(jsonValue.Type); // JsonType.Object
Console.WriteLine((int)jsonValue["someIntValue"]); // 3
```

Alternatively, you can map it to a matching C# structure using `JsonMapper`.  This is most useful for easily reading in structured config or save data into specific C# types.
```C#
using Voorhees;

struct ExampleStructure {
	public int someIntValue;
}

ExampleStructure mappedValue = JsonMapper.FromJson<ExampleStructure>(json);
Console.WriteLine(mappedValue.someIntValue); // 3
```

`JsonMapper` can also be used to map values to built-in types like `Dictionary<T, U>`, `List<T>`, or array values:
```C#
string dictJson = "{\"one\": 1, \"two\": 2}";

Dictionary<string, int> numberNamesToInts = JsonMapper.FromJson<Dictionary<string, int>>(dictJson);
Console.WriteLine(numberNamesToInts.Count); // 2
Console.WriteLine(numberNamesToInts["one"]); // 1
Console.WriteLine(numberNamesToInts["two"]); // 1

string arrayJson = "[1, 2, 3]";

List<int> mappedList = JsonMapper.FromJson<List<int>>(arrayJson);
Console.WriteLine(mappedList.Count); // 3
Console.WriteLine(mappedList[0]); // 1
Console.WriteLine(mappedList[1]); // 2
Console.WriteLine(mappedList[2]); // 3

int[] mappedArray = JsonMapper.FromJson<int[]>(arrayJson);
Console.WriteLine(mappedArray.Length); // 3
Console.WriteLine(mappedArray[0]); // 1
Console.WriteLine(mappedArray[1]); // 2
Console.WriteLine(mappedArray[2]); // 3
```

---

Writing Json Data
-----------------

You can convert a JsonValue object into the matching JSON string using `JsonWriter`:
```C#
using Voorhees;

JsonValue objectValue = new JsonValue {
	{ "one", 1 },
	{ "two", 2 },
	{ "three", 3 }
};
string jsonObject = JsonWriter.ToJson(objectValue);
Console.WriteLine(jsonObject); // {"one":1,"two":2,"three":3}

JsonValue arrayValue = new JsonValue {1, 2, 3};
string jsonArray = JsonWriter.ToJson(arrayValue);
Console.WriteLine(jsonObject); // [1,2,3]
```

Alternatively, you can map a C# value to a matching JSON string using `JsonMapper`.
```C#
using Voorhees;

struct Player {
	public string name;
	public int gold;
	public float ageYears;
}

Player Bilbo = new Player {
	name = "Bilbo Baggins",
	gold = 99,
	ageYears = 111.32f
};

string bilboJson = JsonMapper.ToJson(Bilbo);
Console.WriteLine(bilboJson); // {"name":"Bilbo Baggins","gold":99,"ageYears":111.32}


int[] fibonacci = {1, 1, 2, 3, 5, 8};
string fibonacciJson = JsonMapper.ToJson(fibonacci);
Console.WriteLine(fibonacciJson); // [1,1,2,3,5,8]
```
---

Working with JsonValue
----------------------

`JsonValue` is a discriminated union type that represents a [JSON value](https://www.json.org/json-en.html).  `JsonValue` represents a value that is either a `bool`, `int`, `float`, `string`, `List<JsonValue>` or `Dictionary<string, JsonValue>`.  The `Type` parameter on a `JsonValue` instance returns an enum that indicates what the contained type is.

**Creating JsonValue instances**
`JsonValue` has a number of implicit conversion constructors, and is compatible with the C# syntactic sugar for declaring list and dictionary literals:

```C#
JsonValue boolValue = false;
JsonValue intValue = 3;
JsonValue floatValue = 3.5f;
JsonValue stringValue = "lorem ipsum";
JsonValue listValue = new JsonValue {1, 2, 3};
JsonValue objectValue = new JsonValue {{"one", 1}, {"two", 2}};

// These can be combined to create complex structure literals:
JsonValue complexValue = new JsonValue {
	{"intValue", 42},
	{"boolValue", true},
	{"twoTranslations", new JsonValue {
			{"es", "dos"},
			{"cn", "si"},
			{"jp", "ni"},
			{"fr", "deux"}
		}
	},
	{"somePrimes", new JsonValue {2, 3, 5, 7, 11}}
};
```

`JsonValue` instances also define a number of implicit conversion operators to convert to an instance of the underlying basic type.
```C#
// assuming the above JsonValue definitions...

Console.WriteLine((bool)boolValue); // false
Console.WriteLine((int)intValue); // 3
Console.WriteLine((float)floatValue); // 3.5
Console.WriteLine((string)stringValue); // lorem ipsum
```
