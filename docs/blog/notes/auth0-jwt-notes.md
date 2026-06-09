---
topic: Auth0 JWT Authentication
slug: auth0-jwt
status: notes
sessions: [2026-03-31, 2026-04-01, 2026-05-12, 2026-05-28, 2026-06-05]
---

## 2026-03-31

### Why Auth Before Deployment [`decision`]

We pivoted from going straight to deployment — don't expose an unprotected API publicly. Auth first, deploy second. This is a real-world principle: **never deploy an API without authentication in place**.

### Auth Approaches Compared [`concept`]

| Approach | When to use |
|---|---|
| **API Key** | Simple, internal/server-to-server. Single shared secret. |
| **JWT (JSON Web Token)** | Standard for web apps. Stateless, self-contained tokens with claims. |
| **OAuth2 + Identity Server** | Full user authentication flows. Auth0, Duende IdentityServer, etc. |

We chose **Auth0** (cloud identity provider) because:
- Teaches the same JWT concepts as Duende IdentityServer (used at work)
- No need to host our own identity server
- Free tier is generous for learning

### The OAuth2 Flow [`concept`]

```
Frontend → Auth0 (login) → gets JWT → sends to API → API verifies → allows/denies
```

The API never talks to Auth0 at login time. It only fetches Auth0's **public keys** (once, at startup) to verify token signatures.

### Auth0 Setup — Two Registrations [`concept`]

Auth0 needs to know about both your frontend and backend separately:

- **Application** (type: Single Page App) — represents the React frontend. Gets a Client ID.
- **API** — represents the ASP.NET backend. Gets an Audience identifier (`https://cv-api`).

These are different things! The Application is what users log into. The API is what validates tokens.

### Key JWT Concepts [`concept`]

- **Authority** — the URL of the identity provider (e.g., `https://criveravaldez.uk.auth0.com/`). The API uses this to fetch public keys for signature verification. This is public info, safe to commit.
- **Audience** — an identifier for your API (e.g., `https://cv-api`). Ensures a token was issued *for this specific API*, not some other service. Also safe to commit.
- **Client Secret** — private, NEVER commit. Only the frontend uses this (or machine-to-machine flows).
- **Bearer scheme** — the `Authorization: Bearer <token>` header format. "Bearer" means "whoever bears this token gets access."

### What's Safe to Commit? [`security`]

- **Safe:** Authority URL, Audience identifier (both are public/non-secret)
- **NOT safe:** Client Secret, MongoDB connection string, tokens

### Adding JWT Auth in ASP.NET [`code`]

Three steps in `Program.cs`:

1. **Register services:** `AddAuthentication().AddJwtBearer()` with Authority + Audience
2. **Add middleware:** `UseAuthentication()` then `UseAuthorization()` (order matters!)
3. **Protect routes:** `.RequireAuthorization()` on route groups or individual endpoints

Middleware order matters: CORS → Authentication → Authorization.

### CORS (Cross-Origin Resource Sharing) [`concept`]

Browsers block requests from one origin (e.g., `localhost:3000`) to another (e.g., `localhost:5123`) by default. CORS headers tell the browser "this origin is allowed." Without CORS config, your React frontend can't call your API even if auth is correct.

## 2026-04-01

### Testing with Tokens [`practical`]

Got a test token from Auth0's dashboard (API → Test tab) and verified:
- `curl http://localhost:5123/cv` → **401 Unauthorized** (no token)
- `curl -H "Authorization: Bearer <token>" http://localhost:5123/cv` → **200 OK** with CV data

Also tested via **Scalar UI** (`/scalar/v1`) — use the Auth/Authorize section, select Bearer type, paste just the token (Scalar adds the "Bearer " prefix automatically).

**Gotcha:** Don't put "Bearer" in the header name field and the token in the value. The header name is `Authorization`, the value is `Bearer <token>` all together.

Tokens expire! Check the `exp` claim. Auth0 test tokens typically last 24 hours.

## 2026-05-12

### Frontend Auth0 — A Third Auth0 Registration [`concept`]

