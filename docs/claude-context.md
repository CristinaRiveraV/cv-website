# Claude Context — CV Website Training

Portable snapshot of everything Claude would otherwise pick up from local auto-memory. Kept in the repo so it survives a machine change. Update as things change — treat it as source of truth for cross-machine handover.

Last refreshed: 2026-07-08.

---

## 1. Project overview

- Training project to move from junior → mid-level developer.
- Stack: React (Vite + MUI, TypeScript) frontend, C# ASP.NET Core minimal API backend, MongoDB Atlas.
- Rebuilding the existing portfolio site (CristinaRiveraV.github.io) from scratch.
- Cadence: ~1 hour/day, started March 2026. Days 1–19 complete; Day 20 in progress.
- Single monorepo — frontend under `src/cv-frontend/`, backend under `src/CvApi/` and `src/CvModels/`.

### Live environments

- **API:** https://cv-api-vf6m.onrender.com (Render, auto-deploy on merge to `main`)
- **Frontend:** https://criveravaldez.dev (Vercel, custom domain via Porkbun DNS)
- **CI:** GitHub Actions on every PR — `build-backend` (.NET 10) + `build-frontend` (Node 24, install/lint/build).
- **Database:** MongoDB Atlas (free tier). Single-document design under `Profiles` collection.
- **Auth:** Auth0 tenant with `https://cv-api` audience, `cv:write` permission + `Admin` role via RBAC, M2M app `cv-api-insomnia` for API testing.

### Related side project

- **URL Shortener** at `C:\TrainingProjects\UrlShortener` — separate C# + Redis learning project. Independent of this repo.

---

## 2. How to work with the user

These are non-negotiable working rules — the enforceable ones already live in `CLAUDE.md`, but capture the *why* here so future-Claude knows when they apply and when they don't.

### 2.1 Tutor mode is the default in this repo

- The user-level `tutor` skill is auto-loaded at the start of every conversation (see repo `CLAUDE.md`).
- Socratic questioning is high-value here — user explicitly confirmed it "makes me ask questions I wouldn't otherwise ask." Do not trim the predict-before-reveal loops to move faster.
- Watch for rusty moments after long gaps between sessions — those are exactly when the predict step pays off most.

### 2.2 Never edit source code directly

- The user writes every line herself. Claude explains *what* and *why*, points at file + line, suggests the edit — but does not use Edit/Write/Bash to modify source files.
- Docs, blog notes, and this file **are** fine to edit directly.

### 2.3 Give Visual Studio / ReSharper shortcuts

User uses VS with ReSharper on the backend, VS Code on the frontend. When suggesting code actions, include the shortcut. Common ones:
- `Ctrl+H` — Find and Replace
- `Ctrl+R, R` (ReSharper) — Rename refactoring
- `Alt+Enter` (ReSharper) — Quick actions
- `Ctrl+T` (ReSharper) — Go to Everything
- `Ctrl+Shift+R` (ReSharper) — Refactor This

### 2.4 API testing → Insomnia (not Postman)

Reference the Insomnia UI when giving testing steps.

### 2.5 Don't add files to the toolkit repo

`C:\Projects\ai-developer-toolkit` is a shared repo. Project-specific skills / config for cv-website belong under `.claude/` in *this* repo, never in the toolkit.

### 2.6 Blog notes workflow

- Claude generates blog notes per session via the `blog-notes` skill; the user does NOT hand-write them.
- One synthesised blog post at the end of the whole project.
- Do NOT describe blog notes as a "reflection mechanism the user performs" (was a talk-slide correction).

---

## 3. User profile

- **Role:** Junior developer, active professional. Uses AWS at work with OAuth/IdentityServer + JWT for API auth (background familiar with the Auth0 patterns being used here).
- **Cloud future learning:** Azure — for CV value.
- **IDE:** Visual Studio + ReSharper for C#, VS Code for React.
- **Learning style:** Wants explanations, wants to understand commands before running them, writes code herself. Design conversations first, then code.
- **Presentation style (from 30 Apr 2026 talk):** Kills TED-talk / LinkedIn / aphoristic phrasing. First-person "I" not "we" unless genuinely team-level. Concrete timeline anchors over vague ones. No judgmental framing of other approaches.

### Job change (affects timing)

- Resigned from Zen Internet. Last day: **2026-07-09** (tomorrow at time of writing).
- New job starts July 2026.
- After 2026-07-09: no assumption of Zen work email / Teams / laptop access. Keep examples generic — bound not to disclose Zen business info in blog / CV / talks.

---

## 4. Where the training is

### Completed (Days 1–19)

