# Coding Training Plan

**Goal:** Junior -> Mid-level Developer
**Time commitment:** 1 hour/day
**Started:** March 2026
**Stack:** React (frontend) + C# ASP.NET (backend API) + MongoDB (database)

**Base project:** Rebuild and modernise my existing portfolio site
(original: github.com/CristinaRiveraV/CristinaRiveraV.github.io)

---

## Week 1: Foundations (Days 1-5)

### Day 1: C# OOP Refresher - Classes, Interfaces, and SOLID
- **Concept (20 min):** Classes vs interfaces, inheritance vs composition, Single Responsibility Principle
- **Practice (40 min):** Create C# models for your CV data: `Person`, `Skill`, `Experience`, `Education`
- **Output:** A C# console app that creates your CV data in code and prints it
- **Why:** Everything in the backend builds on clean OOP models

### Day 2: Your First ASP.NET Minimal API
- **Concept (20 min):** What is REST? HTTP verbs (GET, POST, PUT, DELETE), status codes (200, 404, 500), request/response lifecycle
- **Practice (40 min):** Create a Minimal API with one GET endpoint that returns your CV data as JSON
- **Output:** A running API you can hit from the browser at localhost
- **Why:** This is the backbone of your project - React will talk to this

### Day 3: JavaScript Core - Functions, Objects, Arrays
- **Concept (20 min):** Arrow functions, object destructuring, array methods (map, filter, find, reduce)
- **Practice (40 min):** Write JS scripts that transform CV data (e.g. filter skills by category, sort experience by date)
- **Output:** A .js file you can run with `node` in the terminal
- **Why:** You need these fundamentals before touching React

### Day 4: Intro to React - Your First Components
- **Concept (20 min):** What is React? Components, JSX, props. How it differs from your current vanilla HTML approach
- **Practice (40 min):** Create a React app with a simple component that displays your name and a list of skills
- **Output:** A running React app at localhost showing your info
- **Why:** This replaces your current static HTML with something dynamic and maintainable

### Day 5: MongoDB Data Modelling for Your CV
- **Concept (20 min):** Document vs relational thinking, embedding vs referencing, schema design patterns for MongoDB
- **Practice (40 min):** Design your CV database schema - what collections do you need? What gets embedded vs referenced? Create the documents in MongoDB
- **Output:** A written schema design document + data inserted into MongoDB
- **Why:** Good data design now saves pain later. Directly relevant to your DynamoDB objectives too

---

## Progress Tracker

| Day | Topic | Status | Date Completed | Notes |
|-----|-------|--------|----------------|-------|
| 1   | C# OOP & SOLID | Not started | | |
| 2   | ASP.NET Minimal API | Not started | | |
| 3   | JS Fundamentals | Not started | | |
| 4   | Intro to React | Not started | | |
| 5   | MongoDB Data Modelling | Not started | | |

---

## Future Topics (to schedule after Week 1)
- [ ] Connecting React to the API (fetch / async-await)
- [ ] Unit testing & TDD basics (xUnit for C#, Jest for JS)
- [ ] React state management (useState, useEffect)
- [ ] Entity Framework / MongoDB driver in C#
- [ ] Middleware, error handling, logging in ASP.NET
- [ ] React routing (multi-page CV site)
- [ ] CSS/styling in React (styled-components or Tailwind)
- [ ] Git workflow & PR best practices
- [ ] Async programming deeper dive (C# Tasks / JS Promises)
- [ ] System design basics (client-server, WebSockets vs HTTP)
- [ ] DynamoDB basics (transferring MongoDB knowledge)
- [ ] TypeScript introduction
- [ ] Deployment (getting the site live)

---

## How We Work Together
1. Start of session: Quick concept review (I explain, you ask questions)
2. Code together: You write, I guide - not the other way around
3. End of session: Commit your work, note what clicked and what didn't
4. Next session: Brief recap before moving on

---

## Reference: Existing Site Content
Your current site (CristinaRiveraV.github.io) has this content to migrate:
- **Skills:** Programming languages, spoken languages, software, others (with skill bars)
- **Education:** BEng Robotics at Heriot Watt, Audio Engineering, NY High School Diploma
- **Work Experience:** Baker/General Assistant, Class Rep, British Gas placement
- **Projects:** SLAM mapping, Engineers Without Borders, Coffee Shop Sim, Swarm Robotics, Data Mining
- **Other:** Joke API integration, contact page
- **Tech used:** Vanilla HTML/CSS/JS, jQuery, Font Awesome, custom HTML includes, Mocha/Chai tests
