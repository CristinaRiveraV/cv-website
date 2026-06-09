---
topic: Git Workflow & PR Craft
slug: git-workflow-pr-craft
status: notes
sessions: [2026-05-21, 2026-05-27]
---

## 2026-05-21

### The always-on tutor-skill loop closing [`pattern`]

**What happened:** Yesterday's work designed the `tutor` Claude Code skill at user level (`~/.claude/skills/tutor/`) and added a one-line rule to this repo's `CLAUDE.md` saying "always load the tutor skill at session start." Today we opened a PR for that rule, squash-merged it to main, and the loop closed: the rule on main is what auto-loaded the tutor skill at the start of this very session.

**Key insight:** Repo rules in `CLAUDE.md` are *live infrastructure*, not documentation. Writing a rule and writing it *into a file the tooling reads* are completely different things — only the second has effect. The reason this session felt different from a normal Claude conversation isn't because I remembered to switch into tutor mode; it's because `CLAUDE.md` told the tooling to do it for me. The plumbing matters more than the intention. Once that line is on main, every future session in this repo starts in tutor mode without me opting in.

---

### PR writing is critique-and-redraft, not first-draft [`pattern`]

**What happened:** Drafted the PR title and description for the tutor-rule PR. First pass:

- Title: *"Adding a new tutoring skill"*
- Body: *"Added a new rule to always use the tutoring skill, as well as a note in the docs to ensure this skill is documented since it live locally on my PC"*

Claude pushed back on three things, I redrafted:

- Title: *"Add instructions to use tutoring skill"*
- Body: *"Added a new rule to always use the tutoring skill which lives locally on my PC. Also created a note to document the creation of said skill (reasoning and discussion) to use later on in the blogs."*

**Key insights:**

- **Imperative, not gerund.** Git/PR convention is imperative mood — *"Add"*, not *"Adding"*. Reason: the message completes the sentence *"This commit will…"*. My commits had been consistent on this everywhere else; the PR title was the one place I slipped.

- **The accuracy check: "what does this PR actually ADD?"** First title said "Adding a new tutoring skill" — but the diff was 1 line in `CLAUDE.md` + a blog notes file. The skill itself lives at user level, outside the repo. A reviewer would have opened the diff expecting to find a skill file and not found one. The title described what I *was thinking about* (the skill), not what the PR *actually changes* (a rule that references it). Title needs to match the diff, not the mental model.

- **Description = "why", not "what".** The diff already shows *what* changed; the body's job is the motivation a reviewer can't see — why is the rule needed, why does the notes file exist, what makes this PR worth merging. Lead with "why".

- **Squash-merge means the PR title becomes the commit message on main forever.** This is the part that compounds. Sloppy PR title → permanently sloppy `git log`. Good PR title → clean, greppable history. Spending two minutes polishing a title isn't pedantry; it's setting the message that future-me will read every time I `git log --oneline` to figure out what shipped.

---

### Squash-merge mechanics — why `git branch -d` refuses after squashing [`concept`]

**What happened:** After squash-merging the PR on GitHub and pulling main, ran `git branch -d docs/tutor-skill-setup` to delete the local copy. Predicted it would delete silently because main was up to date. Reality: it refused with *"the branch is not fully merged"* — even though I'd literally just merged it. The VS UI version of the same operation worked silently.

**Key insight:**

`git branch -d` checks: *"is the tip commit of this branch reachable from HEAD or its upstream?"* With a regular merge commit, yes — your branch's commit literally sits in main's history. With **squash-merge, no** — squashing creates a *brand-new* commit on main with the same content but a different SHA. To git, that new commit and your branch's commit are siblings, not the same commit. Local git has no way to *prove* the work was merged, so it refuses to delete.

The fix is `git branch -D` (capital D, force delete) — safe *as long as you've confirmed the merge in GitHub*. The VS UI silently runs `-D` under the hood, which is why the GUI just worked.

**The bigger trade-off behind picking squash-merge in the first place:**

| | Squash-merge | Merge commit |
|---|---|---|
| Commits on main per PR | 1 | many (every commit on the branch) |
| In-PR commit history preserved on main | No | Yes |
| Revert | `git revert <sha>` — trivial | `git revert -m 1 <merge-sha>` — fiddly |
| `git log` readability | Clean, one line per PR | Noisy, includes WIP commits |
| Local `git branch -d` after merge | Refuses (use `-D`) | Works |

For solo training work, losing in-PR commit history is a feature, not a cost — those commits are mostly noise. The trivial-revert and clean-log payoffs are what I'm optimising for. Trade-off worth understanding even when the answer is obvious.

---

### Meta — what tutor mode actually does for you [`tip`]

**What happened:** First real session with the always-on tutor skill active. Noticed the difference: I got asked questions I would never have thought to ask myself. *"What does this PR actually add?"*, *"Which merge strategy and why?"*, *"Predict what `git branch -d` will do before you run it."* Each of those forced a reasoning step I'd normally skip.

**Key insight:**

The value of the Socratic stance isn't the *content* of the questions — most of them I could have answered if I'd thought to ask. The value is that the questions get asked at all. Tools that just produce the right answer don't build the reasoning muscle that lets you produce the right answer when the tool isn't there.

