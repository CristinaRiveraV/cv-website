---
topic: "React Frontend Setup"
slug: react-frontend
status: notes
sessions: [2026-04-08, 2026-04-09, 2026-04-10, 2026-04-13, 2026-04-21, 2026-04-24]
---

## 2026-04-08

### Vite is the modern React scaffolding tool [`concept`]

Created the React project using Vite (`npm create vite@latest cv-frontend -- --template react-ts`). Vite has replaced Create React App as the standard way to start a React project. It provides a fast dev server with Hot Module Replacement (HMR) — when you save a file, the browser updates instantly without a full page reload.

### React is a Single Page App [`concept`]

There's only one HTML file (`index.html`) with a single `<div id="root">`. The entire app gets injected into that div by `main.tsx`. Different "pages" aren't separate HTML files — React swaps components in and out within the same page. This is fundamentally different from the original portfolio site (CristinaRiveraV.github.io), which used separate `.html`/`.htm` files for each page.

### React project structure maps to C# concepts [`concept`]

Key analogies that make the React world click for a C# developer:

| React/TypeScript | C# Equivalent |
|---|---|
| `package.json` | `.csproj` — lists dependencies |
| `npm install` | `dotnet restore` |
| `npm run dev` | `dotnet run` |
| Component (function returning JSX) | A class that returns HTML |
| `useState()` | A property that triggers UI re-render on change |
| `main.tsx` | `Program.cs` — the entry point |

### React Router for client-side navigation [`concept`]

React Router (`npm install react-router`) handles URL paths client-side — the browser never loads a new page. It maps URL paths to components, similar to how ASP.NET's `MapGet("/cv/experiences", ...)` maps paths to endpoint handlers. The router intercepts URL changes and swaps the rendered component.

### Page structure decision — three tabs, no sub-navigation [`decision`]

Chose to mirror the original portfolio's three-tab structure: **Home** (`/`), **My CV** (`/cv`), and **Contact Me** (`/contact`). The My CV page will be a single scrollable page with sections (Experience, Education, Skills, Languages) rather than sub-tabs. A CV is something people scroll through, not click between tabs for. Matches the original site's approach but with modern React components.

### Build structure first, style later [`decision`]

Decided to build the pages with plain React first, then add a component library (likely **MUI / Material UI**) later for modern, responsive styling. This way you learn how React works without a library getting in the way, and styling becomes a straightforward swap after.

### Components are just functions [`concept`]

The simplest React component is a function that returns JSX (HTML-like syntax). Created three page components (`Home.tsx`, `Cv.tsx`, `Contact.tsx`) as placeholder functions. `export default` makes them importable, like `public` in C#.

```tsx
function Home() {
  return <h1>Home</h1>
}

export default Home
```

## 2026-04-09

### React Router handles client-side navigation with components [`concept`]

Installed `react-router-dom` and set up routing in `App.tsx`. Unlike ASP.NET where routes are configured in `Program.cs` with `MapGet`, React Router uses components: `<BrowserRouter>` wraps the app, `<Routes>` + `<Route>` maps paths to components, and `<NavLink>` replaces `<a>` tags for navigation without full page reloads. `NavLink` automatically adds an `active` CSS class to the current page link.

### JSX is React's way of mixing HTML and JavaScript [`concept`]

JSX (JavaScript XML) lets you write HTML-like syntax inside TypeScript files. Key differences from HTML: `className` instead of `class`, all tags must be self-close, and you can embed JavaScript expressions with `{curly braces}`. It's the React equivalent of Razor syntax (`.cshtml`) in ASP.NET. TypeScript files use `.tsx` extension.

### TypeScript interfaces define API data shapes [`concept`]

Created interfaces in `types/cv.ts` to describe the API response shape — same concept as C# DTOs. Property names use camelCase because ASP.NET's `System.Text.Json` serializes PascalCase to camelCase by default. TypeScript uses `string | null` for nullable types (like C#'s `string?`).

### useState and useEffect are the core React hooks [`concept`]

`useState<Person | null>(null)` creates a reactive variable — when you call the setter, React re-renders the component. Like a property with `INotifyPropertyChanged` in C#. `useEffect(() => { ... }, [])` runs code once when the component loads — like `OnInitializedAsync()` in Blazor. The empty `[]` means "no dependencies, only run once."

### Fetching API data and rendering lists with .map() [`concept`]

Used `fetch()` inside `useEffect` to call the API, then `.map()` to render arrays as JSX. `.map()` is like LINQ's `.Select()` combined with `foreach` — it transforms each array item into a component. The `key` prop helps React track which items changed during re-renders.

### Public CV endpoints — removed RequireAuthorization [`decision`]

