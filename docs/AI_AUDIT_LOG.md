# AI Audit Log -- Feature -- Product Reviews

## 1. Project Information

| Item | Detail |
|---|---|
| Course | PRN232 |
| Class | SE18D06 |
| Semester | SU26 |
| Group | Group 2 |
| Project | GearZone -- Multi-Vendor E-Commerce Platform |
| Branch | `feature/product-reviews` |
| Scope | Buyer product reviews after order delivery: rating, comment, seller reply, admin moderation |
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

**Goal:** Review eligibility gating, moderation design, rating aggregation

Key tasks AI assisted with:
- How do I gate product reviews behind order completion -- only buyers who purchased can review?
- Design a review system with seller reply functionality and admin moderation for an e-commerce platform.
- How do I efficiently compute average product rating while keeping it accurate as reviews are added/deleted?

---

## 4. AI Usage Sessions

### Session 1

| Field | Detail |
|---|---|
| Date | 2026-05-27 |
| Tool | Claude Code |
| Purpose | Review eligibility gating |
| Related Files | Features/Reviews/*, Controllers/Api/ReviewsController.cs, Pages/StoreOwner/Reviews/*, Repositories/ProductReviewRepository.cs |
| AI Involvement | Significant assistance |

**Prompt used:**

```
How do I gate product reviews behind order completion -- only buyers who purchased can review?
```

**AI output summary:**

Check SubOrder.Status=Completed AND SubOrder contains the reviewed product for the requesting user before allowing review creation.

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
| Related Files | Features/Reviews/*, Controllers/Api/ReviewsController.cs, Pages/StoreOwner/Reviews/*, Repositories/ProductReviewRepository.cs |
| AI Involvement | Moderate assistance |

**Prompt used:**

```
Design a review system with seller reply functionality and admin moderation for an e-commerce platform.
```

**AI output summary:**

ProductReview.SellerReplyText + SellerRepliedAt; seller can only reply to reviews for their own store products.

**What the team used:**
The algorithmic or architectural pattern described above.

**What the team changed:**
Error handling, edge cases, and integration with the rest of the codebase were added manually.

---

## 5. AI Assistance Summary Table

| Area | No AI | Some AI | Heavy AI | AI Generated | Notes |
|---|:---:|:---:|:---:|:---:|---|
| Eligibility gating |  |  | X |  | AI LINQ query, team tested |
| Seller reply |  | X |  |  | AI design, team implemented |
| Rating aggregation | X |  |  |  | Team-decided, AI confirmed approach |

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
