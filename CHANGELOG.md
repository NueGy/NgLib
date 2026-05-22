# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Added
- **QueryBuilder**: `From()` method now supports table aliases via optional second parameter
- **QueryBuilder**: Added native interface methods `WhereEqual()`, `WhereNotEqual()`, `WhereGreater()`, `WhereLess()`, `WhereLike()` for simplified WHERE clauses
- **QueryBuilder**: New `TableAlias` property in `QueryBuilderContext` to store table alias
- **ValidateModel**: Added implicit conversion operator to `bool` for natural usage in conditions

### Changed
- **QueryBuilder**: `BuildFromAndJoins()` now includes alias in FROM clause when specified
- **QueryBuilder**: Improved IntelliSense visibility for common WHERE operations
- **ListResult**: Property `info` renamed to `Info` following C# naming conventions (PascalCase)
- **HttpClientTools**: `Validate()` now throws `HttpRequestException` instead of generic `Exception` with response details in `ex.Data`
- **IDataPOFlow**: Now inherits from `IDataAccessor` for standardized data access

### Removed
- **QueryBuilder**: Removed redundant extension methods that were calling native interface methods with same signature

### Fixed
- **ListResult**: Corrected property naming convention from `info` to `Info`
- **HttpClientTools**: Better exception handling with specific `HttpRequestException` type and diagnostic data
- **IDataPOFlow**: Architectural consistency by inheriting `IDataAccessor`

### Documentation
- Updated `wiki_components_querybuilder.md` with simplified table-based reference
- Added test coverage for table aliases and native WHERE methods
- Added test coverage for ValidateModel implicit bool conversion
- Added test coverage for ListResult property naming

## [1.1.7-beta] - 2026-05-21

### Changed
- Documentation translated to English
- Repository made AI-Ready with improved Copilot instructions

## [1.1.6] - Previous releases

### Added
- Core library components (FORMAT, DATA, APP)
- Nglib.Formula package
- Comprehensive test suite

