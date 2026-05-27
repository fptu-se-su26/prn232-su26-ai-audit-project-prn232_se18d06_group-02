# AI Audit Log -- Feature -- User Profile and Account

## 1. Project Information

| Item | Detail |
|---|---|
| Course | PRN232 |
| Class | SE18D06 |
| Semester | SU26 |
| Group | Group 2 |
| Project | GearZone -- Multi-Vendor E-Commerce Platform |
| Branch | `feature/user-profile` |
| Scope | Buyer profile: view/edit personal info, avatar upload, saved delivery addresses, wallet balance view, order history |
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

**Goal:** Profile management design, image upload validation, wallet UX

Key tasks AI assisted with:
- Design a user profile system for an e-commerce buyer that includes delivery address management with a default address selection.
- How do I validate and upload a user avatar image in ASP.NET Core Razor Pages with size and type restrictions?
- What information should a buyer's wallet transaction history show?

---

## 4. AI Usage Sessions

### Session 1

| Field | Detail |
|---|---|
| Date | 2026-05-27 |
| Tool | Claude Code |
| Purpose | Profile management design |
| Related Files | Features/User/*, Controllers/Api/UsersController.cs, Pages/Public/User/*, Repositories/UserRepository.cs, Repositories/UserAddressRepository.cs |
| AI Involvement | Significant assistance |

**Prompt used:**

```
Design a user profile system for an e-commerce buyer that includes delivery address management with a default address selection.
```

**AI output summary:**

UserAddress table with IsDefault boolean; ensure only one IsDefault per user via EF transaction (unset all, then set selected).

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
| Related Files | Features/User/*, Controllers/Api/UsersController.cs, Pages/Public/User/*, Repositories/UserRepository.cs, Repositories/UserAddressRepository.cs |
| AI Involvement | Moderate assistance |

**Prompt used:**

```
How do I validate and upload a user avatar image in ASP.NET Core Razor Pages with size and type restrictions?
```

**AI output summary:**

Accept image/jpeg, image/png, image/webp; max 5MB; resize to 256x256 before Cloudinary upload to save storage.

**What the team used:**
The algorithmic or architectural pattern described above.

**What the team changed:**
Error handling, edge cases, and integration with the rest of the codebase were added manually.

---

## 5. AI Assistance Summary Table

| Area | No AI | Some AI | Heavy AI | AI Generated | Notes |
|---|:---:|:---:|:---:|:---:|---|
| Default address logic |  | X |  |  | AI transaction pattern |
| Image upload |  | X |  |  | AI validation, team tested |
| Wallet history UI | X |  |  |  | Team-designed display |

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
