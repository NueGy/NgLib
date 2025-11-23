[◀ Back to Documentation Home](../README.md)[◀ Back to Documentation Home](../README.md)



# NET.HTTPCLIENT# NET.HTTPCLIENT



**Namespace:** `Nglib.NET.HTTPCLIENT`**Namespace:** `Nglib.NET.HTTPCLIENT`



## Overview## Overview



Comprehensive HTTP client wrapper for .NET applications with built-in support for authentication, attribute-based API modeling, and OAuth2 flows. Simplifies REST API consumption with automatic token management and request/response serialization.Comprehensive HTTP client wrapper for .NET applications with built-in support for authentication, attribute-based API modeling, and OAuth2 flows. Simplifies REST API consumption with automatic token management and request/response serialization.



## Key Features## Key Features



- **Attribute-Based Modeling**: Define API endpoints using `[Endpoint]` and `[EndpointParameter]` attributes- **Attribute-Based Modeling**: Define API endpoints using `[Endpoint]` and `[EndpointParameter]` attributes

- **Multiple Authentication Methods**: Bearer, Basic, OAuth2 (Client/Password), JWT HMAC, Fixed Token- **Multiple Authentication Methods**: Bearer, Basic, OAuth2 (Client/Password), JWT HMAC, Fixed Token

- **Automatic Token Management**: Built-in token expiration detection and renewal- **Automatic Token Management**: Built-in token expiration detection and renewal

- **Request/Response Helpers**: JSON serialization, validation, header management- **Request/Response Helpers**: JSON serialization, validation, header management

- **URL Utilities**: Query string building, path combination, parameter mapping- **URL Utilities**: Query string building, path combination, parameter mapping



---## Core Classes



## Core Classes### HttpClientTools



### HttpClientToolsHTTP client helper tools for making web API requests.



HTTP client helper tools for making web API requests.**Key Methods**:



**Key Methods**:- `SendWithModelAsync<T>()`: Creates an API call with request and response models

- `SendWithModelAsync<T>()`: Creates an API call with request and response models- `SetBearerToken()`: Sets Bearer token in Authorization header

- `SetBearerToken()`: Sets Bearer token in Authorization header- `SetBasicAuth()`: Sets Basic Authorization

- `SetBasicAuth()`: Sets Basic Authorization- `SetContent()`: Creates JSON content (POST: application/json, GET: querystring)

- `SetContent()`: Creates JSON content (POST: application/json, GET: querystring)- `Validate()`: Validates server response with detailed error messages

- `Validate()`: Validates server response with detailed error messages- `ReadAsync<T>()`: Deserializes JSON response

- `ReadAsync<T>()`: Deserializes JSON response

```csharp- Compatible avec l'injection de dépendances .NET

```csharp

// Simple API call with model// Simple API call with model

var response = await client.SendWithModelAsync<UserResponse>(

    HttpMethod.Get, var response = await client.SendWithModelAsync<UserResponse>(### HttpClientConfigModel

    "/api/users/123", 

    requestModel: null    HttpMethod.Get, Modèle de configuration pour l'authentification HTTP.

);

    "/api/users/123", 

// Manual request with validation

var req = HttpClientTools.PrepareRequest(HttpMethod.Post, "/api/users");    requestModel: null**Fonctionnalités :**

req.SetBearerToken("your-token");

req.SetContent(new CreateUserRequest { Name = "John" }););- Configuration centralisée des méthodes d'authentification

var resp = await client.SendAsync(req);

resp.Validate();- Support des tokens Bearer fixes, OAuth2, Basic Auth

var result = await resp.ReadAsync<UserResponse>();

```// Manual request with validation- Configuration de proxy et options SSL



### EndpointToolsvar req = HttpClientTools.PrepareRequest(HttpMethod.Post, "/api/users");- Paramètres additionnels flexibles



Builds HTTP requests from models decorated with Endpoint attributes.req.SetBearerToken("your-token");



**Key Methods**:req.SetContent(new CreateUserRequest { Name = "John" });### HttpAttributesTools

- `CreateRequestFromModel()`: Creates HttpRequestMessage from attributed model

- `SetRequestParameters()`: Maps model properties to HTTP parametersvar resp = await client.SendAsync(req);Création de requêtes HTTP via attributs sur les modèles.

- `ParseResponseParameters()`: Parses response headers back to model

resp.Validate();

```csharp

[Endpoint("GET", "/api/users/{userId}")]var result = await resp.ReadAsync<UserResponse>();**Fonctionnalités :**

public class GetUserRequest 

