# New Machine Setup — cv-website

Restore order for moving the project to a new machine. Assumes the backup zip (`cv-website-backup-YYYY-MM-DD.zip`) is available and contains: `memory/`, `skills/`, `settings.json`, `settings.local.json`, `.env.development`, `.env.production`.

## Install order

Follow top-to-bottom. Each step depends on the ones above it.

### 1. Foundation

- **Git for Windows** — https://git-scm.com/download/win
  - During install, choose "Git Credential Manager" as the credential helper (default).
- **Browser of choice** — Edge ships with Windows; Chrome/Firefox optional.
- **PowerShell 7** (optional but recommended) — `winget install Microsoft.PowerShell`. Windows PowerShell 5.1 works fine; PS7 is just nicer.

### 2. Node.js (via nvm-windows) — **read this carefully**

The previous machine had a multi-user nvm trap. Don't repeat it. See `docs/blog/notes/nvm-windows-debugging-notes.md` for the full war story.

- Install nvm-windows **from a non-elevated PowerShell, signed in as your daily-driver user.** If UAC pops up during install, the installer will write env vars to the *elevated user's* profile, not yours — which is exactly the trap.
  - https://github.com/coreybutler/nvm-windows/releases (latest `nvm-setup.exe`)
- During install, override the install path to `C:\Users\<you>\AppData\Local\nvm` (under YOUR profile, not Program Files).
- After install, verify in a **new** non-admin PowerShell:
  ```powershell
  $env:NVM_HOME       # should point to C:\Users\<you>\AppData\Local\nvm
  $env:NVM_SYMLINK    # should point somewhere like C:\nvm4w\nodejs
  nvm version
  ```
- Then install Node:
  ```powershell
  nvm install 22.12.0
  nvm use 22.12.0
  node --version      # v22.12.0
  ```
- `.nvmrc` in `src/cv-frontend/` pins this version — once nvm is set up, `nvm use` (no version arg) reads `.nvmrc` automatically.

### 3. .NET SDK 8

- https://dotnet.microsoft.com/download/dotnet/8.0 → SDK installer.
- Verify: `dotnet --version` should report 8.x.

### 4. IDEs

- **Visual Studio 2022** (Community is fine) — https://visualstudio.microsoft.com/
  - Workloads needed: "ASP.NET and web development", ".NET desktop development".
- **ReSharper** — https://www.jetbrains.com/resharper/ (separate install, integrates into VS).
- **VS Code** (secondary editor for React side) — https://code.visualstudio.com/
- **Claude Code** — https://docs.claude.com/en/docs/agents-and-tools/claude-code/install — sign in with your Anthropic account after install.

### 5. Auxiliary tools

- **Insomnia** — https://insomnia.rest/download (API testing — Postman alternative).
- **MongoDB Compass** — https://www.mongodb.com/products/compass (optional GUI for the Atlas cluster).
- **GitHub CLI** (optional) — `winget install GitHub.cli` (for `gh` commands).

## Get the project

### 6. Clone the repo

```powershell
cd C:\
mkdir TrainingProjects
cd TrainingProjects
git clone https://github.com/CristinaRiveraV/cv-website.git
cd cv-website
```

First `git push` or `gh auth login` will trigger Git Credential Manager — log in with your GitHub account in the browser popup. Credentials cache after that.

### 7. Restore from backup zip

Extract the zip into the matching locations:

| In zip | Restore to |
|---|---|
| `memory\` | `C:\Users\<you>\.claude\projects\C--TrainingProjects-cv-website\memory\` |
| `skills\` | `C:\Users\<you>\.claude\skills\` |
| `settings.json` | `C:\Users\<you>\.claude\settings.json` |
| `settings.local.json` | `C:\Users\<you>\.claude\settings.local.json` |
| `.env.development` | `C:\TrainingProjects\cv-website\src\cv-frontend\.env.development` |
| `.env.production` | `C:\TrainingProjects\cv-website\src\cv-frontend\.env.production` |

PowerShell one-liner (adjust source path):
```powershell
Expand-Archive 'C:\path\to\cv-website-backup.zip' -DestinationPath 'C:\Temp\cv-restore' -Force
# Then manually move folders to the destinations above.
```

### 8. Install dependencies

Backend:
```powershell
cd C:\TrainingProjects\cv-website\src\cv-backend
dotnet restore
```

Frontend:
```powershell
cd C:\TrainingProjects\cv-website\src\cv-frontend
npm install
```

## Verify

### 9. Run both apps

Backend (in one terminal):
```powershell
cd C:\TrainingProjects\cv-website\src\cv-backend
dotnet run
# Should listen on http://localhost:5123 (or whatever's in launchSettings.json)
```

Frontend (in another terminal):
```powershell
cd C:\TrainingProjects\cv-website\src\cv-frontend
nvm use            # reads .nvmrc → activates Node 22.12.0
npm run dev
# Should listen on http://localhost:5173
```

Open `http://localhost:5173` — the CV site should load. AppBar should show "Log in".

### 10. Verify Claude Code memory restored

In the project directory:
```powershell
claude
```
Then ask: **"where are we at the training?"** — Claude should recall Day 20 context, the Auth0 callback blocker, and the next-session restart plan. If the answer is generic (no Day 20 detail), the memory folder didn't restore correctly — check the path in step 7.

## Cloud accounts to re-verify (just log in — no install)

These live in the cloud; no local install needed, but you'll need to log in once each:

- **GitHub** — `git push` will prompt via Credential Manager
- **Auth0** — https://manage.auth0.com → tenant `criveravaldez.uk.auth0.com`
- **MongoDB Atlas** — https://cloud.mongodb.com → cluster (connection string already in backend appsettings)
- **Render** — https://dashboard.render.com → cv-api service (auto-deploys on push to main)
- **Vercel** — https://vercel.com/dashboard → cv-frontend (auto-deploys on push to main)
- **Anthropic Console** — https://console.anthropic.com (for Claude Code)

## Gotchas / lessons preserved from previous sessions

1. **nvm-windows under wrong user account** — installed under elevated user, env vars don't reach daily-driver shell. Verify `$env:NVM_HOME` after install. Full diagnosis in `docs/blog/notes/nvm-windows-debugging-notes.md`.
2. **Vite reads env vars at startup only** — `npm run dev` must be restarted after `.env.development` changes; HMR doesn't pick them up.
3. **PATH precedence on Windows** — `(Get-Command node).Source` reveals which Node wins. First hit in `$env:Path -split ';'` is what runs.
4. **`node_modules\` is a build artefact** — never copy it across machines. Always `npm install` fresh.
5. **`.env.development` is gitignored on purpose** — it's not in the repo. The backup zip is the only way to move it across machines.
