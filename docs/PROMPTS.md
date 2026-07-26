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

## Prompt 4 -- 2026-07-27

**Tool:** Codex
**Context:** Promotion Campaign & Voucher implementation across the GearZone
Clean Architecture solution

**Prompt:**
> Implement the approved Promotion Campaign & Voucher plan for
> `GearZone.Web`, `GearZone.Api`, and the related Clean Architecture projects,
> including seller campaigns, seller order/shipping vouchers, authoritative
> checkout quote, atomic reservation lifecycle, order snapshots, payout rules,
> migration, tests, and audit documentation.

**AI Output Summary:**
Implemented the domain model, EF Core persistence and migration, application
services, atomic checkout/payment lifecycle, seller and checkout APIs, Razor
pages, promotion-aware pricing displays, financial snapshots, and automated
tests.

**Used in files:** `GearZone.Domain`, `GearZone.Application`,
`GearZone.Infrastructure`, `GearZone.Api`, `GearZone.Web`, `GearZone.Tests`,
and `docs/PROMOTION_CAMPAIGNS_AND_VOUCHERS.md`

---

## Prompt 5 -- 2026-07-27

**Tool:** Codex
**Context:** Complete customer-facing promotion presentation

**Prompt:**
> Add the missing sale-off presentation to the customer search results and
> product detail page.

**AI Output Summary:**
Enhanced shared product cards with campaign discount badges, campaign names,
original prices, and savings. Enhanced product detail pricing with a campaign
summary, end time, discount percentage, savings, and variant-aware UI updates.

**Used in files:** `GearZone.Web/Pages/Shared/_ProductCard.cshtml`,
`GearZone.Web/Pages/Public/Catalog/Partials/_ProductDetailContent.cshtml`, and
`GearZone.Web/Pages/Public/Catalog/ProductDetail.cshtml`

---

## Prompt 6 -- 2026-07-27

**Tool:** Codex
**Context:** Diagnose and fix checkout optimistic concurrency failure

**Prompt:**
> Diagnose the `DbUpdateConcurrencyException` raised after pressing the checkout
> payment button and fix the checkout flow.

**AI Output Summary:**
Identified the failure in post-payment cart cleanup. Replaced tracked per-row
cart deletion with an idempotent conditional delete, made payment persistence
and cart cleanup transactional, prevented repeated form submission, and added a
regression test for repeated cart cleanup.

**Used in files:** `GearZone.Application/Features/Checkout/CheckoutService.cs`,
`GearZone.Infrastructure/Repositories/CartItemRepository.cs`,
`GearZone.Web/Pages/Checkout/Index.cshtml`, and
`GearZone.Tests/CheckoutConcurrencyTests.cs`

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
