---
topic: MongoDB Integration
slug: mongodb
status: notes
sessions: [2026-03-20, 2026-03-24, 2026-03-27]
---

## 2026-03-20

### Repository Pattern [`pattern`]

The **Repository Pattern** separates data access (how to get data) from business logic (what to do with data). In our project:

- **CvRepository** — handles MongoDB operations (connect, query, insert)
- **CvService** — handles business logic (what data to return, how to shape it)

If you swap databases later (e.g. MongoDB to PostgreSQL), only the repository changes — the service and endpoints don't care where the data comes from.

This is a common pattern in .NET and other backend frameworks.

### Document vs Relational Design [`decision`]

Chose to store the entire CV as a **single embedded document** (Option A) rather than splitting into separate collections. Reasons:

- A CV is a single logical document — you always read/display it as a whole
- The data is small (won't hit MongoDB's 16MB document limit)
- It's the most "document database" way — lean into what makes MongoDB different from SQL
- The API already serves it this way (`GET /cv` returns the whole person)
- Simpler code, fewer queries

Alternative options considered:
- **Option B (multiple collections):** separate collections for experiences, education, etc. linked by personId — essentially relational design in a non-relational DB
- **Option C (hybrid):** embed small/stable data, reference larger/growing collections

### Seed Method — Temporary Code [`tip`]

Created a `SeedFromJson` method on the repository to load example data from the template JSON into MongoDB. This is intentionally temporary — once data is in the database, it gets removed. Good practice: check if data already exists before seeding (`CountDocuments`) to avoid duplicates on restart.

### Adapting models for MongoDB — BsonConstructor vs parameterless constructors [`mistake`]

First attempt: added `[BsonConstructor]` attributes to existing parameterized constructors so MongoDB could deserialize them. This caused a `Member 'Identity' is not mapped` error — MongoDB couldn't match constructor parameters to properties when `[BsonId]` was also present.

**Solution:** Removed `[BsonConstructor]` and parameterized constructors entirely. Instead, added empty parameterless constructors and changed all properties from `{ get; }` to `{ get; set; }`. MongoDB uses the parameterless constructor + property setters to hydrate objects.

**Lesson:** MongoDB's BSON serializer works best with simple POCOs (Plain Old CLR Objects) — parameterless constructor + public get/set properties. Constructor-based immutability (which is great for domain models) fights the serializer. Keep it simple.

### Guid vs string IDs for sub-items [`decision`]

MongoDB threw `GuidSerializer cannot serialize a Guid when GuidRepresentation is Unspecified` for the `Guid Id` properties on Experience, Education, and Project.

Changed from `Guid` to `string` — simpler, no serialization issues, and the IDs are stable (they don't change when items are added/removed). For generating new IDs later: `Guid.NewGuid().ToString()`.

### Null-coalescing assignment `??=` for caching [`concept`]

Used `??=` in CvService to cache the Person from the database:

```csharp
private Person? _cachedPerson;
private Person GetPerson()
{
    _cachedPerson ??= _repository.GetPerson();
    return _cachedPerson ?? throw new InvalidOperationException("No CV data found in database.");
}
```

`??=` means "if null, assign this value." The database only gets hit once — every subsequent call uses the cached value. Good for data that rarely changes (like a CV).

## 2026-03-24

### Primary constructors vs parameterless constructors [`concept`]

C# 12 introduced **primary constructors** — `public class Course()` with `()` right on the class declaration. These are convenient for setting required values, but MongoDB's BSON deserializer can't use them.

The fix: remove the `()` from the class declaration and rely on parameterless constructors + public setters. For `Skill`, which had validation logic in its constructor, we kept the parameterized constructor *alongside* an explicit parameterless one:

```csharp
public class Skill
{
    public Skill() { }  // MongoDB uses this
    public Skill(string name, SkillCategory category, int proficiency)  // Manual creation uses this
    {
        // validation logic...
    }
    public string Name { get; set; }
    public SkillCategory Category { get; set; }
    public int Proficiency { get; set; }
}
```

**Tradeoff:** We lose compile-time safety (nothing forces you to set `Name` when creating a `Skill` manually), but gain compatibility with serializers. This is a common tradeoff in .NET — domain purity vs framework compatibility.

### Testing the full API pipeline [`practice`]

Before committing, tested every endpoint against the live MongoDB Atlas database:
- List endpoints (`/cv/experiences`, `/cv/education`, etc.)
- Individual item endpoints (`/cv/experiences/{id}`)
- 404 responses for non-existent IDs
- Full CV (`/cv`)

Testing the whole pipeline (API -> Service -> Repository -> MongoDB Atlas) catches issues that unit tests on individual layers might miss — like serialization problems that only surface when data actually travels through the wire.

### Keeping secrets out of GitHub [`practice`]

Verified before creating PR that no private data leaked:
- `appsettings.Development.json` (with real connection string) is **gitignored**
- `appsettings.Development.template.json` only has placeholder data ("Your Name", "your@email.com")
- Real CV data lives only in MongoDB Atlas
- Code references `ConnectionString` as a property, never hardcodes values

Always worth a quick check before pushing — `git diff` the files going into the PR to confirm nothing sensitive slipped in.

### Seeding real CV data [`practice`]

When replacing seed data, the `SeedFromJson` method only inserts if the collection is empty (`CountDocuments`). So to re-seed with new data, you must first delete the existing document from MongoDB Atlas (or temporarily drop the collection in code).

## 2026-03-27

### C# `required` keyword for MongoDB models [`concept`]

When you have parameterless constructors with non-nullable properties, the compiler raises CS8618 warnings — "Non-nullable property 'Name' must contain a non-null value when exiting constructor." The `required` modifier is the cleanest fix:

```csharp
// Before — CS8618 warning
public string Name { get; set; }

// After — no warning, plus compile-time safety
public required string Name { get; set; }
```

What `required` does: forces callers to set the property during object initialization (`new Identity { Name = "..." }`). Deserializers (MongoDB BSON, System.Text.Json) bypass this via reflection, so they still work fine — but if you ever construct objects in your own code, the compiler enforces correctness.

Better alternatives that we rejected:
- `= null!` — lies to the compiler ("trust me it's not null" when it is)
- Making properties nullable (`string?`) — wrong semantics, Name should never be null
- `= string.Empty` — silences the warning but hides the real issue

### Renaming a MongoDB collection [`tip`]

Renamed the collection from `"people"` to `"profiles"` in `CvRepository.cs`. Since MongoDB creates collections on first use, changing the name in code effectively creates a new empty collection — the old `"people"` collection still exists in Atlas but is no longer used. This meant `SeedFromJson` auto-ran (new collection was empty) without needing to manually delete old data. Can clean up the orphaned `"people"` collection in Atlas UI later.

Used VS **find-and-replace** (`Ctrl+H`) to also rename the private field from `_people` to `_profiles` throughout the file.

### Staying hands-on with AI assistance [`decision`]

Claude attempted to directly edit `CvRepository.cs` to rename `_people` → `_profiles`. Rejected the tool call and did the rename myself using VS find-and-replace. **Key learning principle:** when the goal is to learn, doing the mechanical work yourself builds muscle memory and IDE familiarity. AI is most valuable as a guide explaining *what* and *why*, not as a typist.
