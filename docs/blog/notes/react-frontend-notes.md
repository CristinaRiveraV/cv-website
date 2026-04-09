---
topic: "React Frontend Setup"
slug: react-frontend
status: notes
sessions: [2026-04-08, 2026-04-09]
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
