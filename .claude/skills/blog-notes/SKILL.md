---
name: blog-notes
description: >
  Track learning notes during training sessions for blog posts.
  Trigger when: user says /blog-note to capture a learning moment,
  /blog-draft to compile a final blog post from accumulated notes,
  or /blog-status to see current notes for a topic.
  Each topic gets its own notes file and final post file.
---

# Blog Notes — Training Session Note Tracker

Capture learning moments during training sessions and compile them into blog posts. Notes are organised per topic (not per day), accumulated across sessions, and compiled into a final draft when the topic is complete.

## File Structure

```
docs/blog/
├── README.md                          # Index of published posts
├── notes/
│   └── <topic-slug>-notes.md          # Running notes (one per topic)
└── <topic-slug>.md                    # Final published post
```

## Instructions

### /blog-note — Capture a Learning Moment

**When:** Any time during a training session when something noteworthy happens — a concept explained, a design decision made, a mistake corrected, a pattern discovered.

**How:**

1. Determine the current topic slug from context (e.g. `csharp-oop-solid`, `dotnet-config-setup`). If unsure, ask the user.
2. Open or create `docs/blog/notes/<topic-slug>-notes.md`
3. Append a new entry under the current date heading with:
   - **What happened** — brief description of the moment
   - **Key insight** — the learning takeaway
   - **Code snippet** (if relevant) — short example
   - **Category tag** — one of: `concept`, `decision`, `mistake`, `pattern`, `tip`
4. If the file is new, add frontmatter:

```markdown
---
topic: <Topic Title>
slug: <topic-slug>
status: notes
sessions: []
---
```

5. Add/update the current date in the `sessions` array if not already there.

**Note format:**

```markdown
## <Date>

### <Short title> [`concept`|`decision`|`mistake`|`pattern`|`tip`]

<What happened and the key insight.>

```<language>
// code snippet if relevant
`` `
```

**Important:** Capture notes naturally as they come up — don't wait until the end of the session. The user may also describe what to note, or you may proactively suggest noting something significant. Always confirm with the user before adding a note.

### /blog-status — Check Notes for a Topic

**When:** User wants to see what notes have been accumulated.

**How:**

1. If a topic slug is provided, read that topic's notes file
2. If no slug provided, list all files in `docs/blog/notes/` and show a summary
3. Show: topic name, number of sessions, number of notes, date range

### /blog-draft — Compile Final Blog Post

**When:** A topic is complete and the user wants to write the final post.

**How:**

1. Read the notes file for the topic
2. Present a suggested outline based on the notes, grouped by theme (not chronologically)
3. Discuss the outline with the user — what to keep, cut, reorder, expand
4. Write the final post to `docs/blog/<topic-slug>.md` with this structure:

```markdown
# <Title>

**Sessions:** <date range>

<Introduction — what we set out to do and why>

## <Section per theme>

<Narrative explanation with code examples>

## What I Learned

<Key takeaways, listed>

## What's Next

<What follows from this topic>
```

5. Update `docs/blog/README.md` to add/update the post link
6. Update the notes file frontmatter: `status: published`

### Proactive Behaviour

During training sessions, you should **proactively suggest** capturing a note when:

- A design decision is made with interesting trade-offs
- The user has an "aha moment" or asks a good clarifying question
- A mistake leads to a useful lesson
- A concept is explained that ties multiple things together
- Something surprising happens (unexpected behaviour, counterintuitive pattern)

Suggest with something like: *"That's a good one for the blog — want me to capture a note about [topic]?"*

### Topic Naming Convention

Use kebab-case slugs that describe the topic, not the session:
- `csharp-oop-solid` (not `day-01`)
- `dotnet-configuration` (not `day-03`)
- `react-component-basics` (not `session-5`)
