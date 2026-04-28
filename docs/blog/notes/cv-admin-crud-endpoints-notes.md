---
topic: CV Admin CRUD Endpoints (with Auth0 RBAC)
slug: cv-admin-crud-endpoints
status: notes
sessions: [2026-04-28]
---

## 2026-04-28

### Picking the next topic [`decision`]

After Day 18 (CV section polish merged), the pending TODOs were: admin update endpoint, frontend Auth0 login, sidebar layout, testing. Picked the **admin update endpoint** because it cashes in on the existing Auth0 backend wiring and unlocks editing the CV without `mongosh`.

### PUT vs PATCH for the update endpoint [`decision`]

Initial question: should the update endpoint use PUT (full replace) or PATCH (partial)?

- **PUT** = simpler. Send the whole document, server overwrites. One endpoint, no surprises.
- **PATCH** = flexible but messy on nested arrays. You write merge logic per field, and arrays like `Experiences` need careful handling (replace? merge by id? append?).

**Decision:** go granular per **section** rather than one endpoint per verb-style. Treat each section as its own resource with proper CRUD, so editing one experience doesn't require sending the whole CV. PATCH avoided entirely — replacing a whole section is unambiguous, partial updates inside a section aren't worth the merge logic.

### Granular sub-resources over a single full-CV PUT [`decision`]

Two shapes considered:

- **One endpoint:** `PUT /cv` — replaces everything. Quickest to ship. Bad UX once you want to add/remove individual experiences.
- **Sub-resources:** `PUT /cv/identity`, `POST /cv/experiences`, `PUT /cv/experiences/{id}`, `DELETE /cv/experiences/{id}`, etc.

Picked sub-resources. ~4× the code but each operation maps to a clear user intent ("update my LinkedIn URL", "delete this old job"). Without it the only safe edit path is "fetch the whole CV, mutate locally, send it all back" — a recipe for clobbering concurrent edits later.

### Single objects vs lists [`pattern`]

- **Single objects** (`Identity`, `ContactInformation`) → only `PUT`. There's exactly one — POST and DELETE don't make sense.
- **Lists with `Id`** (`Experiences`, `Education`, `Certifications`, `Projects`) → standard CRUD: `POST` (create), `PUT /{id}` (replace), `DELETE /{id}`.
- **Lists without `Id`** (`Languages`, `AdditionalSkills`) → use a natural key in the URL. Languages by `Name`. Skills by `Name + Category` (composite key — Skill's equality is on those two fields).

A future option for Skills is to add an `Id` field, which would normalise the URL shape across all list resources, but it's a model change with a Mongo migration cost — deferred.

### Two gaps spotted while writing the spec [`mistake`]

- **Certifications has no GET endpoints at all** — model and section exist, but nobody added the reads. Will fix while in there.
- **`GET /cv/skills` returns the *aggregated* `AllSkills` view** (skills from experiences + projects + additional). The directly-editable list is `AdditionalSkills`, and it has no dedicated GET. New endpoint `GET /cv/skills/additional` will expose it.

### Endpoint spec lives in `docs/api-endpoints.md` [`pattern`]

Wrote the full spec as a markdown table per section before touching code. Cheaper to argue about URL shapes and response codes in a doc than after the routes are wired up. Includes status legend (✅ implemented, 🚧 planned, 💡 future), response code conventions (`201` + `Location` for POST, `204` for DELETE, `404` for missing id), and a suggested implementation order so the work splits cleanly into 2–3 PRs.

### Auth: "any valid token" isn't enough [`concept`]

The backend has Auth0 JWT validation wired up but **no endpoint currently calls `RequireAuthorization`** — auth was scaffolded in Day 13 but never enforced anywhere. Adding it now.

The naive option is `RequireAuthenticatedUser()` — any valid token from the tenant gets in. But any Auth0 application authorised against this API could mint a token. Bad.

**Better:** require a *permission*. Auth0 has RBAC (Role-Based Access Control) — define a permission like `cv:write` on the API, grant it to roles/users/M2M apps, and have ASP.NET check that the JWT has that permission claim.

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CvWrite", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("permissions", "cv:write"));
});
```

When Auth0's "Add Permissions in the Access Token" toggle is on, permissions arrive as one or more claims of type `permissions`. `RequireClaim("permissions", "cv:write")` matches any of them.

### Auth0: "Enable RBAC" + "Add Permissions in the Access Token" are *both* needed [`tip`]

On the API's Settings tab in Auth0:
- **Enable RBAC** alone makes Auth0 *check* permissions, but doesn't put them in the token.
- **Add Permissions in the Access Token** is the toggle that actually stamps `"permissions": ["cv:write"]` into the JWT payload.

Forgetting the second toggle is a classic "why isn't my policy matching" bug — the token validates fine, but the claim is missing, so `RequireClaim` 403s.

### M2M Application vs User — who calls the API? [`concept`]

While walking through the Auth0 setup it became clear the answer to "should I create a user?" depends on **who's calling the API right now vs later**:

| Caller | Auth flow | Auth0 identity |
|---|---|---|
| Insomnia (now, for testing) | OAuth2 Client Credentials | **Application** (M2M), not a user |
| React frontend (future) | Authorization Code + PKCE | **User** (me, logging in via browser) |

For **today**: M2M Application. No user needed. The Application is authorised against `cv-api`, granted `cv:write`, and Insomnia fetches a token with client_id + client_secret. Simpler than inventing a password and a login flow that doesn't exist yet.

For **later** (when frontend login is the topic): create a User, assign them the Admin role (which holds `cv:write`), log in from the SPA. Both paths produce a JWT with the same `permissions` claim, so the same policy works for both.

The Role I created today is reusable — it's not tied to a user yet, but it'll be the obvious thing to assign to my future user account.

### Where we paused [`tip`]

Step 1 of the implementation plan is **"auth scaffolding"** — define the policy, configure Auth0 RBAC, but don't attach the policy to any endpoint yet (so anonymous reads keep working). Pieces:

1. Auth0 dashboard:
   - ✅ `cv:write` permission added on the cv-api API
   - ✅ Enable RBAC + Add Permissions in the Access Token toggled on
   - ✅ Admin role created with `cv:write`
   - ⏸️ M2M Application not yet created — next session
2. Code: ⏸️ `Program.cs` policy registration not yet written — next session
3. Verify: ⏸️ pending — confirm anonymous GETs still return 200 after the code change

Resume next session by creating the M2M app in Auth0, fetching a token, then adding the `CvWrite` policy to `Program.cs`.
