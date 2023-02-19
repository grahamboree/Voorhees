# 🔪 Voorhees

<p align="center">
	<i>A terrifyingly fast C# JSON framework with a focus CPU performance and minimizing allocations</i>	
</p>

[![Build & Test](https://github.com/grahamboree/Voorhees/actions/workflows/build.yaml/badge.svg)](https://github.com/grahamboree/Voorhees/actions/workflows/build.yaml)[![Coverage Status](https://coveralls.io/repos/github/grahamboree/Voorhees/badge.svg?branch=main)](https://coveralls.io/github/grahamboree/Voorhees?branch=main)[![CodeFactor](https://www.codefactor.io/repository/github/grahamboree/voorhees/badge)](https://www.codefactor.io/repository/github/grahamboree/voorhees)

## Features

* Reads and writes json data into a structured object graph with minimal allocations.
* High level API is one line of code to read or write JSON data.
* Low level API reads from and writes to text streams. Seamlessly integrates with cryptographic streams and can read and write data directly to and from sockets, files, etc.
* Maps complex objects to and from json values automatically without needing any annotations, markup, or codegen.
* Fully supports reading and writing multi-dimensional arrays, unlike other popular C# JSON libraries.
* Correctly serializes polymorphic type references.
* Optional value annotiation instructs Voorhees to ignore specific parameters and fields when serializing objects.
* Supports completely custom serializers/deserializers for arbitray types for full customizability of the JSON reading and writing process.
* Generates either pretty-printed JSON or condensed JSON.

---

## Reading Json Data

You can read arbitrary json data into a `JsonValue` structure using the `JsonMapper.FromJson` method.  This method operates on `TextReader` streams, allowing for reading from strings, files, network messages, and more with a shared inteface.

```C#
using Voorhees;

// Load a json string into a JsonValue object graph.
JsonValue jsonValue = JsonMapper.FromJson<JsonValue>("{ \"someIntValue\": 3}");

Console.WriteLine(jsonValue.Type); // JsonValueType.Object
Console.WriteLine((int)jsonValue["someIntValue"]); // 3
```

You can also map json data to a matching C# structure using the generic version of `JsonMapper.FromJson<T>`.  This is most useful for reading in structured data into existing corresponding C# types.

```C#
using Voorhees;

struct ExampleStructure {
	public int someIntValue;
}

TextReader json = new StringReader("{ \"someIntValue\": 3}");
ExampleStructure mappedValue = JsonMapper.FromJson<ExampleStructure>(json);

Console.WriteLine(mappedValue.someIntValue); // 3
```

`JsonMapper` supports loading values into built-in collection types like `Dictionary<T, U>`, `List<T>`, arrays, and more:

```C#
string dictJson = "{\"one\": 1, \"two\": 2}";
Dictionary<string, int> numberNamesToInts = JsonMapper.FromJson<Dictionary<string, int>>(dictJson);
Console.WriteLine(numberNamesToInts.Count); // 2
Console.WriteLine(numberNamesToInts["one"]); // 1
Console.WriteLine(numberNamesToInts["two"]); // 2

List<int> mappedList = JsonMapper.FromJson<List<int>>("[1, 2, 3]");
Console.WriteLine(mappedList.Count); // 3
Console.WriteLine(mappedList[0]); // 1
Console.WriteLine(mappedList[1]); // 2
Console.WriteLine(mappedList[2]); // 3

int[] mappedArray = JsonMapper.FromJson<int[]>("[1, 2, 3]");
Console.WriteLine(mappedArray.Length); // 3
Console.WriteLine(mappedArray[0]); // 1
Console.WriteLine(mappedArray[1]); // 2
Console.WriteLine(mappedArray[2]); // 3
```

## Writing Json Data

You can convert a JsonValue object into the matching JSON string using `JsonMapper`:

```C#
using Voorhees;

JsonValue objectValue = new JsonValue {
	{ "one", 1 },
	{ "two", 2 },
	{ "three", 3 }
};
string jsonObject = JsonMapper.ToJson(objectValue);
Console.WriteLine(jsonObject); // {"one":1,"two":2,"three":3}

JsonValue arrayValue = new JsonValue {1, 2, 3};
string jsonArray = JsonMapper.ToJson(arrayValue);
Console.WriteLine(jsonObject); // [1,2,3]
```

`JsonMapper` also supports mapping arbitrary C# data types in addition to `JsonValue`.

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

By default the JSON generated by JsonMapper does not have any whitespace.  This makes it as dense and small as possible, but not very convenient to read.  To generate more readable JSON results, you can optionally enable pretty-printing.

```C#
using Voorhees;

JsonValue objectValue = new JsonValue {
	{ "one", 1 },
	{ "two", 2 },
	{ "three", 3 }
};
string jsonObject = JsonMapper.ToJson(objectValue, prettyPrint: true);
Console.WriteLine(jsonObject);
/* Prints the following:
{
	"one": 1,
	"two": 2,
	"three": 3
}
*/
```

## Working with JsonValue

`JsonValue` is a discriminated union type that represents a [JSON value](https://www.json.org/json-en.html).  `JsonValue` represents a value that is either a `bool`, `int`, `double`, `string`, `List<JsonValue>` or `Dictionary<string, JsonValue>`.  The `Type` parameter on a `JsonValue` instance returns an enum that indicates what the contained type is.

**Creating JsonValue instances**
`JsonValue` has a number of implicit conversion constructors, and is compatible with the C# syntactic sugar for declaring list and dictionary literals:

```C#
JsonValue boolValue = false;
JsonValue intValue = 3;
JsonValue doubletValue = 3.5;
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
Console.WriteLine((double)doubleValue); // 3.5
Console.WriteLine((string)stringValue); // lorem ipsum
```
