---
topic: Docker & Render Deployment
slug: docker-deployment
status: notes
sessions: [2026-04-01, 2026-04-02]
---

## 2026-04-01

### What is Docker and why do we need it? [`concept`]

Render doesn't have native .NET support, so we need Docker to package our app. Key concepts: **images** are the blueprint (like a recipe), **containers** are running instances (like a served dish). A Dockerfile defines layers — each instruction creates a cacheable layer.

### Multi-stage builds [`pattern`]

Our Dockerfile uses two stages: (1) SDK image to build/publish, (2) smaller ASP.NET runtime image to run. This keeps the final image small — no build tools shipped to production.

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/CvApi/CvApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000
ENTRYPOINT ["dotnet", "CvApi.dll"]
```

### .NET version mismatch in Dockerfile [`mistake`]

First build failed because our projects target .NET 10.0 but the Dockerfile referenced 9.0 images. Fix: update both `FROM` lines to use `10.0`. Lesson: the Dockerfile SDK/runtime versions must match your project's target framework.

## 2026-04-02

### Security when running containers [`concept`]

Several security lessons learned while running the container locally:

- **Never pass secrets directly in CLI commands** — they get saved in shell history. Use env files or secret managers instead.
- **Environment variables (`-e` flag)** are the standard way to pass config into Docker containers. .NET picks these up automatically and they override `appsettings.json` using `__` (double underscore) as a separator (e.g. `MongoDB__ConnectionString` maps to `MongoDB.ConnectionString`).
- **If you accidentally expose a secret** (connection string, password, API key), **rotate it immediately**. Don't assume it's safe just because you deleted the message — it may be cached or logged.

### PowerShell vs Bash line continuation [`tip`]

Multi-line Docker commands use `\` for line continuation in bash but `` ` `` (backtick) in PowerShell. If you get "invalid reference format" errors in PowerShell, this is likely the cause. Safest option: put the whole command on one line.

### Docker container naming [`tip`]

Docker assigns random names (like `compassionate-pike`) if you don't specify `--name`. Use `--name cv-api-container` to give it a meaningful name.