The backend JWT work created an **API** in Auth0 (audience `https://cv-api`) and an **M2M Application** (for Insomnia testing). Today added the third piece: a **Single Page Application** registration in Auth0 — this is what the React frontend uses to log real users in.

So now there are three Auth0 entities, each with a job:

| Entity | Purpose |
|---|---|
| **API** (`cv-api`) | Backend identifier. Validates incoming tokens have `aud: https://cv-api`. |
| **M2M Application** (`cv-api-insomnia`) | Lets Insomnia request tokens directly with a client secret — for API testing. |
| **SPA Application** (this session) | Lets the React frontend redirect users to Auth0's login page, get a token back, send it to the API. |

The SPA and M2M get **different Client IDs**. Same API, different ways of getting a token.

### SPA vs M2M — Why Different App Types [`concept`]

- **M2M (Machine-to-Machine):** Backend service exchanging a stored client secret for a token. No user involved. Token represents the *service*.
- **SPA (Single Page App):** User clicks "Log in" → redirected to Auth0 → enters credentials → redirected back with a token. Token represents the *user*. No client secret stored in the browser (browsers can't keep secrets).

This is why SPA Client IDs are **public-safe** but M2M Client Secrets are **NOT**. The SPA flow doesn't rely on the Client ID being secret — it relies on the Allowed Callback URLs whitelist to prevent token theft.

### Installing `@auth0/auth0-react` [`code`]

The official Auth0 React SDK does the heavy lifting:

```bash
npm install @auth0/auth0-react
```

Then wrap the app in `<Auth0Provider>` at the root (`main.tsx`):

```tsx
<Auth0Provider
  domain={import.meta.env.VITE_AUTH0_DOMAIN}
  clientId={import.meta.env.VITE_AUTH0_CLIENT_ID}
  authorizationParams={{
    redirect_uri: window.location.origin,
    audience: 'https://cv-api',
    scope: 'openid profile email cv:write',
  }}
  cacheLocation="localstorage"
>
  <App />
</Auth0Provider>
```

Key bits:
- **`audience`** — must match the API Identifier exactly, or the token Auth0 issues won't include the API's audience claim and the backend will reject it.
- **`scope`** — `openid profile email` get user info; `cv:write` is the custom permission we defined on the API for the Admin role.
- **`cacheLocation: 'localstorage'`** — keeps the token across page refreshes. Tradeoff: XSS-vulnerable. Acceptable for a personal admin app; for a public site with many users this should stay in memory.

### `useAuth0()` Hook [`pattern`]

Once wrapped in the Provider, any component can pull auth state from the hook:

```tsx
const { isAuthenticated, user, loginWithRedirect, logout, isLoading } = useAuth0()
```

Conditional render in the AppBar — show "Log in" when logged out, show name + "Log out" when logged in. The `!isLoading` guard prevents the AppBar flashing "Log in" for a split second on page reload while the SDK restores the session from localStorage.

### The "undefined page" symptom [`mistake`]

First attempt at clicking "Log in" sent the browser to a non-existent page. The cause: `.env.development` didn't yet have `VITE_AUTH0_DOMAIN` or `VITE_AUTH0_CLIENT_ID`, so the SDK was building `https://undefined/authorize?...`.

**Two lessons:**
1. **Vite reads env vars at startup only.** Editing `.env.development` while `npm run dev` is running has no effect — must Ctrl+C and restart.
2. **`VITE_` prefix is required** for Vite to expose an env var to the browser bundle. Anything without the prefix stays server-side and shows up as `undefined` in `import.meta.env`.

### SPA Client ID — Public-Safe by Design [`security`]

A natural reaction is to add `.env.development` to `.gitignore` because "Client ID looks secret-ish." It isn't. SPA Client IDs ship inside the JS bundle — anyone who opens DevTools can read them. The security model relies on:

1. **Allowed Callback URLs** — Auth0 only redirects back to whitelisted origins, so a stolen Client ID can't be used to phish your users elsewhere.
2. **Allowed Web Origins** — restricts which sites can request tokens silently.
3. **Audience** — the API only accepts tokens minted for itself.

This is the opposite of M2M Client Secrets, which **must** stay server-side. Different threat model, different storage rules.

### Still Blocked [`mistake`]

Ended the session unable to complete a login round-trip. Even after env vars were set, the Auth0 login page wouldn't load. Next session debug list:
1. Verify Allowed Callback / Logout / Web Origin URLs on the SPA include `http://localhost:5173` (no trailing slash, exact match)
2. Verify the API Identifier in the Auth0 "APIs" tab is *exactly* `https://cv-api` (matches the `audience` value)
3. Confirm the SPA has `cv-api` listed under its Connections/APIs and `cv:write` is granted
4. Check the browser DevTools Network tab for the failed request — the URL and response code will pinpoint which step is failing

## 2026-05-28

### "Public client" vs "Confidential client" — The Vocabulary That Explains SPA vs M2M [`concept`]

Earlier sessions established that SPAs can't hold a Client Secret because anything in browser JS is readable in DevTools. Today named the underlying distinction:

- **Public client** — cannot keep secrets (SPAs, mobile apps, CLIs). Auth0 type: *Single Page Application* or *Native*.
- **Confidential client** — can keep secrets (backend servers). Auth0 type: *Regular Web Application* or *Machine to Machine*.

This is **OAuth 2.0 vocabulary**, not Auth0-specific. The two categories drive everything else:
- Public clients use **Authorization Code flow with PKCE** (no secret required — uses a one-time challenge instead).
- Confidential clients use **Client Credentials** or **Authorization Code** flow (with secret).

**PKCE** ("pixy" — Proof Key for Code Exchange) is the cryptographic trick that lets a public client prove "the code I'm exchanging is the one I asked for" without a stored secret. The Auth0 React SDK handles PKCE automatically.

### Origin vs URL — Not Interchangeable [`concept`]

Auth0's SPA settings page has three URL fields that look similar but accept different things:

| Field | Accepts | Why |
|---|---|---|
| Allowed Callback URLs | Full URL with path | Auth0 redirects browser here *after login*, with `?code=...`. Should be a dedicated route like `/callback`. |
| Allowed Logout URLs | Full URL (path optional) | Auth0 redirects browser here *after logout*. Usually home page. |
| Allowed Web Origins | **Origin only** (no path) | Controls **CORS** — which JS origins may call Auth0's token endpoint. Browser preflight only sends `Origin:` header (scheme + host + port). A path is *invalid* here. |

**Definitions for the spec-correct vocabulary:**
- **Origin** = `scheme://host:port` (no path, no query). E.g. `https://criveravaldez.dev`.
- **URL** = origin + path + query + fragment.

Easy to conflate; the spec is strict. Auth0 will reject a path on the Web Origins field.

### What Goes In Each Field — Two Environments [`pattern`]

The frontend runs in **two places** (local dev and production), so each field needs two entries:

| Field | Dev | Prod |
|---|---|---|
| Allowed Callback URLs | `http://localhost:5173/callback` | `https://criveravaldez.dev/callback` |
| Allowed Logout URLs | `http://localhost:5173` | `https://criveravaldez.dev` |
| Allowed Web Origins | `http://localhost:5173` | `https://criveravaldez.dev` |

Notes:
- **`http://` for localhost** is correct — Auth0 makes a special exception for localhost in their HTTPS-only rule because TLS on localhost is pointless (traffic doesn't leave the machine).
- **`/callback`** is convention (every Auth0 doc uses it). Naming it `/auth` would work but creates translation cost when reading SDK examples.
- **Logout URL ≠ callback URL.** A common mistake: re-using the callback path for logout. The callback path is *programmed to process an auth code*; sending a logged-out user there will trigger code-exchange logic on a URL with no code, producing confusing errors. Pick a clean landing page.

### "Return To" UX vs Logout URL Allowlist [`pattern`]

Tempting to set Allowed Logout URLs to "wherever the user was." That conflates two things:

- **Allowed Logout URLs** = a *fixed allowlist* in Auth0's dashboard. Not dynamic.
- **The URL chosen at logout time** = decided by the React code calling `logout({ returnTo: ... })`. Must be one of the allowlisted URLs.

So "return to current page" is a code concern (pass `returnTo` dynamically), not a dashboard concern. The allowlist just bounds what's *permitted*.

### Detour: Vite 8 Requires Newer Node — Multi-User nvm-windows Trap [`mistake`]

Tried `npm run dev` and got a stack of errors. Senior habit applied: **read top-down — first error usually causes the rest.**

```
You are using Node.js 20.18.1. Vite requires Node.js version 20.19+ or 22.12+.
```

Below that, a "Cannot find native binding" error from Rolldown (Vite 8's Rust-based bundler) — a *consequence* of the Node mismatch, not an independent bug.

**The nvm-windows mess** turned out to be the real story:

```powershell
PS > (Get-Command node).Source
C:\Users\cristina.valdez\nodejs\node.exe        # ← old manual install winning

PS > $env:NVM_HOME
C:\Users\cvaldez.admin\AppData\Local\nvm        # ← installed under different user!

PS > $env:NVM_SYMLINK
C:\nvm4w\nodejs                                  # ← where nvm's symlink actually lives
```

What was happening:
1. nvm-windows was installed under the `cvaldez.admin` account, not the daily-driver `cristina.valdez` account → it only "worked" in elevated shells (UAC switches user context).
2. The user's PATH had literal `%NVM_HOME%` and `%NVM_SYMLINK%` strings that **didn't expand** (those env vars exist on the admin account, not the regular user).
3. A leftover manual Node 20 install at `C:\Users\cristina.valdez\nodejs` was earlier on PATH than the (broken) nvm references, so it won.

**Fix:** edit User PATH (System Properties → Environment Variables → User variables → Path):
1. Add `C:\nvm4w\nodejs` (the actual nvm symlink target) near the top.
2. Remove `C:\Users\cristina.valdez\nodejs` (the orphan manual install).
3. Optionally drop the un-expanded `%NVM_HOME%` / `%NVM_SYMLINK%` entries.

**Two takeaways:**
- **PATH order is everything on Windows.** `Get-Command <name>` reveals which one wins; `$env:Path -split ';'` shows the priority order.
- **Don't install developer tools under a separate admin account.** It creates persistent friction (PATH, settings, permissions). If unavoidable, at least set the env vars at user level too.

### Detour: Half-Populated `node_modules` After Machine Move [`mistake`]

Initial `npm run dev` failure was just "`vite` is not recognised." Diagnostic:

```powershell
Test-Path node_modules\.bin\vite.cmd
# False
```

So `node_modules/` existed but was incomplete — `vite.cmd` (npm's Windows shim for the `vite` binary) was missing. Likely cause: the folder came across in a machine-to-machine copy / OneDrive sync that didn't preserve everything (especially the `.bin/` symlinks/shims).

**Lesson:** `node_modules/` should never be synced or version-controlled. It's a build artefact, derived deterministically from `package.json` + `package-lock.json`. If in doubt, nuke and reinstall:

```powershell
Remove-Item -Recurse -Force node_modules, package-lock.json
npm install
```

(Removing `package-lock.json` only when there's a known npm-optional-dependency bug — the rolldown error message explicitly recommended it. Normally keep the lock file.)

### Audit Vulnerabilities — Build-Time vs Runtime [`tip`]

`npm install` reported "2 moderate severity vulnerabilities" — `brace-expansion` and `postcss`. Both are **transitive dependencies** (pulled in by ESLint and Vite respectively), and both run only at **build/lint time** on the developer's machine, not in users' browsers.

**The four questions a senior asks about a vuln:**
1. **Severity?** `low / moderate / high / critical`.
2. **Direct or transitive dependency?** Direct = you can pin; transitive = wait for parent to bump.
3. **Runtime or build-time?** Browser-shipped code is the high-stakes surface.
4. **What's the attack vector?** Sometimes requires conditions you don't have.

For a personal training site, build-tool vulns are a hygiene fix, not an emergency. `npm audit fix` is safe when it doesn't introduce major-version bumps (read the dry-run output before running for real).

### Vocabulary deposit [`concept`]

Terms named today that previously felt fuzzy:

- **Public / confidential client** (OAuth 2.0 terminology — defines what flows you're allowed to use)
- **PKCE** (Proof Key for Code Exchange — replaces the secret in public-client flows)
- **Origin** (scheme://host:port, *no path* — what CORS operates on)
- **Transitive dependency** (a dep of a dep — not in your `package.json` directly)
- **Lock file** (`package-lock.json` — pins exact versions so installs are reproducible)
- **PATH precedence** (Windows walks PATH in order; first hit wins)

## 2026-06-05

### Diagnosing the Callback URL Mismatch — Read The Error Literally [`practical`]

First Log-in attempt of the session produced Auth0's error page:

> The provided redirect_uri is not in the list of allowed callback URLs.

Senior habit applied: **the error message is never lying.** Auth0 is naming two pieces of state in conflict:

- **The provided** = what the SDK sent in `?redirect_uri=...` (controlled by the code)
- **The list** = the dashboard's Allowed Callback URLs (controlled by the Auth0 UI)

Don't assume — verify both:

1. **What the SDK sent.** DevTools → Network → click Log in → find request to `/authorize` on the Auth0 domain → read the `redirect_uri` query parameter.
2. **What the dashboard allows.** Auth0 dashboard → Applications → SPA → Settings → Allowed Callback URLs.

Diff the two values character-by-character. The mismatch lives in one of: trailing slash, URL encoding (`%2F` vs `/`), `http` vs `https`, port, casing, or stale bundle (the code edit didn't actually reach the browser).

### Spotting Inconsistencies In Your Own Plan [`pattern`]

The bug that produced the mismatch was a *plan* bug, not a code bug. Two facts that should have been the same were not:

- Code (`main.tsx`): `redirect_uri: window.location.origin` → sent `http://localhost:5173` (root, no path)
- Dashboard (set in 2026-05-28 session): `http://localhost:5173/callback` (with path)

Earlier notes had committed to `/callback` as the convention (matches Auth0 docs, matches the prod entry `https://criveravaldez.dev/callback`). The code didn't get the memo. **A senior reviewing this PR would catch the inconsistency before clicking Log in.** Lesson: when your dashboard and code touch the same string, treat them as one decision — change them together or write the value in one place if you can.

### Template Literal Syntax — Three String Types In JS [`code`]

Tried to type the new value and produced two syntactically broken variants before landing on the right one:

```js
// ❌ plain string literal — braces are just characters, no code runs
redirect_uri: '{window.location.origin}/callback'

// ❌ template literal syntax but missing the $ — still no interpolation
redirect_uri: `{window.location.origin}/callback`

// ✅ template literal with interpolation
redirect_uri: `${window.location.origin}/callback`
```

JavaScript has three string forms:

| Quotes | Type | Interpolation? |
|---|---|---|
| `'...'` | Plain string | No |
| `"..."` | Plain string | No |
| `` `...` `` | Template literal | Yes, via `${expr}` |

Modern convention: backticks when interpolating, single quotes otherwise. The `$` before `{` is non-negotiable — `{foo}` inside backticks is two literal braces around the literal text `foo`.

Editor tip: your syntax highlighter colours interpolated expressions differently from the surrounding string. If `${window.location.origin}` looks the same colour as the rest of the quote, you've made a mistake.

### Rules of Hooks — Why `useNavigate` Needs A Wrapper Component [`concept`]

Wanted to wire `onRedirectCallback` so Auth0 sends the user back to `/` after a successful login. The natural-feeling code is:

```tsx
// main.tsx — at module top level
const navigate = useNavigate()   // ❌ "invalid hook call"

<Auth0Provider onRedirectCallback={() => navigate('/')}>
```

That throws. React has two **Rules of Hooks**:

1. Only call hooks at the **top level of a React function component** (or another custom hook).
2. Never call hooks from module-level code, regular functions, conditionals, or loops.

**Why the rules exist:** React tracks hooks by **call order within a render**. Each render calls the same hooks in the same order, and React maps each call to a slot of internal state. That mapping only exists *during a component render*. Module-level code in `main.tsx` runs **once at app startup, before React has mounted anything** — there's no render in progress, no component context, nowhere for hook state to live.

The fix is a thin wrapper component, which puts the hook call inside a function component body:

```tsx
function Auth0ProviderWithNavigate({ children }) {
  const navigate = useNavigate()        // ✅ inside a function component, during render
  return (
    <Auth0Provider
      ...
      onRedirectCallback={(appState) => navigate(appState?.returnTo || '/')}
    >
      {children}
    </Auth0Provider>
  )
}
```

`react-hooks/rules-of-hooks` (ESLint rule, ships with the Vite React template) catches violations at lint time.

### Provider Tree Ordering — Context Flows Down, So Ancestors Matter [`concept`]

Adding `useNavigate()` inside `Auth0ProviderWithNavigate` introduced a structural requirement: **the `<BrowserRouter>` must be an ancestor of any component that calls a router hook.** Router hooks read from the router's React Context, and Context is only available to descendants of the Provider.

Original tree (broken for `useNavigate` inside Auth0Provider):

```
<Auth0Provider>          ← outer
  <App>
    <BrowserRouter>      ← inner — router context only available below this
      ...
```

New tree:

```
<BrowserRouter>                    ← outer — router context available everywhere below
  <Auth0ProviderWithNavigate>      ← can now call useNavigate()
    <App>
      ...
```

Mechanic: when you change the provider tree at app root, every consumer of those contexts cares about the ordering. **"Which provider wraps which" is an architectural decision, not a styling one.** Whenever a hook complains it can't find its context, look up the tree.

### Context Providers Render No DOM — Fragments Are The Right Replacement [`concept`]

Moving `<BrowserRouter>` out of `App.tsx` left two top-level JSX siblings (`<AppBar>` + `<Box>`) and produced:

> JSX expressions must have one parent element.

JSX compiles to a single `React.createElement(...)` call per element, so a component's return must produce **one root**. Two fixes:

1. **Fragment** (`<>...</>`) — invisible wrapper, no DOM node added.
2. **Real wrapper element** (`<div>`, `<Box>`, etc.) — adds a DOM node.

Choose based on what the *previous* parent did. `<BrowserRouter>` is a **context provider** — it renders no DOM, just makes router state available to descendants. So a Fragment is the equivalent: zero DOM impact, no styling inherited.

**Senior mental model — three buckets of React components:**

| Bucket | Example | Renders DOM? |
|---|---|---|
| DOM elements | `<div>`, `<AppBar>` | Yes — produces HTML |
| Context providers | `<BrowserRouter>`, `<Auth0Provider>`, `<ThemeProvider>` | No — passes data down via Context |
| Logical wrappers | `<Suspense>`, `<ErrorBoundary>` | No — changes render behaviour |

Knowing which bucket a component is in tells you whether removing/replacing it will affect the DOM.

### Vite HMR Has Limits — Restart When You Restructure The Root [`tip`]

After all the wiring fixes (template literal, wrapper component, BrowserRouter move, Fragment), clicking Log in *still* produced the callback mismatch — even though both strings looked identical in DevTools and the dashboard.

Likely cause: **Vite's HMR (Hot Module Replacement) doesn't handle root-level provider tree changes cleanly.** HMR shines for swapping a single component's render output, but when you restructure `main.tsx` — change which providers wrap what, change config passed to a Provider, add a brand-new component — the previous `Auth0Provider` instance may still be running with stale config.

**Heuristic: if you touched the provider tree or app entry point, do a full restart.**

1. `Ctrl+C` in the terminal running `npm run dev`.
2. `npm run dev` again.
3. In the browser, `Ctrl+Shift+R` for a hard reload (bypasses the cached JS bundle).

This extends the 2026-05-12 lesson ("Vite reads env vars at startup only") — same principle applies to provider config and any other one-time initialization at module load.

### Session ended unfinished [`mistake`]

Did not complete the login round-trip this session — stuck on the callback URL mismatch error after all the wiring changes. Restart of Vite is the next move to try. Then if still failing, paste both strings (sent `redirect_uri` and dashboard entry) verbatim to spot invisible differences (trailing slash, encoding, multi-entry list).
