---
topic: ".NET JSON Enum Labels — single source of truth for display names"
slug: dotnet-json-enum-labels
status: notes
sessions: [2026-04-24]
---

## 2026-04-24

### The problem: raw PascalCase enum names in the API response [`concept`]

`SkillCategory` is a C# enum with values like `SoftSkills` and `AIandTooling`. `System.Text.Json`'s default `JsonStringEnumConverter` serialises each value as its identifier string — so the API was returning `"category": "SoftSkills"` and `"category": "AIandTooling"` in the CV payload. Those went straight into the sidebar as section headings and looked like debug output rather than UI copy.

Single-word enum values (`Backend`, `Frontend`, `Database`, `Testing`, `Other`) are already fine — only the compound ones need help.

### Three approaches considered, two rejected [`decision`]

**A. Humanize on the frontend** — a small `humanize()` helper with a `{ SoftSkills: 'Soft Skills', AIandTooling: 'AI & Tooling' }` override map, falling back to a regex that inserts spaces between lowercase-uppercase transitions. Works, but duplicates the display-name knowledge that logically belongs next to the enum definition. If another client ever consumes the API, it sees the raw values.

**B. `[EnumMember(Value = "…")]` attribute** — historically the .NET attribute for custom serialisation names. Designed for `DataContractSerializer` and older XML scenarios; `System.Text.Json`'s default converter doesn't respect it reliably. Would need a custom converter or a third-party package to honour it. Extra plumbing for a small gain.

**C. `[JsonStringEnumMemberName("…")]` attribute** — .NET 9 added this purpose-built attribute, and `JsonStringEnumConverter` respects it natively. One attribute per tricky value, no new converter, no DTO mapping, no frontend helper. Went with this.

The project is on .NET 10, so C was available immediately. On .NET 8 or earlier, the pragmatic fallback is a custom `JsonConverter<TEnum>` that reads `[EnumMember]` values.

### The one-file change [`pattern`]

```csharp
using System.Text.Json.Serialization;

public enum SkillCategory
{
    Other,
    [JsonStringEnumMemberName("Soft Skills")] SoftSkills,
    Backend,
    Frontend,
    Database,
    DevOps,
    Testing,
    [JsonStringEnumMemberName("AI & Tooling")] AIandTooling
}
```

`Program.cs` already had `options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())` registered — nothing else to wire up. Next API call: `"category": "Soft Skills"`. Frontend renders `{skill.category}` unchanged and gets pretty copy for free.

### BSON and JSON serialisers are independent [`concept`]

The reason this change is safe is that MongoDB's C# driver uses its own BSON serialiser, completely separate from `System.Text.Json`. Storage format on disk doesn't change when JSON labels change — the same document that contained `"Category": "SoftSkills"` (or an integer) before the change still round-trips correctly via the enum after it. The driver still maps `"SoftSkills"` ⇄ `SkillCategory.SoftSkills`, and the JSON layer just prints a different string on the way out.

Two serialisers in play for the same type:
- **BSON serialiser (MongoDB C# driver):** controls how the enum is stored in Mongo. Untouched.
- **JSON serialiser (`System.Text.Json` + `JsonStringEnumConverter`):** controls what the HTTP API writes to the wire. Customised by the attribute.

Useful mental model any time you're working with a storage driver and an HTTP layer on the same POCO — they're separate pipelines with separate configuration.

### Design principle: presentation at the API boundary [`concept`]

The reflex "let's humanise on the frontend, it's a display concern" isn't wrong — but if multiple clients eventually consume the same endpoint (mobile app, another internal service, the `/cv` docs), each one would need its own identical humanize mapping, or risk showing the raw identifier. Putting the display label *at the API boundary*, via an attribute sitting next to the enum definition, makes every consumer consistent for free.

This is the same principle as "validate inputs at the boundary": the place where data crosses from your system's internal language into the outside world is a natural home for the transformations that make it legible. Inside the system, the enum value is `SkillCategory.SoftSkills` — a strong type. Outside, it's `"Soft Skills"` — a label for a human. Both representations live together on the same line.

### Round-trip works for future write endpoints [`tip`]

`JsonStringEnumConverter` respects `[JsonStringEnumMemberName]` in both directions — it serialises `SkillCategory.SoftSkills` to `"Soft Skills"` on the way out, and parses `"Soft Skills"` back to `SkillCategory.SoftSkills` on the way in. So when a future PUT/PATCH endpoint arrives, the UI can post back whatever it received without having to un-humanise first. Pretty in, pretty out, strong type in the middle.
