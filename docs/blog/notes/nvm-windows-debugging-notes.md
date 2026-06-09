---
topic: Debugging nvm-windows on a Split-Account Corporate Windows Machine
slug: nvm-windows-debugging
status: notes
sessions: [2026-06-03]
---

## 2026-06-03

### The setup that bit us [`mistake`]

Day 20 of training was meant to be Auth0 frontend callback wiring. Instead it
turned into a deep debug of why `npm run dev` wouldn't start. Vite 8 refused:

```
You are using Node.js 20.18.1. Vite requires Node.js version 20.19+ or 22.12+.
```

Tight miss — same major, one minor short. The "why is it 20.18.1?" question
took the rest of the hour. The shape of the problem:

- Corporate Windows policy: two accounts on the machine — `cristina.valdez`
  (daily driver, no admin rights) and `cvaldez.admin` (used only for UAC
  elevation).
- nvm-windows had been installed earlier *while elevated as `cvaldez.admin`*.
  Installer defaulted its install path to the elevated user's profile:
  `C:\Users\cvaldez.admin\AppData\Local\nvm`.
- `NVM_SYMLINK` was set to `C:\nvm4w\nodejs` (a SymbolicLink pointing into the
  admin's AppData), and both that and `NVM_HOME` were on the **System** PATH.
- But: `C:\Users\cvaldez.admin\AppData\Local` is ACL-locked to the admin
  user. From `cristina.valdez`'s normal shell, traversing the symlink failed
  silently, PATH lookup fell through, and the next match was an *orphan*
  `C:\Users\cristina.valdez\nodejs` folder containing Node 20.18.1 from some
  earlier manual install.
- So: `node -v` returned 20.18.1 in normal shells and 24.16.0 in admin shells,
  with no obvious sign of *why*.

### The diagnostic that cracked it [`tip`]

`node -v` told us *what* version. It didn't tell us *why*. The command that
actually broke the case open:

```powershell
(Get-Command node).Source
```

Output: `C:\Users\cristina.valdez\nodejs\node.exe`. That single line collapsed
three hypotheses into one: there's a *separate* `node.exe` on PATH that nvm
has never heard of.

**Lesson worth keeping:** whenever a CLI tool behaves like a different version
than expected, the next question is never *"what version"* — it's *"which
binary, from where, picked up by which shell, with which env vars."*
`(Get-Command <tool>).Source` is the five-second answer for any CLI. Works
identically for `dotnet`, `python`, `git`, `java`.

### "Access denied" doesn't always mean ACL [`mistake`]

Mid-cleanup, deleting the orphan folder gave:

```
Remove-Item : Cannot remove item C:\Users\cristina.valdez\nodejs\node.exe:
Access to the path 'node.exe' is denied.
```

First instinct on Windows: "permissions issue, run as admin." Wrong. On
Windows, `UnauthorizedAccessException` is the error you get when a file is
**locked by another running process** — same exception class as a real ACL
denial. The fix wasn't elevation, it was finding the holder:

```powershell
Get-Process node -ErrorAction SilentlyContinue | Select-Object Id, Path, StartTime
Get-Process node | Stop-Process -Force
```

Culprit was almost certainly VS Code's TypeScript language server (which
spawns `node.exe` for itself). Closing it released the lock and the delete
worked first try after that.

**Rule of thumb:** on Windows, "Access denied" on a file *you own* almost
always means "in use," not "ACL." Check `Get-Process` before reaching for
admin.

### Why a fresh shell matters (env-var snapshots) [`concept`]

After the uninstall, `$env:NVM_HOME` in the *running* admin shell still
showed `C:\Users\cvaldez.admin\AppData\Local\nvm` even though the uninstaller
had run successfully. Not a failed uninstall — a stale snapshot.

Environment variables are **copied into a process when it starts**. PowerShell
doesn't re-read the registry mid-session. Windows broadcasts a
`WM_SETTINGCHANGE` message when env vars change; Explorer listens (which is
why a new shell launched from Explorer sees the new value), but PowerShell
ignores it.

So: registry has the new value, your current shell has the old one, until
you open a new shell. **Always rule out "stale shell" before theorising about
scope or permissions.** This wasted ~5 minutes today and is exactly the kind
of thing that ages well.

### Reinstalling: override the default install path [`pattern`]

The reinstall is where the corporate UAC trap is easy to fall into a second
time. The installer was relaunched via "Run as administrator," which prompts
for `cvaldez.admin` credentials. Inside the installer process, "current user"
resolves to `cvaldez.admin` — same as the original install. If you click
through accepting defaults, the new nvm install lands in *the same broken
place as the old one*.

The fix: at the "Select Destination Location" screen, **manually change the
path** from the defaulted `C:\Users\cvaldez.admin\AppData\Local\nvm` to
`C:\Users\cristina.valdez\AppData\Local\nvm`. The admin process can write
into the normal user's AppData (admins have access to all user profiles), so
this works. The `NVM_SYMLINK` location at `C:\nvm4w\nodejs` is fine as the
default — it lives outside `Program Files`, so future `nvm use X` calls don't
need admin to flip the symlink.

**General principle:** any installer's "current user" scope is the
*elevated* user under UAC. On corporate machines with split accounts, that's
the wrong user. Always check install paths before clicking Next.

### Pin Node version per repo [`pattern`]

Once Node 22.12.0 was installed and working, added `.nvmrc` to
`src/cv-frontend/`:

```
22.12.0
```

Caveat learned: **nvm-windows doesn't auto-read `.nvmrc` on `cd`** like Unix
nvm does. The file is documentation-with-tooling-hooks — `nvm use` here is
still a manual call. Worth referencing in the project README so it's
discoverable.

### Summary — the diagnostic order that emerged

When a CLI is "the wrong version":

1. `(Get-Command <tool>).Source` — *which* binary
2. Compare User PATH vs Machine PATH vs Process PATH (`[Environment]::GetEnvironmentVariable("PATH", "Machine"|"User")`)
3. Rule out stale shells (open a fresh one before theorising)
4. Check for orphan installs on PATH that predate your version manager
5. On Windows, "Access denied" → `Get-Process` first, elevation second
