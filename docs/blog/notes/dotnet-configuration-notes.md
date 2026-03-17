---
topic: ".NET Configuration — Keeping Secrets Out of GitHub"
slug: dotnet-configuration
status: notes
sessions: [2026-03-11, 2026-03-13, 2026-03-16]
---

## 11 March 2026

### Keeping personal info out of GitHub — 4 options [`decision`]

Discussed four approaches for keeping real CV data (email, phone, etc.) out of the repo:
1. Environment variables
2. User secrets (`dotnet user-secrets`)
3. appsettings + .gitignore (chosen)
4. External file outside repo

Chose **appsettings + .gitignore** — simplest approach, keeps data alongside the code for easy loading, and a committed template file shows the expected structure without real values.

### Template file pattern [`pattern`]

Two files:
- `appsettings.Development.json` — real data, gitignored
- `appsettings.Development.template.json` — committed, with placeholder values showing the expected shape

This way anyone cloning the repo can see what config is needed and create their own copy.

### Config file location — .NET convention [`tip`]

Config files go in the project root (next to `.csproj`), not in a subfolder. This is .NET convention and where `ConfigurationBuilder` looks by default.

### Template JSON needs fixes before wiring up [`tip`]

The template JSON drifted from the C# models during development. Fixes needed:
- Skills: add `Category` field, rename `ProficiencyLevel` → `Proficiency`
- Languages: use enum strings (Native/Fluent/etc.) not numbers
- Projects: remove `Technologies` (redundant with Skills), rename `Link` → `Url`, add `RepoUrl`, `ImageUrl`, `StartDate`, `EndDate`
- Education: `StartYear`/`EndYear` should be numbers not strings
- Remove extra closing brace at end of file

## 13 March 2026

### SetBasePath — working directory vs app directory [`mistake`]

`SetBasePath(Directory.GetCurrentDirectory())` uses wherever you *run* the command from, not where the app lives. Running `dotnet run` from the repo root meant it couldn't find the JSON in `src/CvModels/`. Fix: use `AppContext.BaseDirectory` (always points to the compiled output) and add `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>` in the `.csproj` so the JSON gets copied to the build output.

### config.Get<T>() vs config.GetSection("Key").Get<T>() [`concept`]

`config.Get<Person>()` binds from the **root** of the JSON — expects `Identity`, `Experiences`, etc. as top-level keys.

`config.GetSection("Person").Get<Person>()` first navigates into a `"Person"` key, then binds from there.

`GetSection` matters when config has multiple unrelated things at the root (e.g. `Person`, `Logging`, `ConnectionStrings`). Each section binds independently. For now, binding from root is fine since the JSON only has person data — but we'll likely need sections later when adding database connection strings.

### Manual config reading vs binder [`concept`]

Manual: read individual values with `config["Person:Identity:Name"]` — returns strings, need to know exact key paths including array indices (`"Education:0:Name"`). Gets fiddly with collections.

Binder (`Microsoft.Extensions.Configuration.Binder` NuGet): `config.Get<T>()` or `config.GetSection("X").Get<T>()` maps JSON to C# objects automatically, matching keys to properties/constructor params by name. Much cleaner, especially for nested objects and lists.

### Nullable return from Get<T>() [`tip`]

`config.GetSection("X").Get<T>()` returns `T?` (nullable) — the section might not exist in the JSON. Always null-check before using the result, or the compiler warns CS8602 "dereference of possibly null reference".

### Config binder can't handle deep constructor-based objects [`mistake`]

Tried `config.Get<Person>()` to bind the whole JSON to `Person` in one go. Failed with `InvalidOperationException: Cannot create instance of type 'Person' because parameter 'identity' has no matching config`. The binder can handle simple types (strings, ints) in constructors, but struggles with nested complex objects (like `Identity`, `ContactInformation`) passed as constructor parameters. It prefers mutable objects (parameterless constructor + public setters).

Three options considered:
1. Add parameterless constructors + setters — works but breaks our immutable design
2. Bind sections individually (what was working before) — fine but manual assembly
3. Use `System.Text.Json` deserializer — handles constructor-based classes natively, more common in real apps for structured data

**Decision:** Going with option 3. The config binder is designed for flat settings (connection strings, feature flags), not deep object graphs. `System.Text.Json` is the right tool for deserializing structured data into constructor-based immutable models.

## 16 March 2026

### System.Text.Json replaces config binder [`decision`]

Switched from `Microsoft.Extensions.Configuration` binder to `System.Text.Json` for deserializing CV data. The config binder is designed for flat key-value settings and can't handle deep constructor-based object graphs. `System.Text.Json` is a full serializer that understands constructors with parameters, nested objects, arrays, and enums.

### Wrapper class vs JsonDocument for root key [`decision`]

Used a wrapper class (`PersonWrapper`) to handle the root `"Person"` key in the JSON. Two options considered:

1. **Wrapper class** (chosen) — fully typed, single-pass deserialization, reusable if more root sections are added later
2. **JsonDocument + GetProperty** — no extra class, but uses magic strings (`GetProperty("Person")`), two parsing passes, and a typo means a runtime error

Wrapper class is the standard approach because it works *with* `System.Text.Json`'s design rather than around it.

### JsonStringEnumConverter must be passed to Deserialize [`mistake`]

`JsonStringEnumConverter` must be added to `JsonSerializerOptions` **and** the options must be passed to `JsonSerializer.Deserialize()`. Without it, enums default to numeric parsing — so `"Remote"` in JSON fails to parse into `WorkMode.Remote`. The converter handles both string and numeric enum values by default, so it's always safe to include.

### Cleanup — remove what you don't use [`tip`]

After switching to `System.Text.Json`, removed the unused NuGet packages (`Microsoft.Extensions.Configuration.Json` and `Binder`) from the `.csproj`. No point keeping dependencies you've moved away from. Also moved `PersonWrapper` from the bottom of `Program.cs` into its own file in `Models/`. No need for a subfolder like `Models/Dtos/` for a single class — YAGNI (You Aren't Gonna Need It). Reorganise later when there's a real need.

### Gitignore glob patterns: `*` vs `**` [`mistake`]

`*/appsettings.Development.json` only matches one directory deep (e.g., `src/appsettings.Development.json`). Our file was at `src/CvModels/appsettings.Development.json` — two levels deep — so it wasn't being ignored. Fix: `**/appsettings.Development.json` — double star matches any number of directories. Always use `**` when you want to ignore a filename regardless of where it sits in the repo.