Removed `.RequireAuthorization()` from the CV endpoint group so the frontend can fetch data without a JWT token. A portfolio site should be publicly readable. Auth will be added to future write endpoints individually, and Auth0 login in the React frontend is a future session topic.

### MUI (Material UI) chosen for styling [`decision`]

Installed MUI (`@mui/material`, `@emotion/react`, `@emotion/styled`). Decision to build structure first with plain HTML/CSS, then swap in MUI was validated — the hand-written CSS served its purpose for learning but will be replaced by MUI components (`Card`, `Typography`, `Chip`, `AppBar`, etc.) next session.

## 2026-04-10

### MUI replaces custom CSS with pre-built components [`concept`]

Swapped all hand-written CSS for MUI components. The key mapping: `<nav>` became `<AppBar>` + `<Toolbar>`, `<div className="card">` became `<Card>` + `<CardContent>`, `<span className="skill-tag">` became `<Chip>`, headings/paragraphs became `<Typography>`, and loading/error states became `<CircularProgress>` and `<Alert>`. Deleted `App.css` and `index.css` entirely — MUI handles everything.

### The sx prop is CSS-in-JS with theme awareness [`concept`]

MUI's `sx` prop replaces className-based styling. Spacing values are multiples of 8px (so `padding: 2` = 16px). Theme-aware shortcuts like `color: 'primary.main'` and `color: 'text.secondary'` pull from MUI's palette. Layout shorthands: `mx` = margin horizontal, `py` = padding vertical, `mb` = margin bottom.

```tsx
<Box sx={{ maxWidth: 900, mx: 'auto', py: 4, px: 3 }}>
```

### Typography: variant vs component prop [`concept`]

`variant` controls the visual style (size, weight), `component` controls the HTML tag. `<Typography variant="h3" component="h1">` looks like an h3 but renders as `<h1>` in the DOM. This separates visual design from semantic HTML — important for accessibility. `gutterBottom` is a boolean shorthand that adds bottom margin without needing `sx`.

### MUI's component prop enables hybrid components [`concept`]

MUI components accept a `component` prop that changes the underlying HTML element. `<Button component={NavLink} to="/cv">` renders an MUI-styled button that behaves as a React Router link — routing and styling in one element. Same pattern used with `<Button component={Link}>` on the Home page.

### flexGrow for nav bar layout [`concept`]

Used `flexGrow: 1` on the name in the `Toolbar` to push navigation buttons to the right. `Toolbar` is a flex container by default, so the name with `flexGrow: 1` expands to fill all available space, and the button group sits on the far right. A common Material Design pattern.

### @mui/icons-material is a separate package [`tip`]

MUI icons (`EmailIcon`, `LinkedInIcon`, `GitHubIcon`, `PhoneIcon`) come from `@mui/icons-material`, not the core `@mui/material` package. Each icon is its own component import. Used `<ListItemIcon>` to place icons alongside text in the contact list.

### CORS middleware order matters [`mistake`]

The React app (port 5173) couldn't reach the API (port 5123) despite CORS being configured. The problem: `UseHttpsRedirection()` was before `UseCors()` in the middleware pipeline. The redirect response went out *without* CORS headers, and the browser blocked it. Fix: move `UseCors()` before `UseHttpsRedirection()`. Lesson: ASP.NET middleware order is critical — each middleware only processes requests that reach it.

```csharp
// Before (broken): redirect fires before CORS headers are added
app.UseHttpsRedirection();
app.UseCors();

// After (fixed): CORS headers added to all responses, including redirects
app.UseCors();
app.UseHttpsRedirection();
```

### MongoDB Atlas IP whitelist expires between sessions [`tip`]

Got a `System.TimeoutException` with `State: "Disconnected", Servers: []` when the API tried to query MongoDB. The cluster was fine — the local IP had changed since the last session. Fix: update Network Access in MongoDB Atlas. For a training project, `0.0.0.0/0` (allow all) avoids this recurring issue.

### git restore recovers deleted files [`tip`]

Accidentally deleted `App.tsx` instead of `App.css`. Recovered it with `git restore src/cv-frontend/src/App.tsx`, which restores the last committed version. Since the MUI changes hadn't been committed yet, the file came back as the old version and needed re-applying. Lesson: commit often — each commit is a save point.

## 2026-04-13

### Vite environment variables use the VITE_ prefix [`concept`]

Vite controls what gets included in the browser bundle. Only variables prefixed with `VITE_` are exposed to client code via `import.meta.env.VITE_VARIABLE_NAME`. This is a security feature — it prevents accidentally leaking server-side secrets (like database passwords) to the browser. Accessed using `import.meta.env` instead of Node's `process.env`.

