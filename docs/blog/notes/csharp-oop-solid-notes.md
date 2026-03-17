---
topic: "C# OOP & SOLID — Building My CV Models"
slug: csharp-oop-solid
status: notes
sessions: [2026-03-10, 2026-03-11, 2026-03-13]
---

## 10 March 2026

### OOP fundamentals refresher [`concept`]

Before writing any code, reviewed core OOP concepts: classes vs interfaces, inheritance vs composition, and the Single Responsibility Principle. Key takeaway: interfaces define contracts ("anything that implements me must have these capabilities"), and composition ("has a") is generally preferred over inheritance ("is a") — `Person` shouldn't inherit from `Skill`, it should contain a list of skills.

### Skill class with constructor validation [`pattern`]

Built `Skill` with constructor validation to keep proficiency scores between 0 and 10. Extracted the validation into a reusable private method.

```csharp
// Private validation method keeps constructor clean
private static int ValidateProficiency(int value)
{
    if (value < 0 || value > 10)
        throw new ArgumentOutOfRangeException(...);
    return value;
}
```

### SkillCategory enum design [`decision`]

Designed `SkillCategory` based on actual CV sections: Backend, Frontend, Database, DevOps, Testing, SoftSkills, AIandTooling. Modelling from real data, not abstract ideas.

### Experience with nullable end dates [`pattern`]

`Experience` uses nullable `DateOnly?` for `EndDate` — null means "current job". Also created `WorkMode` enum (Remote/Hybrid/OnSite) and `Responsibility` class that distinguishes between day-to-day duties and achievements.

### Skills belong on Experience, not Responsibility [`decision`]

Skills are tied to the job/role level (Experience), not individual responsibilities. A responsibility might use several skills, and a skill applies across multiple responsibilities in the same role.

### Project folder organisation [`pattern`]

Organised into `Models/` and `Enums/` folders — common C# convention for keeping domain types tidy.

### DateOnly for dates, int for years [`decision`]

Used `DateOnly` (not `DateTime`) for precise dates like job start/end. Used plain `int` for things that only need a year (like education).

## 11 March 2026

### Education model — EndYear made non-nullable [`decision`]

Changed `EndYear` from nullable to non-nullable `int`. If you're listing education on a CV, you've completed it — there's always an end year.

### Language + LanguageProficiency enum [`concept`]

Created `Language` class with a `LanguageProficiency` enum (Native, Fluent, Advanced, Intermediate, Basic). Enum values represent well-known proficiency levels rather than arbitrary numbers.

### Project — computed IsOngoing property [`decision`]

`IsOngoing` is a computed property (`EndDate == null`), not a stored field. It's derived from the project's own data, so it belongs on the model. This contrasts with `SortOrder` and `IsFeatured` which were removed (see below).

### SortOrder and IsFeatured removed from Project [`decision`]

`SortOrder` is a display/presentation concern — it belongs in the service or view layer, not on the domain model. Same for `IsFeatured` — whether a project is "featured" is a presentation decision, not an inherent property of the project. Separation of concerns.

### Person — extracting Identity and ContactInformation [`pattern`]

Rather than putting everything on `Person`, extracted `Identity` (Name, Title, PersonalSummary) and `ContactInformation` (Email, Phone?, LinkedIn, GitHub, Portfolio?) into their own classes. Keeps `Person` as a clean root model that composes other objects.

### AllSkills as a computed property — Single Source of Truth [`concept`]

`Person.AllSkills` aggregates skills from all experiences and projects. It's computed (not stored) so there's only one source of truth — you can't accidentally have a skill in `AllSkills` that doesn't belong to any experience or project.

### Nullable constructor params must match property nullability [`mistake`]

`ContactInformation` had a bug: constructor parameters were nullable but properties weren't (or vice versa). The nullability of constructor params must match the properties they set. The compiler warns about this but it's easy to miss.

### Identity.CurrentRole — string over complex type [`decision`]

Changed `Identity.CurrentRole` from `Experience` type to a simple `string Title`. A person's current title is just a label for display — it doesn't need to carry the full weight of an `Experience` object with dates, responsibilities, etc.

## 13 March 2026

### Reference equality vs value equality — Distinct() didn't deduplicate [`mistake`]

`AllSkills` used `.Distinct()` to remove duplicate skills, but it wasn't working — React appeared twice. The reason: by default, C# compares objects by **reference** (are they the same instance in memory?), not by **value** (do they have the same data?). Two `Skill("React", Frontend, 4)` objects created separately are different references, even with identical data.

Fix: override `Equals` and `GetHashCode` on `Skill` to define equality by `Name` + `Category` (not `Proficiency` — "React Frontend" is the same skill regardless of level).

```csharp
public override bool Equals(object? obj)
{
    if (obj is not Skill other) return false;
    return Name == other.Name && Category == other.Category;
}

public override int GetHashCode()
{
    return HashCode.Combine(Name, Category);
}
```

### Deduplication with GroupBy — keeping the highest proficiency [`pattern`]

Even with `Equals`/`GetHashCode`, `Distinct()` just picks the first match and drops the rest. If the same skill has different proficiency levels across experiences and projects, you'd silently lose the higher one. Replaced `Distinct()` with `GroupBy` + pick max:

```csharp
.GroupBy(s => new { s.Name, s.Category })
.Select(g => g.OrderByDescending(s => s.Proficiency).First())
```

This groups by identity, then keeps the highest-rated instance. Better for a CV — show your best level.
