# CV API Endpoint Specification

Status legend:
- ✅ Implemented
- 🚧 Planned in this milestone
- 💡 Future / nice-to-have

All write endpoints require a valid Auth0 JWT **with the `cv:write` permission**. Read endpoints stay public.

---

## Auth model

Auth0 is already wired up in `Program.cs` (Authority + Audience), but no endpoint currently calls `.RequireAuthorization(...)`. The plan:

1. Define an authorization policy `"CvWrite"` that requires a `permissions` claim containing `cv:write` (Auth0 emits this when RBAC is enabled on the API and a permission is granted to a user/role).
2. Apply `.RequireAuthorization("CvWrite")` to every write endpoint (POST / PUT / DELETE).
3. Reads stay anonymous.

Why a permission instead of "any valid token": any Auth0 application registered against the same tenant could mint a token. Requiring `cv:write` means only users/M2M apps explicitly granted that permission can edit the CV.

---

## Identity (single object)

| Method | Path | Auth | Body | Status |
|---|---|---|---|---|
| GET | `/cv/identity` | – | – | ✅ |
| PUT | `/cv/identity` | `cv:write` | `Identity` | 🚧 |

PUT replaces the entire Identity object. There's only ever one, so PATCH would only save typing.

---

## Contact Information (single object)

| Method | Path | Auth | Body | Status |
|---|---|---|---|---|
| GET | `/cv/contact` | – | – | ✅ |
| PUT | `/cv/contact` | `cv:write` | `ContactInformation` | 🚧 |

Same reasoning as Identity.

---

## Experiences (list, by `Id`)

| Method | Path | Auth | Body | Status |
|---|---|---|---|---|
| GET | `/cv/experiences` | – | – | ✅ |
| GET | `/cv/experiences/{id}` | – | – | ✅ |
| POST | `/cv/experiences` | `cv:write` | `Experience` (no Id — server generates) | 🚧 |
| PUT | `/cv/experiences/{id}` | `cv:write` | `Experience` (full replace) | 🚧 |
| DELETE | `/cv/experiences/{id}` | `cv:write` | – | 🚧 |

Nested arrays (`Skills`, `Responsibilities`) are edited by PUTting the whole experience. Sub-sub-resource endpoints (e.g. `POST /experiences/{id}/responsibilities`) are 💡 future — overkill for now.

Returns:
- `POST` → `201 Created` with `Location: /cv/experiences/{newId}` and the created entity in the body.
- `PUT` → `200 OK` with the updated entity, or `404` if id not found.
- `DELETE` → `204 No Content`, or `404` if id not found.

---

## Education (list, by `Id`)

| Method | Path | Auth | Body | Status |
|---|---|---|---|---|
| GET | `/cv/education` | – | – | ✅ |
| GET | `/cv/education/{id}` | – | – | ✅ |
| POST | `/cv/education` | `cv:write` | `Education` | 🚧 |
| PUT | `/cv/education/{id}` | `cv:write` | `Education` | 🚧 |
| DELETE | `/cv/education/{id}` | `cv:write` | – | 🚧 |

Same shape as Experiences. Nested `Courses` edited via PUT on the parent.

---

## Certifications (list, by `Id`)

| Method | Path | Auth | Body | Status |
|---|---|---|---|---|
| GET | `/cv/certifications` | – | – | 💡 (GET list missing — add it) |
| GET | `/cv/certifications/{id}` | – | – | 💡 |
| POST | `/cv/certifications` | `cv:write` | `Certification` | 🚧 |
| PUT | `/cv/certifications/{id}` | `cv:write` | `Certification` | 🚧 |
| DELETE | `/cv/certifications/{id}` | `cv:write` | – | 🚧 |

> Heads-up: there are currently **no GET endpoints for certifications** even though the model and section exist. We should add the GETs while we're in here.

---

## Projects (list, by `Id`)

| Method | Path | Auth | Body | Status |
|---|---|---|---|---|
| GET | `/cv/projects` | – | – | ✅ |
| GET | `/cv/projects/{id}` | – | – | ✅ |
| POST | `/cv/projects` | `cv:write` | `Project` | 🚧 |
| PUT | `/cv/projects/{id}` | `cv:write` | `Project` | 🚧 |
| DELETE | `/cv/projects/{id}` | `cv:write` | – | 🚧 |

---

## Additional Skills (list, identified by `Name` + `Category`)

`Skill` has no `Id` field — equality is `Name + Category`. Two options:

**Option A (chosen for MVP): URL-encode Name in the path, scope by category via query.**

| Method | Path | Auth | Body | Status |
|---|---|---|---|---|
| GET | `/cv/skills` | – | – | ✅ (returns the *aggregated* `AllSkills` view) |
| GET | `/cv/skills/additional` | – | – | 🚧 (returns just `AdditionalSkills`) |
| POST | `/cv/skills/additional` | `cv:write` | `Skill` | 🚧 |
| PUT | `/cv/skills/additional/{name}?category={cat}` | `cv:write` | `Skill` | 🚧 |
| DELETE | `/cv/skills/additional/{name}?category={cat}` | `cv:write` | – | 🚧 |

**Option B (future):** add an `Id` to `Skill` and use it like other lists. Cleaner but a model change with migration cost.

> Note the existing `GET /cv/skills` returns the aggregated view (skills from experiences + projects + additional). The new endpoints target **only** the `AdditionalSkills` list — that's the only one that's directly editable as a list. Skills inside an Experience are edited by PUTting that Experience.

---

## Languages (list, identified by `Name`)

`Language` has no `Id` — `Name` is the natural key.

| Method | Path | Auth | Body | Status |
|---|---|---|---|---|
| GET | `/cv/languages` | – | – | ✅ |
| POST | `/cv/languages` | `cv:write` | `Language` | 🚧 |
| PUT | `/cv/languages/{name}` | `cv:write` | `Language` | 🚧 |
| DELETE | `/cv/languages/{name}` | `cv:write` | – | 🚧 |

---

## Full CV

| Method | Path | Auth | Body | Status |
|---|---|---|---|---|
| GET | `/cv` | – | – | ✅ |
| PUT | `/cv` | `cv:write` | `Person` | 💡 future / probably never |

We're going granular, so a full-CV PUT shouldn't be needed. Skipped.

---

## Error responses (standard)

| Code | When |
|---|---|
| `400 Bad Request` | Body fails validation (missing required field, bad enum value, etc.) |
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Valid token but missing `cv:write` permission |
| `404 Not Found` | Item id (or name) doesn't exist |
| `409 Conflict` | POST creates a duplicate (e.g., a Skill with the same Name+Category, a Language with the same Name) |
| `500 Internal Server Error` | Unexpected — caught by the global exception handler |

---

## Suggested implementation order

1. **Auth scaffolding** — add the `CvWrite` policy, configure Auth0 RBAC + `cv:write` permission, leave it unattached. Smallest, isolated change. Verify anonymous reads still work.
2. **Identity & Contact PUT** — single-object endpoints, no array logic, easiest first writes. Apply `RequireAuthorization("CvWrite")`. Test 401/403/200 paths.
3. **Experiences CRUD** — first list resource. Establishes the pattern (POST/PUT/DELETE + 404/201/204 response codes) for the rest.
4. **Education / Certifications / Projects CRUD** — copy-paste the Experiences pattern. Also add the missing Certifications GETs.
5. **Languages CRUD** — natural-key URLs (Name).
6. **Additional Skills CRUD** — composite-key URLs (Name + Category).

Steps 1–3 is a healthy first PR. Steps 4–6 can be a follow-up PR (or split further) so reviews stay small.