{```- Décoration de modèles avec HttpEndpointAttribute

    [Required]

    [EndpointParameter(HttpParameterTypeEnum.Path)]- Génération automatique de requêtes depuis les propriétés

    public int UserId { get; set; }

    ### EndpointTools- Mapping automatique des paramètres HTTP

    [EndpointParameter("filter", HttpParameterTypeEnum.Query)]

    public string Filter { get; set; }Builds HTTP requests from models decorated with Endpoint attributes.

    

    [EndpointParameter("X-Custom-Header", HttpParameterTypeEnum.Header)]### HttpTools

    public string CustomHeader { get; set; }

}**Key Methods**:Utilitaires pour manipulation d'URLs et query strings.



// Usage- `CreateRequestFromModel()`: Creates HttpRequestMessage from attributed model

var request = new GetUserRequest { UserId = 123, Filter = "active" };

var httpRequest = EndpointTools.CreateRequestFromModel(request, "https://api.example.com");- `SetRequestParameters()`: Maps model properties to HTTP parameters**Fonctionnalités :**

var response = await client.SendAsync(httpRequest);

```- `ParseResponseParameters()`: Parses response headers back to model- Combinaison d'URLs de base avec des chemins



**Parameter Types** (HttpParameterTypeEnum):- Génération de query strings depuis dictionnaires

- `Path`: Replaces `{placeholder}` in URL

- `Query`: Appends to querystring```csharp- Ajout de paramètres à des URLs existantes

- `Header`: Adds to request headers[Endpoint("GET", "/api/users/{userId}")]

- `FormData`: Sends as form-urlencodedpublic class GetUserRequest 

- `Body`: Serializes as JSON body{

    [Required]

**Validation**:    [EndpointParameter(HttpParameterTypeEnum.Path)]

- Supports `[Required]` attribute (throws if null)    public int UserId { get; set; }

- Validates path placeholders exist in URL template    

- Auto-detects parameter name from property if not specified    [EndpointParameter("filter", HttpParameterTypeEnum.Query)]

    public string Filter { get; set; }

### HttpTools    

    [EndpointParameter("X-Custom-Header", HttpParameterTypeEnum.Header)]

Various HTTP utility methods.    public string CustomHeader { get; set; }

}

**Key Methods**:

- `CombineRootUrl()`: Combines base URL with path// Usage

- `GetQueryString()`: Builds querystring from dictionaryvar request = new GetUserRequest { UserId = 123, Filter = "active" };

- `AppendQueryToUrl()`: Appends parameters to existing URLvar httpRequest = EndpointTools.CreateRequestFromModel(request, "https://api.example.com");

- `ConvertToHttpMethod()`: Converts string to HttpMethodvar response = await client.SendAsync(httpRequest);

```

```csharp

string fullUrl = HttpTools.CombineRootUrl("https://api.example.com", "/users/123");**Parameter Types** (HttpParameterTypeEnum):

// Result: https://api.example.com/users/123- `Path`: Replaces `{placeholder}` in URL

- `Query`: Appends to querystring

var queryString = HttpTools.GetQueryString(new Dictionary<string, string> {- `Header`: Adds to request headers

    { "page", "1" },- `FormData`: Sends as form-urlencoded

    { "limit", "10" }- `Body`: Serializes as JSON body

});

// Result: page=1&limit=10**Validation**:

```- Supports `[Required]` attribute (throws if null)

- Validates path placeholders exist in URL template

### HttpClientTokenHandler- Auto-detects parameter name from property if not specified



DelegatingHandler that manages authentication tokens for HttpClient.### HttpTools

Various HTTP utility methods.

**Features**:

- Automatic token expiration detection**Key Methods**:

- OAuth2 token renewal (Client Credentials, Password Grant)- `CombineRootUrl()`: Combines base URL with path

- Bearer, Basic, JWT HMAC support- `GetQueryString()`: Builds querystring from dictionary

- Proxy configuration- `AppendQueryToUrl()`: Appends parameters to existing URL

- SSL validation bypass (for dev environments)- `ConvertToHttpMethod()`: Converts string to HttpMethod



```csharp```csharp

var config = new HttpClientConfigModel {string fullUrl = HttpTools.CombineRootUrl("https://api.example.com", "/users/123");

    AuthType = TokenAuthTypeEnum.OAuth2Client,// Result: https://api.example.com/users/123

    ClientId = "your-client-id",

    ClientSecret = "your-secret",var queryString = HttpTools.GetQueryString(new Dictionary<string, string> {

    TokenEndpointUrl = "https://auth.example.com/token"    { "page", "1" },

};    { "limit", "10" }

});

var handler = new HttpClientTokenHandler { Config = config };// Result: page=1&limit=10

var client = new HttpClient(handler);```

client.BaseAddress = new Uri("https://api.example.com");

### HttpClientTokenHandler

// Token is automatically managedDelegatingHandler that manages authentication tokens for HttpClient.

var response = await client.GetAsync("/api/protected-resource");

```**Features**:

- Automatic token expiration detection

### HttpClientConfigModel- OAuth2 token renewal (Client Credentials, Password Grant)

- Bearer, Basic, JWT HMAC support

Configuration model for authentication.- Proxy configuration

- SSL validation bypass (for dev environments)

**Properties**:

- `BaseUrl`: Base URL for API```csharp

- `AuthType`: Authentication method (see TokenAuthTypeEnum)var config = new HttpClientConfigModel {

- `Username/Password`: For Basic or OAuth2 Password grant    AuthType = TokenAuthTypeEnum.OAuth2Client,

- `ClientId/ClientSecret`: For OAuth2 Client Credentials    ClientId = "your-client-id",

- `TokenEndpointUrl`: OAuth2 token endpoint    ClientSecret = "your-secret",

- `FixedToken`: Pre-defined token    TokenEndpointUrl = "https://auth.example.com/token"

- `ProxyUrl`: Proxy configuration};

- `DisableSslValidation`: Disable SSL validation (dev only)

var handler = new HttpClientTokenHandler { Config = config };

```csharpvar client = new HttpClient(handler);

// Fixed Bearer Tokenclient.BaseAddress = new Uri("https://api.example.com");

var config = HttpClientConfigModel.PrepareWithFixedToken(

    "your-bearer-token", // Token is automatically managed

    "https://api.example.com"var response = await client.GetAsync("/api/protected-resource");

);```



// OAuth2 Client Credentials### HttpClientConfigModel

var config = new HttpClientConfigModel(TokenAuthTypeEnum.OAuth2Client) {Configuration model for authentication.

    ClientId = "app-id",

    ClientSecret = "app-secret",**Properties**:

    TokenEndpointUrl = "https://auth.example.com/oauth2/token",- `BaseUrl`: Base URL for API

    BaseUrl = "https://api.example.com"- `AuthType`: Authentication method (see TokenAuthTypeEnum)

};- `Username/Password`: For Basic or OAuth2 Password grant

```- `ClientId/ClientSecret`: For OAuth2 Client Credentials

- `TokenEndpointUrl`: OAuth2 token endpoint

### HttpClientTokenTools- `FixedToken`: Pre-defined token

- `ProxyUrl`: Proxy configuration

Extension methods for token management.- `DisableSslValidation`: Disable SSL validation (dev only)



**Key Methods**:```csharp

- `IsTokenExpired()`: Checks if token has expired// Fixed Bearer Token

- `SetToken()`: Updates token with expirationvar config = HttpClientConfigModel.PrepareWithFixedToken(

- `RefreshTokenOAuth2Async()`: Obtains new token via OAuth2 flow    "your-bearer-token", 

    "https://api.example.com"

```csharp);

