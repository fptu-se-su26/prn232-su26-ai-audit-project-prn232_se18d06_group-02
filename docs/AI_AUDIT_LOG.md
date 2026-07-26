# AI Audit Log -- Develop -- Integration Base

## 1. Project Information

| Item | Detail |
|---|---|
| Course | PRN232 |
| Class | SE18D06 |
| Semester | SU26 |
| Group | Group 2 |
| Project | GearZone -- Multi-Vendor E-Commerce Platform |
| Branch | `develop` |
| Scope | Full project bootstrapping: solution structure, .gitignore, dependency wiring |
| Date | 2026-05-27 |
| Team | Ho Huy Hoang, Dam Nguyen Khang, Nguyen Sinh Nhat, Phan Tran Cong Vu, Dang Cong Quoc Khanh |
| Primary Owner | Ho Huy Hoang (DE180416) |

---

## 2. AI Tools Used

- [x] Claude (Claude Code CLI)
- [x] GitHub Copilot
- [ ] ChatGPT
- [ ] Gemini
- [ ] Cursor
- [ ] Perplexity

---

## 3. AI Usage Goals for This Branch

**Goal:** Architecture design, solution scaffolding, dependency configuration

Key tasks AI assisted with:
- How to structure a Clean Architecture .NET 8 solution for a multi-vendor e-commerce platform?
- What is the correct dependency direction in Clean Architecture and how do I enforce it via csproj references?
- Generate a comprehensive .gitignore for an ASP.NET Core + React/Vite monorepo.

---

## 4. AI Usage Sessions

### Session 1

| Field | Detail |
|---|---|
| Date | 2026-05-27 |
| Tool | Claude Code |
| Purpose | Architecture design |
| Related Files | GearZone.sln, .gitignore, all four projects |
| AI Involvement | Significant assistance |

**Prompt used:**

```
How to structure a Clean Architecture .NET 8 solution for a multi-vendor e-commerce platform?
```

**AI output summary:**

Suggested Domain -> Application <- Infrastructure -> Web dependency graph; Domain has zero references to other projects.

**What the team used:**
The core pattern / design / code structure suggested above.

**What the team changed:**
Adapted to project naming conventions and verified correctness against actual requirements.
Tested in the running application before accepting.

---

### Session 2

| Field | Detail |
|---|---|
| Date | 2026-05-27 |
| Tool | Claude Code / GitHub Copilot |
| Purpose | Follow-up detail implementation |
| Related Files | GearZone.sln, .gitignore, all four projects |
| AI Involvement | Moderate assistance |

**Prompt used:**

```
What is the correct dependency direction in Clean Architecture and how do I enforce it via csproj references?
```

**AI output summary:**

Only Infrastructure and Web may reference Application; Application references only Domain -- enforced by csproj ProjectReference entries.

**What the team used:**
The algorithmic or architectural pattern described above.

**What the team changed:**
Error handling, edge cases, and integration with the rest of the codebase were added manually.

---

## 5. AI Assistance Summary Table

| Area | No AI | Some AI | Heavy AI | AI Generated | Notes |
|---|:---:|:---:|:---:|:---:|---|
| Architecture design |  | X |  |  | Confirmed by team |
| Solution scaffolding |  | X |  |  | AI-suggested, manually adjusted |
| .gitignore creation | X |  |  |  | Team reviewed for missing patterns |

---

## 6. AI Errors / Limitations Observed

| # | Issue | How Detected | Resolution |
|---|---|---|---|
| 1 | Occasionally suggested outdated .NET 6 API syntax | Build error | Replaced with .NET 8 equivalents |
| 2 | Some EF configurations had incorrect cascade rules | FK constraint error at runtime | Manually set DeleteBehavior per relationship |
| 3 | AI sometimes over-engineered solutions | Code review | Simplified to fit project scope |

---

## 7. Verification Methods

- Ran the application end-to-end after implementing AI suggestions
- Compared generated code against official Microsoft/library documentation
- Team code review before merging to develop
- Tested with realistic data (not just happy path)

---

## 8. Team Contribution

| Member | Student ID | Role | AI Used? |
|---|---|---|---|
| Ho Huy Hoang | DE180416 | Leader, Auth & Orders | Yes |
| Dam Nguyen Khang | DE180417 | Domain & Payments | Yes |
| Nguyen Sinh Nhat | DE180430 | Abstractions & Catalog | Yes |
| Phan Tran Cong Vu | DE180494 | Infrastructure & Admin | Yes |
| Dang Cong Quoc Khanh | DE180880 | Repositories & Frontend | Yes |

---

