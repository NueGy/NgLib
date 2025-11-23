[◀ Back to Documentation Home](../README.md)

# DATA.DATAMODEL

**Namespace:** `Nglib.DATA.DATAMODEL`

Component for automatic HTML form generation from API data models. Enables advanced model manipulation with metadata for dynamic user interfaces.

## Main Features

- **Complex Models**: Encapsulation of API models with display metadata
- **Form Generation**: Automatic support for fields, validations and display options
- **REST API Wrapper**: Complete CRUD with KeyValues and JSON serialization
- **Attribute Configuration**: Configuration metadata via `DataModelConfigAttribute`
- **Typed Collections**: DataModel list manipulation with generic constraints

## Main Classes

### DataModel<TModel>
Main container associating a data model (`TModel`) with its display metadata (`FormValues`, `InfoValues`).

### ModelValue
Field descriptor for HTML generation: type, validation, style, possible values.

### DataModelApiWrapper<TApiModel>
HTTP client wrapper for CRUD operations on REST APIs with KeyValues and standard JSON support.

### DataModelConfigAttribute
Class attribute defining API URL, labels, icons and front-end configuration.