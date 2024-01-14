# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Static `ToJson` and `FromJson` methods to `JsonMapper` that operate on streams

### Fixed

- Skip boxing numeric value types when calling `JsonMapper.Write<T>`
- Removed calls from JsonTokenWriter to .NET methods that perform allocations internally
- Reduced the size of stack-allocated string buffers for reading in numeric data types

## [1.0.0] - 2024-01-13

Initial release

### Added

- `JsonMapper` for object <-> json mapping
- `JsonTokenReader` and `JsonTokenWriter` for reading and writing json data tokens
- `JsonValue` as a generic, type for operating with json data

[unreleased]: https://github.com/grahamboree/Voorhees/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/grahamboree/Voorhees/releases/tag/v1.0.0