---
topic: "CI/CD with GitHub Actions"
slug: ci-cd-github-actions
status: notes
sessions: [2026-04-16, 2026-04-17]
---

## 2026-04-16

### CI vs CD — two halves of automation [`concept`]

CI (Continuous Integration) and CD (Continuous Deployment) are often mentioned together but serve different purposes. CI automatically runs checks (build, tests, linting) on every push or PR — it catches problems before they reach the main branch. CD automatically deploys your app when code is merged — it eliminates manual deployment steps. You don't need both from the same tool. In this project, Render handles CD (auto-deploys on commit to `main`) and GitHub Actions handles CI (checks on every PR).

### GitHub Actions runs workflows defined in YAML [`concept`]

GitHub Actions is GitHub's built-in CI/CD platform. You define workflows in `.yml` files inside `.github/workflows/`. Each workflow specifies: **when** to run (`on:` — e.g., on pull requests to main), **where** to run (`runs-on:` — e.g., `ubuntu-latest`, a free GitHub-hosted Linux VM), and **what** to do (`steps:` — an ordered list of commands). GitHub automatically detects these files and runs the workflows — no configuration needed in the GitHub UI.

### A workflow can have multiple parallel jobs [`concept`]

Jobs within a workflow run in parallel by default, each on their own fresh VM. This project's CI has two jobs: `build-backend` (restores NuGet packages, builds the .NET solution) and `build-frontend` (installs npm packages, runs ESLint, builds the React app). They don't depend on each other, so they run simultaneously — faster feedback. If one fails, the other still runs, so you see all issues at once rather than fixing one and discovering another.

## 2026-04-17

### YAML indentation is significant and strict [`gotcha`]

YAML uses indentation (spaces only, never tabs) to define structure — similar to Python. In GitHub Actions workflows, list items (steps) start with a `-` dash, and all dashes at the same level must line up vertically. Properties belonging to a step are indented further under the dash. Getting this wrong causes parsing errors. The VS Code YAML extension by Red Hat provides real-time validation with red squiggly underlines, making it much easier to catch formatting issues.

### Pre-built actions save you from reinventing the wheel [`concept`]

GitHub Actions has a marketplace of pre-built actions you can use with `uses:`. Instead of manually installing .NET or Node.js on the CI runner, you use `actions/setup-dotnet@v4` or `actions/setup-node@v4` and specify the version. `actions/checkout@v4` clones your repo into the runner. The `@v4` is a version tag — pinning to a major version means you get bug fixes but no breaking changes.

### CI tool versions must match your project [`debugging`]

The workflow initially used .NET 9 and Node 22, but the project targets .NET 10 (LTS) and was built with Node 24 locally. Mismatched versions caused build failures. Always check your project's actual versions (`TargetFramework` in `.csproj`, `node --version` locally) and match them in the CI configuration. This is a common CI pitfall — what works on your machine won't work in CI if the runtime versions differ.

### npm ci vs npm install in CI environments [`concept`]

`npm ci` (clean install) is the recommended command for CI because it installs exactly what's in `package-lock.json` without modifying it — fast and deterministic. However, it's strict: if `package-lock.json` is even slightly out of sync with `package.json`, it fails. This happened when transitive dependencies (`@emnapi/core`) resolved to different versions than what was in the lock file. `npm install` is more forgiving — it resolves and updates the lock file as needed. For a personal project, `npm install` in CI is a practical choice to avoid lock file sync headaches.

### The working-directory default simplifies monorepo CI [`concept`]

When your project has multiple subdirectories (like `src/cv-frontend` for the frontend), every `run` command would need `cd src/cv-frontend &&` prefixed. The `defaults: run: working-directory:` setting tells every `run` step in that job to execute from a specific directory automatically. This keeps the workflow clean and avoids repetitive path navigation.
