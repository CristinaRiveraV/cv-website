---
topic: MongoDB Integration
slug: mongodb
status: notes
sessions: [2026-03-20, 2026-03-24, 2026-03-27, 2026-04-22, 2026-04-23]
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

## 2026-04-22

### Evolving a required field across Mongo + C# (schema migration) [`pattern`]

Added a new `Location` property to `Identity` that we wanted to be non-nullable in C# (`public required string Location { get; set; }`). Because the property is `required`, existing documents in the `Profiles` collection needed the field populated *before* the API restarted — otherwise deserialization throws on the very first `GET /cv`. Order matters:

1. Add the C# property in `Identity.cs` (don't start the app yet).
2. Update the existing document in Mongo so every row has the new field.
3. Restart / rebuild the API. Now the data shape matches the model.

This is a miniature version of the database migration pattern you'd see in any real system: schema changes land in both places or they don't land at all. With `required`, C# will refuse to construct the object if the BSON document is missing the element.

### `$set` with dot-notation to add a nested field via mongosh [`tip`]

Updated the single document in the `Profiles` collection from the Atlas shell:

```js
db.Profiles.updateOne(
  {},
  { $set: { "Identity.Location": "Manchester, UK" } }
)
```

The empty `{}` filter matches any document — fine when there's exactly one. The dotted key `"Identity.Location"` targets a field *inside* the nested `Identity` sub-document without having to replace the whole object. If I'd written `{ Identity: { Location: "..." } }`, it would have overwritten the entire `Identity` sub-document and wiped `Name`, `JobTitle`, `PersonalSummary`. Dot-notation adds/updates the one field and leaves the rest alone.

### MongoDB driver rejects unknown elements by default [`concept`]

Hit this error when running the API:

```
System.FormatException: 'An error occurred while deserializing the Identity property
of class Person: Element 'Location' does not match any field or property of class Identity.'
```

Counter-intuitive wording — it said "does not match any field or property of class `Identity`", even though I *had* added `Location` to the class. The real cause was a stale build (see next note). But the error itself is worth understanding: the MongoDB C# driver is **strict** by default — if a BSON document contains an element the POCO class doesn't have, deserialization fails instead of silently ignoring it. Strictness can be relaxed by annotating a class with `[BsonIgnoreExtraElements]`, but for this project keeping strictness is the right call — schema drift between DB and code should surface loudly.

### Stale build: CvModels wasn't rebuilt, API loaded the old Identity [`mistake`]

Saw the "Element 'Location' does not match any field or property" error *after* adding the `Location` property and `Ctrl+F5`-ing. The mongosh update had worked and the doc had the new field — but the running API was loading the *previous* compiled `CvModels.dll`, so its in-memory `Identity` class still had only three properties. VS's incremental build decided `CvApi` hadn't changed and skipped rebuilding the dependency.

Fix: `Build → Rebuild Solution` forced both `CvModels` and `CvApi` to recompile from scratch, and the next run deserialized cleanly. Lesson: when you change a class in a referenced project and the runtime behaviour still doesn't match, suspect a stale build before suspecting a logic bug.

## 2026-04-23

### Promoting an entry from one array to its own collection-field [`pattern`]

The old CV had the AWS Certified Cloud Practitioner lumped in under Education. That's fine for a static HTML page, but once the shape is modelled in C# it becomes awkward: a certification isn't an `Education` — no `Institution`, no `EndYear`, it has an issuer and a (possibly expiring) date. Refactored by introducing a new `Certification` class and a `Certifications` array on `Person`, then moving the entry in Mongo. The migration was two mongosh operations on the single CV document:

```js
// Add the Certifications array with the AWS entry
db.Profiles.updateOne({}, { $set: { Certifications: [ {
  Id: "cert-aws-cloud-practitioner",
  Name: "AWS Certified Cloud Practitioner",
  Issuer: "Amazon Web Services",
  IssueDate: ISODate("2024-11-01"),
  ExpiryDate: ISODate("2027-11-01"),
  Description: "..."
} ] } })

// Remove the AWS entry from Education
db.Profiles.updateOne({}, { $pull: { Education: { Name: /AWS/ } } })
```

`$set` creates the field if it doesn't exist and replaces it if it does. `$pull` removes elements from an array matching the filter. Doing it in two discrete operations keeps each one easy to reason about — if the `$pull` had been wrong, the `$set` would still have succeeded and nothing would be half-migrated.

### ISODate stores real dates, not strings [`tip`]

Stored `IssueDate` / `ExpiryDate` as `ISODate("2024-11-01")` rather than `"2024-11-01"`. The `ISODate()` helper in mongosh creates a genuine BSON `Date` value that the MongoDB C# driver deserializes cleanly into `DateTime`. Storing the same value as a string would force either a string property on the C# side or a custom converter — neither needed when the data type is right from the start. Downside: mongosh prints them back as full ISO 8601 timestamps like `2024-11-01T00:00:00Z`, so date formatting has to happen in the API/frontend.

### Slug IDs vs GUIDs for small collections [`decision`]

Used `"cert-aws-cloud-practitioner"` as the Id rather than a `Guid`. Reasoning: there's only ever going to be a handful of certifications and they're all referenced by a single user. A readable slug makes Mongo queries, logs, and debugging easier (`{ Id: "cert-aws-cloud-practitioner" }` is self-documenting in a way that `{ Id: "7b3f…" }` isn't). GUIDs matter when identifiers have to be globally unique across distributed systems — overkill for a personal CV. The `Id` field on `Certification` is `string`, so it'll accept whichever convention makes sense per collection.
