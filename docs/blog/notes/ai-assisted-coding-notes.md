---
topic: AI-Assisted Coding
slug: ai-assisted-coding
status: notes
sessions: [2026-05-13]
---

## 2026-05-13

### Building a "tutor mode" Claude Code skill [`decision`]

**What happened:** Designed and built a `tutor` Claude Code skill — a senior-dev-mentoring-junior mode that flips Claude's default behaviour from "deliver a working answer" to "make the human learn it". Lives at user level (`~/.claude/skills/tutor/`) so it works in any repo; always-on in this training repo via a `CLAUDE.md` override.

All this came from a conversation i had with another junior developer, which has a similar setup. the main difference is that i want mine to be able to coach me but continue working in other repos.

**Key insights:**

- **The skill is the *stance*, not the content.** What a senior actually does with a junior — Socratic questioning, predict-before-reveal, progressive hints, asking the junior to review their own code before commenting, prompting "what about errors / concurrency / scale", flagging refactoring smells — these are habits, and they can be written down. A skill is just an instruction set that turns those habits on.

- **User-level vs project-level is a real architectural choice.** Putting the skill in this repo only would have meant copy-pasting it into every future training repo (UrlShortener, anything I build next). Putting it at user level means one source of truth, available everywhere. Project-level `CLAUDE.md` is the right place for the *trigger*, not the *content*. Same separation-of-concerns instinct as code: data in one place, references to it elsewhere.

- **Manual activation beat always-on for cross-repo use.** First instinct was "always on in this repo." Better answer: manually activated by phrases like "tutor mode on" / "/tutor", overridable per-repo via `CLAUDE.md`. Why: work repos shouldn't suddenly refuse to write code for you. The override pattern (default = manual, repo-specific = always-on) gives both.

- **Rejected: time-based hook ("tutor mode every day 11am–12pm").** Tempting because it matches the calendar-block mental model, but mid-conversation context switches at 12:00 break flow. The 11–12 window is *human* behaviour — block the calendar, open the training repo, the always-on rule does the rest. Don't ask the tool to enforce what discipline should enforce. I usually have multiple agents working on multiple tasks at the same time, so having the tutor mode activate like this would basically slow down the process for other projects.
 
- **Name collisions matter.** Briefly had two skills both named `tutor` (project + user level). Claude could match either trigger, behaviour would be unpredictable. Resolved by deleting the project-level copy and letting `CLAUDE.md` carry the always-on instruction. Lesson: skill names are a global namespace within a machine.

**Files involved:**

- `~/.claude/skills/tutor/SKILL.md` — the skill itself (single source of truth)
- `<this-repo>/CLAUDE.md` — adds an always-on rule that loads the user-level skill at session start
