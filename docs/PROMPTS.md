# AI Prompts Log -- Develop -- Integration Base

Branch: `develop`
Scope: Full project bootstrapping: solution structure, .gitignore, dependency wiring

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Full project bootstrapping: solution structure, .gitignore, dependency wiring

**Prompt:**
> How to structure a Clean Architecture .NET 8 solution for a multi-vendor e-commerce platform?

**AI Output Summary:**
Suggested Domain -> Application <- Infrastructure -> Web dependency graph; Domain has zero references to other projects.

**Used in files:** GearZone.sln, .gitignore, all four projects

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Full project bootstrapping: solution structure, .gitignore, dependency wiring

**Prompt:**
> What is the correct dependency direction in Clean Architecture and how do I enforce it via csproj references?

**AI Output Summary:**
Only Infrastructure and Web may reference Application; Application references only Domain -- enforced by csproj ProjectReference entries.

**Used in files:** GearZone.sln, .gitignore, all four projects

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Full project bootstrapping: solution structure, .gitignore, dependency wiring

**Prompt:**
> Generate a comprehensive .gitignore for an ASP.NET Core + React/Vite monorepo.

**AI Output Summary:**
Produced .gitignore covering bin/, obj/, node_modules/, .vs/, *.user, dist/.

**Used in files:** GearZone.sln, .gitignore, all four projects

---
