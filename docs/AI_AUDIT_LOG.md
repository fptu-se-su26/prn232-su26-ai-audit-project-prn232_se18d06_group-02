# AI Audit Log -- Feature -- Messaging and Chat

## 1. Project Information

| Item | Detail |
|---|---|
| Course | PRN232 |
| Class | SE18D06 |
| Semester | SU26 |
| Group | Group 2 |
| Project | GearZone -- Multi-Vendor E-Commerce Platform |
| Branch | `feature/messaging-chat` |
| Scope | Real-time buyer-seller chat via SignalR: conversation threads, read/unread state, inbox with unread counts |
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

**Goal:** SignalR Hub design, real-time messaging patterns, read state management

Key tasks AI assisted with:
- Design a real-time chat system between buyers and sellers using SignalR in ASP.NET Core -- what is the correct Hub design?
- How do I implement unread message counts efficiently without querying all messages?
- How should I handle SignalR connection management for users who have multiple browser tabs open?

---

## 4. AI Usage Sessions

### Session 1

| Field | Detail |
|---|---|
| Date | 2026-05-27 |
| Tool | Claude Code |
| Purpose | SignalR Hub design |
| Related Files | Features/Chat/*, Controllers/Api/ChatController.cs, Hubs/ChatHub.cs, Pages/Public/User/Messages/*, Pages/StoreOwner/Messages/* |
| AI Involvement | Significant assistance |

**Prompt used:**

```
Design a real-time chat system between buyers and sellers using SignalR in ASP.NET Core -- what is the correct Hub design?
```

**AI output summary:**

ChatHub.SendMessage: persists to DB via ChatService; sends to recipient user group via Clients.User(recipientId); returns to sender for confirmation.

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
| Related Files | Features/Chat/*, Controllers/Api/ChatController.cs, Hubs/ChatHub.cs, Pages/Public/User/Messages/*, Pages/StoreOwner/Messages/* |
| AI Involvement | Moderate assistance |

**Prompt used:**

```
How do I implement unread message counts efficiently without querying all messages?
```

**AI output summary:**

ChatMessage.IsReadByRecipient boolean; unread count = COUNT WHERE IsReadByRecipient=false AND RecipientId=currentUser; updated on conversation open.

**What the team used:**
The algorithmic or architectural pattern described above.

**What the team changed:**
Error handling, edge cases, and integration with the rest of the codebase were added manually.

---

## 5. AI Assistance Summary Table

| Area | No AI | Some AI | Heavy AI | AI Generated | Notes |
|---|:---:|:---:|:---:|:---:|---|
| SignalR Hub |  |  | X |  | AI design, team implemented |
| Unread counts |  | X |  |  | AI query, team optimized |
| Multi-tab |  | X |  |  | AI explained built-in behavior |

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