Full day-by-day log below in §7. Headline: models → config → minimal API → OpenAPI/Scalar → error handling → MongoDB integration → Auth0 JWT → Docker + Render deploy → React frontend + MUI → Vite env vars → Vercel deploy → DNS + GitHub Actions CI → Identity.Location schema change → Certifications section + friendly enum labels → CV admin update endpoints PR 1.

### Day 20 — IN PROGRESS (frontend Auth0 login)

- Branch: `feature/frontend-auth0-login`
- SDK installed, `Auth0Provider` wired, AppBar login/logout UI in place, env vars set.
- Callback wiring code done (session 6, 2026-06-05): `Auth0ProviderWithNavigate` wrapper, `/callback` redirect_uri, `onRedirectCallback` navigate, `<BrowserRouter>` moved up in `main.tsx`, Fragment in `App.tsx`.
- **BLOCKED:** Auth0 returns `The provided redirect_uri is not in the list of allowed callback URLs` even though DevTools Network shows `http://localhost:5173/callback` and dashboard has that exact value.
- **Strong suspicion:** Vite HMR didn't pick the provider-tree restructure cleanly — stale bundle serving old `Auth0Provider`.

**First moves on next session, in order:**
1. Hard restart Vite (`Ctrl+C`, then `npm run dev`) + hard-reload browser (`Ctrl+Shift+R`).
2. Try login again. Success signal: token in localStorage, `isAuthenticated` true, AppBar shows name + Log out, lands on `/`.
3. If still failing: paste the literal `redirect_uri` from Network payload AND the literal dashboard entry side-by-side. Look for trailing slash, URL encoding (`%2F`), adjacent-line typo, zero-width chars.
4. Sanity check in console: `window.location.origin` returns exactly `http://localhost:5173` (no trailing slash).
5. Do NOT modify `redirect_uri` or the dashboard again until strings compared verbatim.

Auth0 dashboard items (Callback / Logout URLs, Web Origins, API Identifier, Connections, Admin user) were all verified through session 5 — don't redo unless symptoms change.

**Deferred until login round-trips successfully:**
- Verify token contents at https://jwt.io (audience `https://cv-api`, `scope` includes `cv:write`, `permissions` array contains `cv:write`).
- Wire `getAccessTokenSilently()` into an authenticated `fetch` helper for write endpoints.
- Protected `/admin` route (redirect if not authenticated).
- Edit forms for Identity / Contact / Experiences.
- Vercel env vars: `VITE_AUTH0_DOMAIN` + `VITE_AUTH0_CLIENT_ID` for production build.

---

## 5. TODOs (backlog)

### Day 19 PR 2 — Education / Certifications / Projects CRUD

- Same `TryAdd` / `TryUpdate` / `TryDelete` repository pattern as Experiences — straight copy, swap the type.
- Client-provided Id (kebab-slug), nullable-return service signatures.
- **New work:** add missing `GET /cv/certifications` and `GET /cv/certifications/{id}` reads (model + section exist, reads never wired).
- Flip 🚧 → ✅ in `docs/api-endpoints.md` per endpoint.

### Day 19 PR 3 — Languages + AdditionalSkills

- Natural-key URLs: `/cv/languages/{name}`, `/cv/skills/additional/{name}?category={cat}`.
- No `Id` field — equality on Name (Languages) or Name + Category (Skills).
- Needs URL-encoding for names with spaces / special chars.

### Global exception handler swallows 400s

- `src/CvApi/Program.cs` `UseExceptionHandler` catches `BadHttpRequestException` (JSON deserialisation / missing `required` field) and writes 500. Spec in `docs/api-endpoints.md` promises 400 that never fires.
- Discovered 2026-05-07 while testing `PUT /cv/contact`.
- Fix: make the handler discriminate — let framework 4xx bubble through, only catch unexpected for 500.
- Worth doing before PR 2 (more validation surface on list resources).

### AI-assisted coding CV section

- Extend the CV data model with a section for AI-assisted coding fluency (tools, MCPs, prompting/workflow).
- Two design options — discuss before implementing:
  - (a) New top-level section on `Person`/`Cv` alongside Experience/Education/Certifications — richer descriptions, links.
  - (b) New `SkillCategory` enum value with sub-fields — lighter, reuses rendering.

### Presentation cleanup

- `presentation-outline.md` — the Canva tips at file bottom still reference old slide numbers (`5, 11, 12`). Should be `7 (one-line method), 10 (without/with table), 11 (takeaway)`.
- Once outline is fully clean, next presentation work is building it in Canva.

### Future learning topics (post-current-milestones)

- Angular — with intent to migrate the entire CV website from React as a learning exercise.
- WASM.
- "Overstack" — meaning unclear (Stack Overflow? OpenStack? fullstack?). Clarify with user before scheduling.
- TypeScript (deeper — currently only using minimal TS in the frontend).
- Migrating React → Angular would learn both Angular AND TypeScript at once — natural pairing.

