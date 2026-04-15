---
topic: "Frontend Deployment (Vercel + Custom Domain)"
slug: frontend-deployment
status: notes
sessions: [2026-04-14, 2026-04-15]
---

## 2026-04-14

### Debugging a 500: always check the logs [`concept`]

The Render API was returning 500 Internal Server Error when hit from `npm run preview`. The root cause was a MongoDB authentication failure — the connection string in Render's environment variables was wrong. Lesson: a 500 from your API means the server crashed handling the request. The fix isn't in the frontend — check the server logs. Render shows logs in the dashboard under "Logs."

### Environment variables live in the hosting platform, not just in code [`concept`]

Even though `.env.production` has `VITE_API_URL`, the backend connection string (`MongoDB__ConnectionString`) lives only in the Render dashboard — never in code. When that value was wrong, the deployed API broke. This is a key distinction: frontend env vars can be committed (they're public URLs), but backend secrets must be set in the hosting platform and never committed.

## 2026-04-15

### A React SPA is a static site [`concept`]

Despite being "dynamic" in the browser, a React Single Page Application is a static site from a hosting perspective. `npm run build` produces plain files: one `index.html`, JavaScript bundles, and CSS. No server-side processing needed — the browser downloads these files and React handles everything client-side (routing, API calls, rendering). This is different from server-side rendering (like Next.js) where the server generates HTML per request. The distinction matters because static hosting is simpler, faster, and usually free.

### Vercel vs Render for frontend hosting [`decision`]

Chose Vercel over Render Static Site for the frontend. Key reason: Render's free tier has cold starts on static sites (same spin-down behavior as the API), meaning visitors could wait for both the frontend AND the API to wake up. Vercel serves static sites from a global CDN with no cold starts — the frontend loads instantly, and only the API call has the Render cold start delay. Splitting frontend and backend across platforms is actually standard practice in production.

### Deploying to Vercel from a monorepo [`concept`]

When the frontend isn't at the repo root (ours is in `src/cv-frontend`), you set the **Root Directory** in Vercel's project settings. Vercel then treats that subdirectory as the project root for build commands. Key settings: Root Directory = `src/cv-frontend`, Framework Preset = Vite (auto-detected), Build Command = `npm run build`, Output Directory = `dist`. Vercel connects to GitHub and auto-deploys on push to `main`.

### DNS connects your domain to your hosting [`concept`]

A domain registrar (like Porkbun) sells and manages domain names but doesn't host web apps. To connect a custom domain to Vercel, you configure DNS records at the registrar. DNS is like the internet's phonebook — it translates human-readable domain names into addresses computers can find. The flow: visitor types `yourdomain.dev` -> DNS lookup -> routes to Vercel's servers -> your site loads.

### A record vs CNAME record [`concept`]

Two types of DNS records used to point a domain at Vercel:

- **A record (Address Record):** Maps a domain directly to an IP address. Used for the root domain (`@` / `yourdomain.dev`). Like saying "the office is at 123 Main Street." Required for root domains because of a technical rule: root domains cannot use CNAME.
- **CNAME record (Canonical Name):** Maps a domain to another domain name, which then resolves to an IP. Used for subdomains (`www`). Like saying "same place as Vercel's building." Preferred for subdomains because if the host changes their IP, the CNAME follows automatically.

```
yourdomain.dev      -> A record     -> 216.198.79.1 (Vercel's IP)
www.yourdomain.dev  -> CNAME record -> xxx.vercel-dns.com -> (Vercel's IP)
```

### DNS propagation takes time [`concept`]

After adding DNS records at the registrar, changes don't take effect instantly. DNS is a distributed system — records are cached by servers worldwide, and it takes time for the new values to spread (propagate). This can take minutes to hours. Vercel automatically detects when records are correct and updates the status from "Invalid Configuration" to verified.
