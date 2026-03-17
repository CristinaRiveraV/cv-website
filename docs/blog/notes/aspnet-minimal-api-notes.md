---
topic: ASP.NET Minimal API
slug: aspnet-minimal-api
status: notes
sessions: [2026-03-17]
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
