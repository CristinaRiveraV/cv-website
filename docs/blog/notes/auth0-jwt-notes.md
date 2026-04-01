---
topic: Auth0 JWT Authentication
slug: auth0-jwt
status: notes
sessions: [2026-03-31, 2026-04-01]
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
