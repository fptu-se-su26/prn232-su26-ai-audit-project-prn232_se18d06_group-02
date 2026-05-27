# AI Audit Log -- Feature -- Payment Processing

## 1. Project Information

| Item | Detail |
|---|---|
| Course | PRN232 |
| Class | SE18D06 |
| Semester | SU26 |
| Group | Group 2 |
| Project | GearZone -- Multi-Vendor E-Commerce Platform |
| Branch | `feature/payment-processing` |
| Scope | PayOS online payment, COD, wallet top-up, platform transaction tracking, payment webhook handling |
| Date | 2026-05-27 |
| Team | Ho Huy Hoang, Dam Nguyen Khang, Nguyen Sinh Nhat, Phan Tran Cong Vu, Dang Cong Quoc Khanh |
| Primary Owner | Dam Nguyen Khang (DE180417) |

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

**Goal:** Payment webhook security, strategy pattern, timeout handling

Key tasks AI assisted with:
- How do I verify PayOS webhook signatures in ASP.NET Core to prevent spoofed payment callbacks?
- Design a payment strategy pattern that supports PayOS, COD, and wallet balance -- extensible for future methods.
- How should I handle payment timeout -- what if the user closes the browser before paying?

---

## 4. AI Usage Sessions

### Session 1

| Field | Detail |
|---|---|
| Date | 2026-05-27 |
| Tool | Claude Code |
| Purpose | Payment webhook security |
| Related Files | Features/Payments/*, Controllers/Api/CheckoutController.cs (payment endpoints), Infrastructure/External/PayOS*.cs, Infrastructure/External/CodPaymentStrategy.cs |
| AI Involvement | Significant assistance |

**Prompt used:**

```
How do I verify PayOS webhook signatures in ASP.NET Core to prevent spoofed payment callbacks?
```

**AI output summary:**

Compute HMAC-SHA256 of sorted payload fields using PayOS secret key; compare with webhook signature header; reject if mismatch.

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
| Related Files | Features/Payments/*, Controllers/Api/CheckoutController.cs (payment endpoints), Infrastructure/External/PayOS*.cs, Infrastructure/External/CodPaymentStrategy.cs |
| AI Involvement | Moderate assistance |

**Prompt used:**

```
Design a payment strategy pattern that supports PayOS, COD, and wallet balance -- extensible for future methods.
```

**AI output summary:**

IPaymentStrategy with ProcessAsync(order) and ValidateAsync(order); factory selects strategy by PaymentMethod enum.

**What the team used:**
The algorithmic or architectural pattern described above.

**What the team changed:**
Error handling, edge cases, and integration with the rest of the codebase were added manually.

---

## 5. AI Assistance Summary Table

| Area | No AI | Some AI | Heavy AI | AI Generated | Notes |
|---|:---:|:---:|:---:|:---:|---|
| Webhook security |  |  | X |  | AI security pattern -- critical |
| Strategy pattern |  | X |  |  | AI design, team extended |
| Timeout handling |  | X |  |  | AI job pattern |

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
