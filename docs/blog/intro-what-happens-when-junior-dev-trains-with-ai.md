---
title: "What Happens When a Junior Dev Trains with AI — A Series"
author: Cristina Rivera Valdez
series: "Junior to Mid: Rebuilding My Portfolio From Scratch"
status: draft
---

# What Happens When a Junior Dev Trains with AI — A Series

## The shift

Software development is changing fast. AI tools can now write code, suggest fixes, and scaffold entire projects in minutes. And with that comes a question I keep hearing: *"Will AI replace developers?"*

I don't think so — but I do think the role is shifting. We're moving from a world where the job is mostly *writing* code to one where it's increasingly about *understanding, reviewing, and debugging* code. And if anything, that makes it more important than ever to actually know what's going on under the hood.

This series is about what happens when a junior developer decides to test that idea — by using AI not as a shortcut, but as a structured learning partner.

## Who I am

I'm Cristina, a C# developer with a couple of years of experience working with .NET, SQL and NoSQL databases, and JavaScript with React on the frontend. I know my way around the stack — I can build features, fix bugs, and ship code.

Recently, I moved to the AI team at my organization. We're exploring how AI can be used as a productivity tool across the company — not just the chatbots we already had deployed on our website, but deeper integrations that can genuinely improve how teams work. Part of my role involves experimenting with AI coding tools, testing what works, and figuring out where the real value is.

That combination — being a junior developer who's also now hands-on with AI tools every day — is what led to this experiment.

## The problem

Here's the thing about being a junior developer: you can know a lot of things without really *knowing* them. I can write a class, set up an API endpoint, build a React component. But if someone asks me *why* I made a particular design decision, or to explain how the pieces fit together, I'm not always confident in my answer.

I wanted to change that. Not by reading more tutorials or watching more courses, but by building something real — from the ground up, making every decision deliberately, and being able to explain each one.

The goal: go from junior to mid-level. Not just in title, but in understanding.

## The experiment

So I set up a challenge for myself: rebuild my portfolio website from scratch using React, C# ASP.NET, and MongoDB — the same technologies I work with daily — but this time with Claude Code as my pair-programming partner and teacher.

The idea isn't to have AI write the code for me. It's the opposite, really. I wanted a partner that could explain concepts when I need them, challenge my assumptions, and help me think through decisions — but still let me be the one writing the code.

I dedicate about an hour a day to this. Each session focuses on a specific topic — C# OOP one day, .NET configuration the next, React components later on. It's structured like a training programme, but flexible enough to go deeper when something interesting comes up.

And because I'm also working with AI tools professionally — testing MCPs, building skills, evaluating what Claude Code can actually do — this project doubles as a real-world experiment. How far can AI-assisted learning actually go? What works? What doesn't? I wanted to find out.

## How we actually work

A typical session looks nothing like "ask AI to write code, copy-paste, done." It's much more like working with a senior developer who has infinite patience.

Here's what actually happens: we discuss a topic, Claude explains the concepts, and then I write the code myself. Along the way, there's a constant back-and-forth — I ask questions, Claude suggests approaches, I push back when something doesn't feel right, and we talk through the trade-offs together. Sometimes I come up with ideas and Claude helps me evaluate them. Sometimes Claude suggests something and I say "no, let's do it differently" — and we do.

For example, during one of our early sessions building the data models for my CV, Claude suggested adding a `SortOrder` property to the `Project` class. It made sense at first glance — you'd want to control the display order of your projects. But when we talked it through, we realised it was a presentation concern, not a property of the project itself. That kind of decision — and the reasoning behind it — is exactly what I was missing before.

In another session, I used `.Distinct()` to remove duplicate skills from a list — and it just didn't work. Two identical skills kept showing up. Claude walked me through why: C# compares objects by reference, not by value, by default. It's something I'd read about before, but never actually hit in practice. We fixed it together, and now I'll never forget the difference.

It's not just coding either. We plan together, we debug together, and at the end of each session we reflect on what we covered. Every design decision gets discussed, every mistake gets understood, not just fixed.

## The meta moment

Here's where it gets a bit meta. From the very beginning, I knew I wanted to document this journey — not just the code, but the decisions, the mistakes, and the "aha" moments. But I also knew that if I left all the writing until the end of the week, I'd forget half of what I'd learned.

So we built a solution inside the tool itself. Claude Code has a feature called "skills" — reusable prompts that you can trigger with a command. We created a custom skill called `/blog-note` that lets me capture a learning moment mid-session, tagged by topic, without breaking my flow. At the end, I can run `/blog-draft` and it compiles all the notes into a structured post.

I didn't plan to build a productivity tool during a training session. But that's kind of the point — when your learning partner is also a development tool, the line between "learning about code" and "building things that help you learn" gets blurry in the best way.

## The roadmap

The training is structured in four phases, each building on the last:

**Phase 1: Foundations** — C# OOP and SOLID principles, ASP.NET minimal APIs, JavaScript core concepts, intro to React, and MongoDB data modelling. This is about making sure the basics are solid and understood, not just memorised.

**Phase 2: Connecting the pieces** — Hooking React up to the API, state management, integrating MongoDB with C#, and building a multi-page site with routing. This is where the stack starts working as a whole.

**Phase 3: Making it real** — Styling, middleware and error handling, unit testing and TDD, and git workflow best practices. The things that separate "it works on my machine" from production-ready code.

**Phase 4: Going deeper** — TypeScript, async programming deep dives, system design basics, and deployment. By this point, the goal is to not just know how to use these tools, but to understand *why* they work the way they do.

Each phase gets its own set of blog posts, covering the topics we worked on, the decisions we made, and the things that surprised me along the way. And once the website is built, the blog itself will live on it — so the project will literally contain its own documentation. The series you're reading will be a section of the portfolio site we're building.

## The bigger picture

There's a narrative going around that AI will replace developers. I think the reality is more nuanced — and more interesting.

AI tools are getting incredibly good at generating code. But generating code and understanding code are not the same thing. Someone still needs to review what the AI wrote, catch the subtle bugs, make the architectural decisions, and — critically — debug things when they break. And things will break.

In my work on the AI team, I've seen this firsthand. We're exploring how AI can improve productivity across the organisation, and one thing that's become clear is that AI works best when the person using it actually understands the domain. The better you understand the code, the better you can direct, evaluate, and correct the AI's output.

That's why I think junior developers especially need to invest in understanding, not just output. The temptation is to let AI do the heavy lifting and move on. But if you don't understand what was generated, you can't debug it when the AI agent goes down at 2am and customers are waiting.

This series is my way of making sure I'm on the right side of that equation.

## What to expect from this series

Each post in this series covers a specific topic from the training — C# OOP, .NET configuration, React state management, and so on. But these aren't tutorials. You won't find step-by-step instructions for building a portfolio site.

Instead, each post focuses on the things I find most valuable as a developer: the *decisions* (why we chose one approach over another), the *mistakes* (what went wrong and what I learned from it), and the *patterns* (techniques and concepts I'll carry into future projects).

If you're a developer at a similar stage — you can write code, but you want to understand it more deeply — I hope you'll find something useful here. And if you're curious about what AI-assisted learning actually looks like in practice, beyond the hype, stick around.

The next post in the series covers C# OOP and SOLID principles — how we built the domain models for a CV, and why every property ended up exactly where it did.
