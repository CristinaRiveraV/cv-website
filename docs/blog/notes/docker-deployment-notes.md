---
topic: Docker & Render Deployment
slug: docker-deployment
status: notes
sessions: [2026-04-01, 2026-04-02, 2026-04-07]
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

Docker assigns random names (like `compassionate-pike`) if you don't specify `--name`. Use `--name cv-api-container` to give it a meaningful name. Use `--rm` to auto-delete the container when it stops — great for local testing so you don't accumulate stopped containers.

### .dockerignore — what to exclude and why [`concept`]

Works just like `.gitignore` but for Docker builds. Without it, `COPY . .` sends everything to the Docker engine — `.git/`, `node_modules/`, docs, even `appsettings.Development.json` with your real secrets. Our `.dockerignore` excludes: `.git`, `.github`, `.claude`, `docs`, `node_modules`, `*.md`, `appsettings.Development.json`, `bin`, `obj`. Benefits: faster builds, smaller build context, and secrets don't get baked into the image.

### Development-only middleware [`concept`]

Scalar (OpenAPI UI) returned 404 inside the container because it's wrapped in `if (app.Environment.IsDevelopment())`. In Docker, the default environment is **Production**. This is intentional — you don't want API docs publicly exposed. You can override with `-e ASPNETCORE_ENVIRONMENT=Development` for local testing.

### Deploying to Render [`concept`]

Render is a cloud platform that can build and run Docker containers directly from a GitHub repo. Key configuration:

- **Web Service** type for running containers
- Connect your GitHub repo — Render pulls code and builds the image automatically
- **Environment variables** set in the Render dashboard (not in code) for secrets like `MongoDB__ConnectionString`
- **Port** must match the `EXPOSE` in your Dockerfile (10000)
- **Auto-deploy** triggers a new build on every push to the configured branch
- **Health Check Path** — leave blank if your API doesn't have a `/healthz` endpoint, otherwise Render thinks your service is unhealthy

### Render deployment gotchas [`mistake`]

First deploy failed for two reasons: (1) branch was set to `main` but the Dockerfile only existed on `feature/docker-deployment`, and (2) the Dockerfile path was `.dockerfile` instead of `./Dockerfile` (capital D matters). The build log said `transferring dockerfile: 2B` — the tiny size was a clue it found the wrong file.

## 2026-04-07

### Testing a live API with Insomnia [`tip`]

Used Insomnia (similar to Postman) to test the deployed API. To test authenticated endpoints: go to your Auth0 Dashboard → APIs → CV Website API → Test tab, copy the access token, then add it in Insomnia's Auth/Headers tab as `Authorization: Bearer <token>`. This is the same flow you'd use with any HTTP client — curl, Postman, Insomnia, etc.

### No root endpoint = expected 404 [`concept`]

The root `/` returned 404, which initially looks like the deployment is broken. But it's simply because no endpoint is mapped there — only the `/cv` route group exists. Lesson: a 404 doesn't always mean the deployment failed. Check what endpoints you actually defined before assuming something is wrong.

### End-to-end verification checklist [`pattern`]

Testing the live deployment confirmed multiple layers working together in production:
- **Docker container** running correctly on Render
- **MongoDB Atlas** connection working from the cloud (real CV data returned)
- **Auth0 JWT validation** working (401 without token, 200 with valid token)

This is a good sanity checklist for any deployment: test each integration point, not just "does it respond."

### Render free tier cold starts [`concept`]

Free tier services on Render spin down after a period of inactivity. The first request after idle time takes noticeably longer (a "cold start") while the container spins back up. This is normal for free tier — paid tiers keep the service running. Something to be aware of when demoing or sharing the URL.