Predict-before-reveal pays the most when you're rusty. I came into this session a week off and couldn't recall where Auth0 broke last time. If Claude had just summarised the resume notes I'd have absorbed them passively. Instead it asked me to recall first — which surfaced the gap (I genuinely didn't remember) and made the eventual catch-up land properly.

The same logic that justifies the always-on rule in `CLAUDE.md` justifies leaving the Socratic loop intact even when it feels slow. The slowness is the point.

---

## 2026-05-27

### Switching computers mid-project surfaces all the state that *isn't* in the repo [`mistake`]

**What happened:** Picked the project back up on a different machine and the first thing we did was audit git before touching code. Four separate things had drifted, none of which a normal "git clone and go" would have caught:

1. **Git identity had silently fallen back to the wrong email.** This repo had *no* `--local` `user.name`/`user.email`, so it inherited the global config — which on this machine is my **work** identity (`@zeninternet.co.uk`). The proof was in `git log`: the latest WIP commit was authored as my work self, while the two before it (made on the old machine) used my personal GitHub no-reply address. One commit, quietly mis-attributed.

2. **Working-tree files had been deleted but were still committed.** `git status` showed `deleted: pages/Home.tsx`, `Cv.tsx`, `Contact.tsx`, `types/cv.ts`, plus the assets — gone from disk, but `App.tsx` still `import`ed all of them. So the working tree was in a state that *wouldn't compile*, even though HEAD was fine. Recovered with `git restore <paths>`.

3. **Two untracked directories that should never be committed:** `src/cv-frontend/.vite/` (Vite's dependency pre-bundling cache) and `.claude/settings.local.json` (Claude Code's per-machine settings). Neither was in `.gitignore` yet — the frontend ignore covered `node_modules`/`dist` but not `.vite`.

**Key insight:** A git repo is only *part* of your environment. The stuff that bites you on a new machine is precisely the stuff that lives **outside version control by design** — global git config, build caches, editor/tool local settings. The audit-before-you-code habit exists because `git clone` restores the tracked files perfectly and tells you nothing about the three or four things around them that aren't tracked. Reading `git status` *carefully* (a `deleted:` line is not the same as a planned deletion — cross-check against what HEAD has and what the code still references) is what caught the broken working tree before it became a confusing "why won't this build" half an hour later.

The identity one is the sneakiest because git never errors — it just uses *an* identity, silently the wrong one. The fix is a per-repo `git config --local user.email`, which is exactly the kind of state that doesn't travel with the clone.

### A rule on `main` doesn't apply to a branch cut before it merged [`concept`]

**What happened:** Noticed `CLAUDE.md` on this branch had no tutoring rule — yet I *knew* we'd added one (PR #16, the loop-closing moment from 2026-05-21). Turned out the rule was on `main`, and `feature/frontend-auth0-login` had been branched off *before* #16 merged. `git log --oneline HEAD..origin/main` showed exactly one commit on main missing from here: the tutor-rule commit. `git rev-list --left-right --count` confirmed the branches had diverged 1-and-1 (main had the rule; this branch had the unrelated Auth0 WIP commit). Brought it in with `git merge origin/main`.

**Key insight:** `CLAUDE.md` is live infrastructure (noted 2026-05-21) — but infrastructure only applies where it physically exists in the tree. A long-lived feature branch is a *snapshot of main at branch-time* plus your own commits; improvements merged to main afterwards don't reach back to it. This is the everyday argument for keeping feature branches short and merging `main` in regularly: not just to avoid conflicts, but so the branch keeps inheriting the project's evolving rules, CI, and tooling. A week-old branch can quietly be running under last week's instructions.

### Why merge commits get a pass on the message rules [`concept`]

**What happened:** Concluding the `git merge origin/main` opened the editor with git's default message (`Merge remote-tracking branch 'origin/main' into feature/frontend-auth0-login`). Tutor asked whether to keep it or polish it — given how much we'd fussed over imperative mood and "why-not-what" on a *normal* commit message back on 2026-05-21. I noticed the staging area had only the merge content (CLAUDE.md + a blog note from main) staged, while my unrelated gitignore edits and a new blog note were left unstaged/untracked — so concluding the merge would commit *only* the merge. Correctly called that a merge commit shouldn't bundle unrelated changes.

**Key insight:** The commit-message rules I learned (imperative mood, lead with *why*) exist because a **normal commit has a human author who decided what goes in it** — the message is the only place that intent is recorded. A **merge commit's content isn't hand-authored**; git mechanically computes it by combining two histories, so there's no hidden human decision to explain and the auto-generated "Merge X into Y" already carries the useful information. That's why the default merge message is universally accepted while `git commit -m "stuff"` is not. The *optional* upgrade is a one-line *why* when the merge itself was a decision (here: "merged main specifically to pull the tutor rule onto this branch") — cheap insurance for a merge that wasn't just routine catch-up.

The companion lesson: git's staging during a merge is a feature, not a nuisance. Unrelated working-tree changes stay out of the merge commit automatically, which keeps the merge atomic. Bundle the gitignore fixes and blog note into their own commits *after* concluding the merge.
