---
topic: ASP.NET Minimal API
slug: aspnet-minimal-api
status: notes
sessions: [2026-03-17, 2026-03-18, 2026-03-19]
---

## 2026-03-17

### WriteIndented for dev-time JSON formatting [`decision`]

Browser shows raw, unformatted JSON by default when hitting API endpoints. Added `WriteIndented = true` to `ConfigureHttpJsonOptions` as a temporary convenience during development. This will be removed later when Swagger UI or similar tooling is added, since pretty-printing adds extra bytes to responses in production.

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
});
```

### CORS setup for frontend-backend communication [`concept`]

When a React app (e.g. `localhost:3000`) tries to fetch from an API on a different origin (e.g. `localhost:7254`), the browser blocks it by default — this is a security feature called Same-Origin Policy. CORS (Cross-Origin Resource Sharing) tells the API to explicitly allow requests from other origins.

Using `AllowAnyOrigin()` for development; in production you'd restrict to specific domains.

### Middleware ordering matters [`pattern`]

ASP.NET processes middleware in the order you add it. CORS must come before endpoint mappings or it won't apply. The correct order is:

1. `app.UseHttpsRedirection();`
2. `app.UseCors();`
3. `app.MapGet(...)` endpoints
4. `app.Run();`

If you put `UseCors()` after the endpoints, CORS headers won't be added to responses and the browser will still block cross-origin requests.

### Enum serialization — numbers vs strings [`decision`]

ASP.NET serializes enums as numbers by default (e.g. `"mode": 2`). Added `JsonStringEnumConverter` to `ConfigureHttpJsonOptions` so the API returns human-readable strings (`"mode": "Remote"`). Self-documenting API responses are better for consumers.

### Route grouping with MapGroup [`pattern`]

Instead of repeating `/cv` in every endpoint, `app.MapGroup("/cv")` creates a group with a shared prefix. All endpoints inside automatically get `/cv` prepended. Cleaner code, same behaviour.

```csharp
var cvGroup = app.MapGroup("/cv");
cvGroup.MapGet("/identity", ...);  // becomes GET /cv/identity
```

### Root endpoint as API index [`tip`]

Added a `GET /` endpoint that returns a JSON object listing all available endpoints. Inspired by HATEOAS (REST principle where APIs describe their own navigation). Used an anonymous object (`new { ... }`) — a quick throwaway object without defining a class.

### Choosing IDs over indexes for route parameters [`decision`]

For endpoints like `/cv/experiences/{id}`, we chose Guid IDs over array indexes. Indexes are fragile (reordering changes what `/experiences/0` returns) and meaningless. GUIDs are stable and unique. Added `Guid Id` to Experience, Education, and Project models. MongoDB will generate IDs automatically later.

### Dependency Injection (DI) [`concept`]

Instead of endpoints directly accessing a local `person` variable, created a `CvService` class registered as a singleton. Endpoints declare `CvService cv` as a parameter and ASP.NET provides it automatically. Benefits: separation of concerns (service handles data, endpoints handle HTTP), easier to swap implementations later (e.g. JSON file → MongoDB), and testable.

```csharp
// Register
builder.Services.AddSingleton<CvService>();

// Inject — ASP.NET passes it automatically
cvGroup.MapGet("/identity", (CvService cv) => cv.GetIdentity());
```

### Results.Ok and Results.NotFound for proper status codes [`pattern`]

When an endpoint can return different HTTP status codes (200 or 404), use `Results.Ok()` and `Results.NotFound()` instead of returning the object directly. The return type becomes `IResult` which can represent any HTTP response.

[Screenshot: aspnet-api-base-endpoint.png]
[Screenshot: aspnet-api-first-cv-endpoint.png]

## 2026-03-18

### Scalar — modern OpenAPI UI [`concept`]

ASP.NET already had OpenAPI document generation built in (`AddOpenApi()` + `MapOpenApi()` from the template), which serves the raw JSON spec at `/openapi/v1.json`. But to get an interactive UI, you need a separate package. Scalar is Microsoft's modern recommendation (replacing Swagger UI for .NET 9+). Installed `Scalar.AspNetCore`, added `using Scalar.AspNetCore` and `app.MapScalarApiReference()` next to `MapOpenApi()`. UI available at `/scalar/v1`.

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();           // serves the JSON spec
    app.MapScalarApiReference(); // serves the interactive UI
}
```

### Response type annotations for OpenAPI [`pattern`]

Without annotations, the OpenAPI spec doesn't know what types your endpoints return — Scalar shows vague responses. Adding `.Produces<T>()` tells OpenAPI the exact return type so the UI shows full schemas. For endpoints that can return 404, chain both `.Produces<T>()` and `.Produces(StatusCodes.Status404NotFound)`. `.WithSummary("...")` adds a human-readable description, and `.WithTags("CV")` on the route group groups all endpoints together in the UI.

```csharp
var cvGroup = app.MapGroup("/cv").WithTags("CV");

cvGroup.MapGet("/experiences/{id}", (Guid id, CvService cv) => { ... })
    .Produces<Experience>()
    .Produces(StatusCodes.Status404NotFound)
    .WithSummary("Get a specific work experience by ID");
```

## 2026-03-19

### Expression lambdas vs block lambdas [`tip`]

C# expression lambdas (`() => expr`) can only contain a single expression, not statements. `throw` is a statement, so `() => throw new Exception()` won't compile — the compiler sees a delegate that takes 0 arguments and complains. Use a block lambda with curly braces instead: `() => { throw new Exception(); }`. Same applies to other statements like `if`, `for`, variable declarations, etc.

```csharp
// Won't compile — throw is a statement, not an expression
app.MapGet("/test", () => throw new Exception("boom"));

// Works — block lambda allows statements
app.MapGet("/test", () => { throw new Exception("boom"); });
```

### Custom middleware vs UseExceptionHandler [`decision`]

Built a custom `ErrorHandlingMiddleware` class to learn how middleware works — a class with a `RequestDelegate _next` constructor param and an `InvokeAsync(HttpContext context)` method that wraps `await _next(context)` in a try/catch. This teaches the middleware pattern used for all cross-cutting concerns (logging, auth, timing, etc.).

Then swapped it for ASP.NET's built-in `app.UseExceptionHandler()`, which does the same thing but is battle-tested and handles edge cases like `Response.HasStarted` automatically. For real projects, prefer the built-in — you get the same result with less code to maintain. The custom approach is valuable for learning, not for production.

```csharp
// Built-in — preferred for real projects
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "An unexpected error occurred. Please try again later."
        });
    });
});
```
