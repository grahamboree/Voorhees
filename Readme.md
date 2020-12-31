Voorhees
========

*A terrifyingly fast C# JSON micro-framework*

---

Current Status
------
[![Build Status](https://travis-ci.org/grahamboree/voorhees.svg?branch=master)](https://travis-ci.org/grahamboree/voorhees)[![Build status](https://ci.appveyor.com/api/projects/status/6nl98purvr2186iw?svg=true)](https://ci.appveyor.com/project/grahamboree/voorhees)[![Coverage Status](https://coveralls.io/repos/github/grahamboree/voorhees/badge.svg?branch=master)](https://coveralls.io/github/grahamboree/voorhees?branch=master)

**Features**
* Read and write json data into a structured object graph with minimal allocations.
* Map complex objects to and from json values

**In-Development**
* Multidimensional array mapping support

**Planned**
* Change mapping de-serialization to combine reading and mapping so it's not allocating a bunch of intermediate JsonValue's 
* Serialize and de-serialize correct underlying value for polymorphic type references
* Field and Property annotations to ignore when serializing

Why
---

Why do we need another C# JSON library in 2020?  The simple answer is, we don't really.  Voorhees was created as a distraction from 2020, but it's also designed specifically with game development in mind.  This means a specific focus on memory and CPU performance, along with the ability to control 