if (handler.IsTokenExpired()) {

    await handler.RefreshTokenOAuth2Async();// OAuth2 Client Credentials

}var config = new HttpClientConfigModel(TokenAuthTypeEnum.OAuth2Client) {

    ClientId = "app-id",

// Manual token update    ClientSecret = "app-secret",

handler.SetToken("new-access-token", expireIn: 3600);    TokenEndpointUrl = "https://auth.example.com/oauth2/token",

```    BaseUrl = "https://api.example.com"

};

---```



## Authentication Methods (TokenAuthTypeEnum)### HttpClientTokenTools

Extension methods for token management.

### None

No authentication, uses LastToken if available.**Key Methods**:

- `IsTokenExpired()`: Checks if token has expired

### Basic- `SetToken()`: Updates token with expiration

Standard HTTP Basic authentication with username/password.- `RefreshTokenOAuth2Async()`: Obtains new token via OAuth2 flow



### OAuth2Client```csharp

Standard OAuth2 Client Credentials flow.if (handler.IsTokenExpired()) {

```    await handler.RefreshTokenOAuth2Async();

POST /token}

Content-Type: application/x-www-form-urlencoded

// Manual token update

grant_type=client_credentialshandler.SetToken("new-access-token", expireIn: 3600);

&client_id=your-client-id```

&client_secret=your-secret

```## Authentication Methods (TokenAuthTypeEnum)



### OAuth2Password### None

OAuth2 Resource Owner Password Credentials flow.No authentication, uses LastToken if available.

```

POST /token### Basic

Content-Type: application/x-www-form-urlencodedStandard HTTP Basic authentication with username/password.



grant_type=password### OAuth2Client

&username=user@example.comStandard OAuth2 Client Credentials flow.

&password=userpassword```csharp

&client_id=your-client-id (optional)POST /token

```Content-Type: application/x-www-form-urlencoded



### JwtHmacgrant_type=client_credentials

Signs a JWT HS256 with secret key (SOON - Not implemented yet).&client_id=your-client-id

&client_secret=your-secret

### FixedBearerToken```

Uses a pre-defined token from configuration.

### OAuth2Password

---OAuth2 Resource Owner Password Credentials flow.

```csharp

## AttributesPOST /token

Content-Type: application/x-www-form-urlencoded

### [Endpoint]

Defines HTTP endpoint for API client requests.grant_type=password

&username=user@example.com

**Parameters**:&password=userpassword

- `method`: HTTP method (GET, POST, PUT, DELETE, PATCH)&client_id=your-client-id (optional)

- `path`: URL path with optional placeholders (`/api/users/{id}`)```

- `Description`: Optional documentation

### JwtHmac

### [EndpointParameter]Signs a JWT HS256 with secret key (SOON - Not implemented yet).

Maps property to HTTP request parameter.

### FixedBearerToken

**Parameters**:Uses a pre-defined token from configuration.

- `name`: Parameter name (defaults to property name)

- `type`: HttpParameterTypeEnum (Path, Query, Header, FormData, Body)## Attributes

- `StringFormat`: Format string for serialization (e.g., "yyyy-MM-dd")

### [Endpoint]

---Defines HTTP endpoint for API client requests.



## Advanced Examples**Parameters**:

- `method`: HTTP method (GET, POST, PUT, DELETE, PATCH)

### Complete API Wrapper with Endpoint Attributes- `path`: URL path with optional placeholders (`/api/users/{id}`)

```csharp- `Description`: Optional documentation

// Define request models

[Endpoint("GET", "/api/v2/products/{productId}")]### [EndpointParameter]

public class GetProductRequest {Maps property to HTTP request parameter.

    [Required]

    [EndpointParameter(HttpParameterTypeEnum.Path)]**Parameters**:

    public int ProductId { get; set; }- `name`: Parameter name (defaults to property name)

    - `type`: HttpParameterTypeEnum (Path, Query, Header, FormData, Body)

    [EndpointParameter("include_reviews", HttpParameterTypeEnum.Query)]- `StringFormat`: Format string for serialization (e.g., "yyyy-MM-dd")

    public bool IncludeReviews { get; set; }

}## Advanced Examples



[Endpoint("POST", "/api/v2/products")]### Complete API Wrapper with Endpoint Attributes

public class CreateProductRequest {```csharp

    [EndpointParameter(HttpParameterTypeEnum.Body)]// Define request models

    public Product Product { get; set; }[Endpoint("GET", "/api/v2/products/{productId}")]

    public class GetProductRequest {

    [EndpointParameter("X-Idempotency-Key", HttpParameterTypeEnum.Header)]    [Required]

    public string IdempotencyKey { get; set; }    [EndpointParameter(HttpParameterTypeEnum.Path)]

}    public int ProductId { get; set; }

    

// Use in API wrapper    [EndpointParameter("include_reviews", HttpParameterTypeEnum.Query)]

public class ProductApiClient {    public bool IncludeReviews { get; set; }

    private readonly HttpClient _client;}

    

    public ProductApiClient(HttpClient client) {[Endpoint("POST", "/api/v2/products")]

        _client = client;public class CreateProductRequest {

    }    [EndpointParameter(HttpParameterTypeEnum.Body)]

        public Product Product { get; set; }

    public async Task<ProductResponse> GetProductAsync(int productId, bool includeReviews = false) {    

        var request = new GetProductRequest {     [EndpointParameter("X-Idempotency-Key", HttpParameterTypeEnum.Header)]

            ProductId = productId,     public string IdempotencyKey { get; set; }

            IncludeReviews = includeReviews }

        };

        var httpRequest = EndpointTools.CreateRequestFromModel(request);// Use in API wrapper

        var response = await _client.SendAsync(httpRequest);public class ProductApiClient {

        response.Validate();    private readonly HttpClient _client;

        return await response.ReadAsync<ProductResponse>();    

    }    public ProductApiClient(HttpClient client) {

            _client = client;

    public async Task<ProductResponse> CreateProductAsync(Product product) {    }

        var request = new CreateProductRequest {     

            Product = product,    public async Task<ProductResponse> GetProductAsync(int productId, bool includeReviews = false) {

            IdempotencyKey = Guid.NewGuid().ToString()        var request = new GetProductRequest { 

        };            ProductId = productId, 

        var httpRequest = EndpointTools.CreateRequestFromModel(request);            IncludeReviews = includeReviews 

        var response = await _client.SendAsync(httpRequest);        };

        response.Validate();        var httpRequest = EndpointTools.CreateRequestFromModel(request);

        return await response.ReadAsync<ProductResponse>();        var response = await _client.SendAsync(httpRequest);

    }        response.Validate();

}        return await response.ReadAsync<ProductResponse>();

```    }

    

### OAuth2 with HttpClientFactory    public async Task<ProductResponse> CreateProductAsync(Product product) {

```csharp        var request = new CreateProductRequest { 

// Startup configuration            Product = product,

services.AddHttpClient<IMyApiClient, MyApiClient>()            IdempotencyKey = Guid.NewGuid().ToString()

    .ConfigurePrimaryHttpMessageHandler(() => {        };

        var config = new HttpClientConfigModel {        var httpRequest = EndpointTools.CreateRequestFromModel(request);

            AuthType = TokenAuthTypeEnum.OAuth2Client,        var response = await _client.SendAsync(httpRequest);

            ClientId = Configuration["Api:ClientId"],        response.Validate();

            ClientSecret = Configuration["Api:ClientSecret"],        return await response.ReadAsync<ProductResponse>();

            TokenEndpointUrl = Configuration["Api:TokenUrl"]    }

        };}

        return new HttpClientTokenHandler { Config = config };```

    });

### OAuth2 with HttpClientFactory

// Service implementation```csharp

public class MyApiClient : IMyApiClient {// Startup configuration

    private readonly HttpClient _client;services.AddHttpClient<IMyApiClient, MyApiClient>()

        .ConfigurePrimaryHttpMessageHandler(() => {

    public MyApiClient(HttpClient client) {        var config = new HttpClientConfigModel {

        _client = client;            AuthType = TokenAuthTypeEnum.OAuth2Client,

        _client.BaseAddress = new Uri("https://api.example.com");            ClientId = Configuration["Api:ClientId"],

    }            ClientSecret = Configuration["Api:ClientSecret"],

                TokenEndpointUrl = Configuration["Api:TokenUrl"]

    public async Task<User[]> GetUsersAsync() {        };

        return await _client.SendWithModelAsync<User[]>(        return new HttpClientTokenHandler { Config = config };

            HttpMethod.Get,     });

            "/api/users"

        );// Service implementation

    }public class MyApiClient : IMyApiClient {

}    private readonly HttpClient _client;

```    

    public MyApiClient(HttpClient client) {

---        _client = client;

        _client.BaseAddress = new Uri("https://api.example.com");

## Best Practices    }

    

1. **Use HttpClientFactory**: Always use `IHttpClientFactory` in production instead of manual HttpClient creation    public async Task<User[]> GetUsersAsync() {

2. **Token Security**: Never hardcode tokens or secrets in code        return await _client.SendWithModelAsync<User[]>(

3. **Validation**: Always call `.Validate()` on responses for detailed error messages            HttpMethod.Get, 

4. **Attribute Models**: Use `[Endpoint]` attributes for type-safe API definitions            "/api/users"

5. **SSL in Production**: Only use `DisableSslValidation = true` in development        );

6. **Error Handling**: Wrap API calls in try-catch to handle network errors    }

}

---```



## Related Components## Best Practices

- [APP.CODE](wiki_components_appcode.md): Reflection and property tools

- [FORMAT](wiki_components_format.md): String and date formatting utilities1. **Use HttpClientFactory**: Always use `IHttpClientFactory` in production instead of manual HttpClient creation

- [DATA.ACCESSORS](wiki_components_accessors.md): Data accessor patterns2. **Token Security**: Never hardcode tokens or secrets in code

3. **Validation**: Always call `.Validate()` on responses for detailed error messages

## Notes4. **Attribute Models**: Use `[Endpoint]` attributes for type-safe API definitions

- Some obsolete warnings exist (e.g., `JsonSerializerOptions.IgnoreNullValues`) - will be fixed in future versions5. **SSL in Production**: Only use `DisableSslValidation = true` in development

- `OAuthModels` class is marked obsolete6. **Error Handling**: Wrap API calls in try-catch to handle network errors

- JWT HMAC authentication not yet implemented

## Related Components
- **APP.CODE**: Reflection and property tools
- **FORMAT**: String and date formatting utilities
- **DATA.BASICS**: Data accessor patterns

## Notes
- Some obsolete warnings exist (e.g., `JsonSerializerOptions.IgnoreNullValues`) - will be fixed in future versions
- `OAuthModels` class is marked obsolete
- JWT HMAC authentication not yet implemented
