# AI Audit Log -- Core -- Repository Implementations

## 1. Project Information

| Item | Detail |
|---|---|
| Course | PRN232 |
| Class | SE18D06 |
| Semester | SU26 |
| Group | Group 2 |
| Project | GearZone -- Multi-Vendor E-Commerce Platform |
| Branch | `core/infrastructure-repositories` |
| Scope | GearZone.Infrastructure/Repositories: generic Repository<T,TKey>, UnitOfWork, all domain-specific repository implementations |
| Date | 2026-05-27 |
| Team | Ho Huy Hoang, Dam Nguyen Khang, Nguyen Sinh Nhat, Phan Tran Cong Vu, Dang Cong Quoc Khanh |
| Primary Owner | Dang Cong Quoc Khanh (DE180880) |

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

**Goal:** Repository pattern implementation, EF Core query optimization, transaction management

Key tasks AI assisted with:
- Implement a generic repository pattern with EF Core that supports eager loading via Expression<Func<T,object>>.
- How should UnitOfWork be implemented to coordinate multiple repositories in a single DbContext?
- When should I override the generic repository methods vs adding domain-specific query methods?

---

## 4. AI Usage Sessions

### Session 1

| Field | Detail |
|---|---|
| Date | 2026-05-27 |
| Tool | Claude Code |
| Purpose | Repository pattern implementation |
| Related Files | GearZone.Infrastructure/Repositories/*.cs |
| AI Involvement | Significant assistance |

**Prompt used:**

```
Implement a generic repository pattern with EF Core that supports eager loading via Expression<Func<T,object>>.
```

**AI output summary:**

Repository<T,TKey> with IQueryable.Include() loop and ApplyIncludes helper; returns IQueryable from Query().

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
| Related Files | GearZone.Infrastructure/Repositories/*.cs |
| AI Involvement | Moderate assistance |

**Prompt used:**

```
How should UnitOfWork be implemented to coordinate multiple repositories in a single DbContext?
```

**AI output summary:**

UnitOfWork holds a single DbContext instance; SaveChangesAsync commits all tracked changes atomically.

**What the team used:**
The algorithmic or architectural pattern described above.

**What the team changed:**
Error handling, edge cases, and integration with the rest of the codebase were added manually.

---

## 5. AI Assistance Summary Table

| Area | No AI | Some AI | Heavy AI | AI Generated | Notes |
|---|:---:|:---:|:---:|:---:|---|
| Generic repository |  |  | X |  | AI template adapted for project |
| UnitOfWork |  | X |  |  | AI pattern, team wired DI |
| Domain queries | X |  |  |  | Team-written based on feature needs |

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