---

## 6. Deployment plan

- **Now:** Render (API, free tier) + MongoDB Atlas (free tier) + Vercel (frontend, free) + Porkbun (domain).
- **Custom domain:** criveravaldez.dev, DNS on Porkbun default nameservers, Vercel SSL cert.
- **Later:** Azure App Service and/or AWS App Runner as deployment alternatives when Azure comes up as a learning topic.

---

## 7. Training log (Days 1–20 summary)

Full detail was captured session-by-session in local memory. Preserved here for portability.

- **Day 1 (10 Mar):** C# OOP & SOLID — Skill, SkillCategory, Experience, Responsibility, WorkMode, Course models.
- **Day 2 (11 Mar):** More models — Education, Language, Project, Person, Identity, ContactInformation. Config: appsettings + template + .gitignore.
- **Day 3 (13 Mar):** .NET Configuration — reading JSON into models.
- **Day 4 (17 Mar):** ASP.NET Minimal API — CvApi project, `/cv` endpoints, CvService with DI.
- **Day 5 (18 Mar):** OpenAPI + Scalar.
- **Day 6 (19 Mar):** Error handling middleware — PR #2.
- **Day 7 (20 Mar):** MongoDB integration — Atlas + C# driver, single-document design, CvRepository.
- **Day 8 (24 Mar):** MongoDB testing, seeding real CV data — PR #3.
- **Day 9 (27 Mar):** `required` keyword, cleanup — merged PR #3.
- **Day 10 (31 Mar):** Auth0 + JWT authentication — tenant, JwtBearer — PR #5.
- **Day 11 (1 + 7 Apr):** Docker + Render — multi-stage Dockerfile, deployed to Render, live tests passing. PR #6.
- **Day 12 (8–9 Apr):** React frontend setup — Vite + TS, React Router, TS interfaces, CV page fetches real data. `feature/react-frontend`.
- **Day 13 (10 Apr):** MUI styling — AppBar, Typography, Card, Chip, List, icons. Home + Contact pages. CORS fix, MongoDB Atlas IP whitelist.
- **Day 14 (13 Apr):** Vite env vars — `.env.development` / `.env.production` / `.env.example`, `VITE_API_URL`. PR #8. Render 500 blocker discovered.
- **Day 15 (14–15 Apr):** Debug Render 500 — MongoDB connection string wrong in Render env. Fixed. PR #9 merged (Days 12–14). Deployed frontend to Vercel. Porkbun DNS (A + CNAME).
- **Day 16 (16–17 Apr):** DNS fix (Porkbun default nameservers, Vercel SSL). GitHub Actions CI — `build-backend` + `build-frontend` parallel jobs. PRs #10, #11. Sidebar refactor (Grid, Skills by category, LinearProgress, Languages).
- **Day 17 (22–23 Apr):** Full-stack schema change — `Identity.Location`. Mongo Atlas mongosh update. Stale-build gotcha (dotnet.exe held CvModels.dll — `taskkill //F //IM dotnet.exe //T`, delete bin/obj, Rebuild). PR #12.
- **Day 18 (23–24 Apr):** Certifications section + section polish + friendly enum labels via `[JsonStringEnumMemberName]`. PRs #13, #14. Key takeaway: user's pushback on Claude's duplicative proposals led to the clean API-boundary solution.
- **Day 19 (28 Apr – 7 May):** CV admin update endpoints — granular per-section CRUD with Auth0 RBAC. Branch `feature/cv-update-endpoint`. PR #15 (Identity / Contact / Experiences full CRUD) merged 2026-05-08. Session 3 lesson: 403 was stale token, not framework claim-mapping — mint fresh tokens before reaching for deep diagnostics. Bug found: exception handler → 500s (see §5 TODOs).
- **Day 20 (12 May + sessions through 2026-06-05):** IN PROGRESS. See §4 for exact resume state.

---

## 8. If you're future-Claude reading this cold

1. Read `CLAUDE.md` in the repo root first — that's the enforced ruleset. This file is context, that file is contract.
2. Match the current branch to §4 / §7 to place yourself in the timeline.
3. Do NOT edit source files. Guide the user through the edits herself, with VS/ReSharper shortcuts.
4. Tutor mode / Socratic questioning is the norm here — don't shortcut it.
5. Blog notes are Claude-generated via the `blog-notes` skill, not user-written.
6. Auto-memory files under `%USERPROFILE%\.claude\projects\C--TrainingProjects-cv-website\memory\` will NOT exist on a new machine. This file is the replacement — trust it, and refresh it as new state accumulates.
