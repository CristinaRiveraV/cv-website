---
topic: MongoDB Integration
slug: mongodb
status: notes
sessions: [2026-03-20]
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
