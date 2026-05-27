# AI Audit Log -- Feature -- Product Catalog Browsing

## 1. Project Information

| Item | Detail |
|---|---|
| Course | PRN232 |
| Class | SE18D06 |
| Semester | SU26 |
| Group | Group 2 |
| Project | GearZone -- Multi-Vendor E-Commerce Platform |
| Branch | `feature/catalog-browsing` |
| Scope | Public product listing, search, filtering by category/brand/price/rating, product detail page, store profile browsing |
| Date | 2026-05-27 |
| Team | Ho Huy Hoang, Dam Nguyen Khang, Nguyen Sinh Nhat, Phan Tran Cong Vu, Dang Cong Quoc Khanh |
| Primary Owner | Nguyen Sinh Nhat (DE180430) |

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

**Goal:** Complex query design, pagination strategy, product filtering UX

Key tasks AI assisted with:
- Design a composable product filtering system in EF Core that supports category, brand, price range, and custom attributes without N+1 queries.
- How do I implement cursor-based pagination in ASP.NET Core for a product listing API?
- What is the best way to render a product variant selector from a database-driven attribute system?

---

## 4. AI Usage Sessions

### Session 1

| Field | Detail |
|---|---|
| Date | 2026-05-27 |
| Tool | Claude Code |
| Purpose | Complex query design |
| Related Files | Features/Catalog/*, Controllers/Api/CatalogController.cs, Pages/Public/Catalog/*, Application/Common/ProductSpecifications/ |
| AI Involvement | Significant assistance |

**Prompt used:**

```
Design a composable product filtering system in EF Core that supports category, brand, price range, and custom attributes without N+1 queries.
```

**AI output summary:**

Specification pattern with IQueryable<Product> chaining; ProjectTo<ProductDto> with AutoMapper to avoid SELECT *.

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
| Related Files | Features/Catalog/*, Controllers/Api/CatalogController.cs, Pages/Public/Catalog/*, Application/Common/ProductSpecifications/ |
| AI Involvement | Moderate assistance |

**Prompt used:**

```
How do I implement cursor-based pagination in ASP.NET Core for a product listing API?
```

**AI output summary:**

Keyset pagination using (LastProductId, LastCreatedAt) tuple; more performant than OFFSET for large datasets.

**What the team used:**
The algorithmic or architectural pattern described above.

**What the team changed:**
Error handling, edge cases, and integration with the rest of the codebase were added manually.

---

## 5. AI Assistance Summary Table

| Area | No AI | Some AI | Heavy AI | AI Generated | Notes |
|---|:---:|:---:|:---:|:---:|---|
| Filter query design |  |  | X |  | Specification pattern from AI |
| Pagination |  | X |  |  | AI pattern, team tested edge cases |
| Variant selector |  | X |  |  | AI UI logic, team integrated |

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
