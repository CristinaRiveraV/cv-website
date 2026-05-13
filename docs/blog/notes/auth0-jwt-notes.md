---
topic: Auth0 JWT Authentication
slug: auth0-jwt
status: notes
sessions: [2026-03-31, 2026-04-01, 2026-05-12]
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
