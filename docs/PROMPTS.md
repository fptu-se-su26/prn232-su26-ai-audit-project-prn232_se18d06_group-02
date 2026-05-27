# AI Prompts Log -- Feature -- React Frontend

Branch: `feature/frontend-react`
Scope: gearzone-react: Vite + React + TypeScript + Tailwind CSS SPA for the buyer-facing storefront

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** gearzone-react: Vite + React + TypeScript + Tailwind CSS SPA for the buyer-facing storefront

**Prompt:**
> Set up a React + TypeScript + Tailwind CSS v4 + Vite project with React Router v6 for an e-commerce storefront.

**AI Output Summary:**
Vite config with @tailwindcss/vite plugin; tsconfig with strict mode; React Router createBrowserRouter with layout routes.

**Used in files:** gearzone-react/src/**, gearzone-react/package.json, gearzone-react/vite.config.ts

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** gearzone-react: Vite + React + TypeScript + Tailwind CSS SPA for the buyer-facing storefront

**Prompt:**
> Implement an Axios interceptor that attaches a JWT token from localStorage and retries the request once after a 401 with a refreshed token.

**AI Output Summary:**
Axios request interceptor adds Authorization header; response interceptor catches 401, calls /refresh-token, retries original request once.

**Used in files:** gearzone-react/src/**, gearzone-react/package.json, gearzone-react/vite.config.ts

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** gearzone-react: Vite + React + TypeScript + Tailwind CSS SPA for the buyer-facing storefront

**Prompt:**
> Design a product variant selector component in React that builds on a database-driven attribute system.

**AI Output Summary:**
VariantSelector component: renders dropdowns from attribute names; tracks selected option per attribute; computes matching ProductVariant on change.

**Used in files:** gearzone-react/src/**, gearzone-react/package.json, gearzone-react/vite.config.ts

---