### Multiple .env files for different environments [`concept`]

Vite auto-selects `.env` files based on the mode: `.env.development` loads during `npm run dev`, `.env.production` loads during `npm run build`. This means the app automatically points to `localhost` in development and the Render URL in production — no manual switching. There's also `.env.local` (always loaded, git-ignored) for personal overrides.

### Template literals for string interpolation [`concept`]

Replaced single-quoted strings with backtick template literals to embed the env variable: `` `${import.meta.env.VITE_API_URL}/cv` ``. Template literals (`` `...` ``) allow `${expression}` inside strings — similar to C#'s `$"Hello {name}"` string interpolation. Single quotes (`'...'`) don't support this.

### .env.example as documentation [`decision`]

Created `.env.example` with the expected variable names as a template for anyone cloning the repo. The actual `.env.development` and `.env.production` files were committed since they only contain public URLs (no secrets). If secrets are added later, the real env files would be git-ignored and `.env.example` would be the only committed reference.

### npm run preview tests the production build locally [`tip`]

`npm run build` creates production files in `dist/` but doesn't serve them. `npm run preview` serves the built files locally, using production env vars. This lets you verify the production configuration before deploying — the preview hit the Render API URL, which revealed a 500 error that needs debugging next session.

### Render redeployment needed after merging backend changes [`issue`]

The production preview initially returned 401 (Unauthorized) because the deployed API on Render was behind — the `RequireAuthorization` removal from PR #7 hadn't been redeployed. After redeploying, the error changed to 500 (Internal Server Error), which needs investigation next session. Lesson: merging to main doesn't auto-deploy unless Render is configured for auto-deploy on push.

## 2026-04-21

### MUI Grid items must be direct children of a Grid container [`mistake`]

Tried to split the CV page into a left sidebar (Skills, Languages) and right main column (Experience, Education). First attempt had the sidebar `<Grid size={{ xs: 12, md: 4 }}>` floating outside any container and the main column double-wrapped in an unsized `<Grid>`. Result: everything stacked vertically instead of sitting side by side. The fix: one `<Grid container spacing={4}>` with the sidebar Grid and the main Grid as direct children at the same level. Grid items only participate in the flex layout when they're direct children of a container — nested Grids become regular divs.

```tsx
<Grid container spacing={4}>
  <Grid size={{ xs: 12, md: 4 }}>{/* sidebar */}</Grid>
  <Grid size={{ xs: 12, md: 8 }}>{/* main */}</Grid>
</Grid>
```

### Grouping an array by a field with reduce + Object.entries [`pattern`]

To group skills by their `category` field for sectioned display, built an object with `reduce`, then turned it into an iterable of `[category, skills]` pairs with `Object.entries`. The `??=` (nullish coalescing assignment) creates the array on first hit for each category. Same shape as C# LINQ's `GroupBy().Select(g => ...)`.

```tsx
Object.entries(
  cv.allSkills.reduce<Record<string, Skill[]>>((acc, s) => {
    (acc[s.category] ??= []).push(s)
    return acc
  }, {})
).map(([category, skills]) => (
  <Box key={category}>{/* heading + skills */}</Box>
))
```

### LinearProgress value maps 0–100, not the data's native range [`concept`]

Skill proficiency on the backend is `0–10`, but MUI's `<LinearProgress variant="determinate" value={...} />` expects a percentage (`0–100`). Multiplied by 10 to map the ranges: `value={skill.proficiency * 10}`. A good reminder that MUI components standardise on their own ranges — check the docs rather than assuming they'll take whatever number you hand them.

### Inline icon + text via flex + Typography body2 [`pattern`]

For a metadata-style line like "🏠 Manchester, UK", wrapped a `HomeIcon` and a `<Typography variant="body2" color="text.secondary">` in a flex Box. `display: 'flex'` + `alignItems: 'center'` puts them on the same baseline, `gap: 0.5` spaces them, and `fontSize="small"` on the icon keeps it proportional to the body2 text. This is the idiomatic MUI way to build a labelled-icon row — no custom CSS needed.

```tsx
<Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 2 }}>
  <HomeIcon fontSize="small" color="primary" />
  <Typography variant="body2" color="text.secondary">
    Manchester, UK
  </Typography>
</Box>
```

### dotnet dev-certs fixes ERR_CERT_AUTHORITY_INVALID in the browser [`tip`]

