# Day 1: C# OOP & SOLID — Building My CV Models

**Date:** 10 March 2026

Today I started building the data models for my CV website in C#. Before writing any code, I reviewed the core OOP concepts: classes vs interfaces, inheritance vs composition, and the Single Responsibility Principle. The key takeaway was that interfaces define contracts ("anything that implements me must have these capabilities"), and composition ("has a") is generally preferred over inheritance ("is a") — my `Person` shouldn't inherit from `Skill`, it should contain a list of skills.

Then I created a console app and started modelling my real CV data. I built a `Skill` class with constructor validation to keep proficiency scores between 0 and 10, and extracted the validation into a reusable private method. I designed a `SkillCategory` enum based on my actual CV sections: Backend, Frontend, Database, DevOps, Testing, SoftSkills, and AIandTooling. I also built an `Experience` class with nullable end dates (for current jobs), a `WorkMode` enum for remote/hybrid/on-site, and a `Responsibility` class that distinguishes between day-to-day duties and achievements. Along the way, I organised the project into `Models/` and `Enums/` folders — a common C# convention.

**Still to do:** Education (with Courses), Language (with proficiency levels like Native/Fluent), Project, and Person (which ties everything together). Then wire it all up in Program.cs with my real data.