## 9. Academic Integrity Commitment

The team commits that:
- All AI usage has been honestly recorded in this log.
- No AI output was submitted without review and understanding.
- Every team member can explain the code in this branch.
- We are responsible for the correctness of the final product.

| Representative | Date |
|---|---|
| Ho Huy Hoang | 2026-05-27 |

---

## 10. Promotion Campaign & Voucher Implementation

| Field | Detail |
|---|---|
| Date | 2026-07-27 |
| Tool | Codex |
| Purpose | Implement the approved seller promotion campaign and voucher plan across the Clean Architecture solution |
| Prompt Reference | `PROMPTS.md#prompt-4----2026-07-27` |
| AI Involvement | Heavy implementation and verification assistance |

**AI output summary:**

Implemented seller campaign entities and CRUD, product/date overlap validation,
centralized effective pricing, conditional quota and stock updates, voucher
reservation states, authoritative checkout quote, idempotent checkout request
handling, payment/COD lifecycle transitions, financial snapshots,
promotion-aware Razor UI, EF migration, tests, and deployment documentation.

**Project decisions preserved:**

- Campaigns are seller-only and apply to all variants of selected products.
- Existing Super Admin platform vouchers remain compatible.
- Campaign and seller voucher discounts reduce seller commissionable amount;
  platform vouchers do not reduce seller payout.
- Quote does not reserve capacity; place order revalidates and reserves inside
  a transaction.
- Historical financial records are backfilled without recalculating prior
  payouts.

**Human review requirement:**

The team-provided plan was treated as authoritative. The team should review the
migration against a staging backup and complete the PayOS/COD browser acceptance
flow before production deployment.

**Applied to:**

`GearZone.Domain`, `GearZone.Application`, `GearZone.Infrastructure`,
`GearZone.Api`, `GearZone.Web`, `GearZone.Tests`, and
`docs/PROMOTION_CAMPAIGNS_AND_VOUCHERS.md`.

**Verification:**

- `dotnet build GearZone.sln --no-restore` passed.
- `dotnet test GearZone.Tests/GearZone.Tests.csproj --no-restore` passed 64/64.
- EF Core forward migration SQL and rollback SQL were both generated
  successfully without applying changes to the configured database.

---

## 11. Customer Promotion UI Completion

| Field | Detail |
|---|---|
| Date | 2026-07-27 |
| Tool | Codex |
| Purpose | Add missing sale-off information to customer search results and product detail |
| Prompt Reference | `PROMPTS.md#prompt-5----2026-07-27` |
| AI Involvement | Focused UI implementation and verification |

**AI output summary:**

Updated the shared customer product card to show the active campaign discount
percentage, campaign name, original price, effective price, and savings. Updated
product detail to show the campaign summary and end time, and to keep all sale
metadata synchronized with the selected variant.

**Human review requirement:**

The team should confirm campaign colors and wording against the desired product
design and run a browser check with an active percentage campaign and an active
fixed-amount campaign.

**Applied to:**

`GearZone.Web` customer catalog and product detail Razor pages.

**Verification:**

- `dotnet build GearZone.sln --no-restore` passed with 0 errors.
- `dotnet test GearZone.Tests/GearZone.Tests.csproj --no-build --no-restore`
  passed 64/64.

---

## 12. Checkout Cart-Cleanup Concurrency Fix

| Field | Detail |
|---|---|
| Date | 2026-07-27 |
| Tool | Codex |
| Purpose | Diagnose and fix checkout `DbUpdateConcurrencyException` |
| Prompt Reference | `PROMPTS.md#prompt-6----2026-07-27` |
| AI Involvement | Root-cause analysis, implementation, and regression testing |

**AI output summary:**

Changed purchased-cart cleanup from tracked `RemoveRange` deletes to an
idempotent `ExecuteDeleteAsync` operation. Payment persistence and cart cleanup
now run in one database transaction, and the checkout UI disables repeated
submissions while processing. Follow-up database inspection confirmed that
failed attempts had committed orders but no payment rows. The final correction
clears the quote/reservation ChangeTracker boundary and explicitly adds only the
new payment to the post-payment transaction, preventing stale row-version
entities from entering that save batch.

**Human review requirement:**

The team should exercise COD and PayOS checkout in the browser, including a
double-click/repeated-submit scenario.

**Applied to:**

`GearZone.Application`, `GearZone.Infrastructure`, `GearZone.Web`, and
`GearZone.Tests`.

**Verification:**

- `dotnet build GearZone.sln --no-restore` passed with 0 errors.
- Checkout concurrency regression test passed.
- Full test suite passed 65/65.