The Vite dev site (http://localhost:5173) calling the ASP.NET API over HTTPS (https://localhost:7254) threw `ERR_CERT_AUTHORITY_INVALID` in Chrome. The API was running fine — the browser just didn't trust the self-signed ASP.NET dev certificate. Fixed once with `dotnet dev-certs https --trust` (accept the Windows prompt, restart Chrome). One-time setup per machine.

## 2026-04-24

### Formatting ISO dates with `toLocaleDateString` [`pattern`]

The API returns `IssueDate` as a full ISO 8601 string (`"2024-11-01T00:00:00Z"`) because the backend stores it as `DateTime`. Nobody wants to read that on a CV. The browser's built-in `Intl` API handles it in one line:

```tsx
const formatMonthYear = (iso: string) =>
  new Date(iso).toLocaleDateString('en-UK', { year: 'numeric', month: 'long' })
```

Result: `"November 2024"`. Swapping `month: 'long'` for `month: 'short'` gives `"Nov 2024"`. No date library (`date-fns`, `dayjs`) needed for this level of formatting — `Intl.DateTimeFormat` under the hood is built into every modern browser. Only reach for a library when you need parsing of arbitrary formats, timezone arithmetic, or relative-time output.

### Frontend types drift from backend models if not kept in sync [`mistake`]

Started rendering Certifications, ran the page, got nothing. The API was returning the field correctly and the new section was coded correctly. Turned out `VITE_API_URL` pointed at the live deployed Render API — which auto-deploys from `main` — so the feature branch's API changes hadn't shipped there yet. `cv.certifications` was `undefined` at runtime and `.map()` blew up silently.

Two lessons in one:

1. **TypeScript types describe an API contract, not reality.** `Person.certifications: Certification[]` is a promise about what the server *should* return; if the server doesn't, the runtime doesn't care what the `.d.ts` said.
2. **Be explicit about which API the frontend dev is hitting** — local backend, local `npm run dev` hitting production, or `npm run preview` against production. Each has different guarantees about what fields exist.

Fixed by checking `.env` and confirming the URL matched the running API with the new schema.

### Grouping list items by a parent field with reduce + Object.entries (part two) [`pattern`]

Same pattern as the skills grouping from 21 Apr, applied inside each Experience card. The real CV has responsibilities under themed subheadings like "Backend & API Development (C#/.NET)", "Cloud Engineering (AWS, Serverless, IaC)" etc. Rather than modelling a nested "group of bullets" structure, kept `Responsibility` flat with a `Category` field and grouped on render:

```tsx
Object.entries(
  exp.responsibilities.reduce<Record<string, Responsibility[]>>((acc, r) => {
    (acc[r.category] ??= []).push(r)
    return acc
  }, {})
).map(([category, items]) => (
  <Box key={category}>{/* subheading + bulleted items */}</Box>
))
```

Flat-with-grouping-key is usually better than nested when the grouping might change. If I'd modelled a `ResponsibilityGroup { Category, Items[] }`, reshuffling items across groups would need Mongo array surgery. With a flat list, changing an item's category is a one-field update.

### MUI bullet markers with `ListItemIcon` + `FiberManualRecordIcon` [`pattern`]

MUI's `<List>` and `<ListItem>` don't render visible bullet markers out of the box — they're semantic lists with flex layout. To get a real bullet-point look that stays consistent with the MUI Card/Typography styling around it, combined `ListItemIcon` with the small-font `FiberManualRecordIcon`:

```tsx
<ListItem sx={{ pl: 2 }} alignItems="flex-start">
  <ListItemIcon sx={{ minWidth: 20, mt: 1 }}>
    <FiberManualRecordIcon sx={{ fontSize: 8 }} />
  </ListItemIcon>
  <ListItemText primary={responsibility.description} />
</ListItem>
```

Pinning `minWidth: 20` on the icon slot stops MUI from reserving its default 56px gutter (designed for full-size icons, wildly oversized for a bullet). `alignItems="flex-start"` plus `mt: 1` on the icon aligns the bullet with the first line of the text instead of floating centered vertically when the text wraps across multiple lines.

### Emoji in section headers — Unicode, not an icon [`tip`]

Literally typed emoji into the header text (`💼 Experience`, `🎓 Education`). No icon library, no CSS, no build step — emoji are characters and React renders text. Gave the CV page a small personality boost without the complexity of choosing a `@mui/icons-material` equivalent for each section.

### Backend had fields the frontend didn't know about [`mistake`]

`Responsibility.cs` has had `Category` and `IsAchievement` since Day 1, but the TypeScript `Responsibility` interface only declared `description`. The data was coming across in the JSON all along — it just wasn't typed. When adding the grouping, tried to access `r.category` and TypeScript threw `Property 'category' does not exist on type 'Responsibility'`. Fix: add the missing field to the TS interface.

This kind of drift is inevitable without codegen. OpenAPI schema generation + a TS client generator (`openapi-typescript`, `nswag`) would eliminate it — probably a future session's work.
