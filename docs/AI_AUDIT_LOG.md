# AI Audit Log 

## 1. Project Information

| Item | Detail |
|---|---|
| Course | PRN232 |
| Class | SE18D06 |
| Semester | SU26 |
| Group | Group 2 |
| Project | GearZone -- Multi-Vendor E-Commerce Platform |
| Branches | 
- core/domain-entities  
- core/application-abstractions  
- core/infrastructure-repositories  
- core/infrastructure-ef-config  
- core/logging-infrastructure  
- core/infrastructure-external-services |
| Scope | 
- Domain: Entities, aggregates, value objects, enums  
- Application: Interfaces, service contracts, logging abstraction  
- Infrastructure: Repositories, EF Core config, logging, external APIs |
| Date | 2026-05-27 |
| Team | Ho Huy Hoang, Dam Nguyen Khang, Nguyen Sinh Nhat, Phan Tran Cong Vu, Dang Cong Quoc Khanh |
| Primary Owners | 
- Domain: Dam Nguyen Khang (DE180417)  
- Abstractions: Nguyen Sinh Nhat (DE180430)  
- Repositories: Dang Cong Quoc Khanh (DE180880)  
- EF Config & Infra: Phan Tran Cong Vu (DE180494)  
- External Services: Ho Huy Hoang (DE180416) |

---

## 2. AI Tools Used

- [x] Claude (Claude Code CLI)
- [x] GitHub Copilot
- [ ] ChatGPT
- [ ] Gemini
- [ ] Cursor
- [ ] Perplexity

---

## 3. AI Usage Goals

### Repository & Transactions
- Generic Repository<T,TKey>
- UnitOfWork pattern
- Eager loading with Expression<Func<T, object>>

### Logging & Middleware
- IAppLogger<T> abstraction
- RequestLoggingMiddleware
- Audit Trail logging

### EF Core Configuration
- IEntityTypeConfiguration usage
- Many-to-many relationships
- Enum as string mapping
- Migration & seed strategy

### Domain Modeling
- Aggregate design (Order → SubOrder → OrderItem)
- Base Entity<TKey>
- Payout system

### Application Abstractions
- IRepository, IUnitOfWork design
- Strategy pattern (payment)
- Service boundary decisions

### External Services
- PayOS integration + webhook verification
- Cloudinary upload
- SMTP email
- Goong API

---

## 4. AI Usage Sessions

### Session 1 – EF Core Configuration

| Field | Detail |
|---|---|
| Date | 2026-05-27 |
| Tool | Claude Code |
| Purpose | EF Core relationships & configuration |
| Related Files | ApplicationDbContext, Configurations, Migrations |
| AI Involvement | Significant |

**Prompts**

**Summary**
- Used join entity (StoreFollow) with composite key
- Applied HasConversion<string>() for enum mapping

---

### Session 2 – Repository & UnitOfWork

| Field | Detail |
|---|---|
| Date | 2026-05-27 |
| Tool | Claude Code |
| Purpose | Repository pattern & transaction |
| Related Files | Repositories/*.cs |
| AI Involvement | Significant |

**Prompts**

**Summary**
- Repository supports Include() for eager loading
- UnitOfWork uses single DbContext and SaveChangesAsync

---

### Session 3 – Logging Architecture

| Field | Detail |
|---|---|
| Date | 2026-05-27 |
| Tool | Claude Code |
| Purpose | Logging abstraction & middleware |
| Related Files | Logging/*.cs, Middleware/*.cs |
| AI Involvement | Significant |

**Prompts**

**Summary**
- IAppLogger<T> abstraction
- Middleware logs request/response with elapsed time
- Log level based on HTTP status (5xx → error)

---

### Session 4 – Domain Modeling

| Field | Detail |
|---|---|
| Date | 2026-05-27 |
| Tool | Claude Code |
| Purpose | Domain design |
| Related Files | Domain/Entities, Enums |
| AI Involvement | Significant |

**Prompts**

**Summary**
- Defined aggregate boundaries
- Entity<TKey> with minimal properties (Id only)

---

### Session 5 – Application & External Services

| Field | Detail |
|---|---|
| Date | 2026-05-27 |
| Tool | Claude Code / Copilot |
| Purpose | Abstractions & integrations |
| Related Files | Abstractions, External |
| AI Involvement | Moderate |

**Prompts**


**Summary**
- IRepository standard methods + Query()
- Strategy pattern for payment
- PayOS HMAC verification
- Cloudinary UploadAsync → SecureUrl

---

## 5. AI Assistance Summary

| Area | No AI | Some AI | Heavy AI | Notes |
|---|:---:|:---:|:---:|---|
| Repository |  |  | X | Core from AI |
| UnitOfWork |  | X |  | Pattern refined |
| Domain modeling |  | X |  | AI + team |
| EF configuration |  | X |  | Syntax help |
| Logging |  | X |  | Abstraction design |
| Middleware |  |  | X | AI implementation |
| External APIs |  |  | X | PayOS, Cloudinary |
| Service design | X |  |  | Team decision |

---

## 6. AI Errors / Limitations

| # | Issue | Detection | Resolution |
|---|---|---|---|
| 1 | Outdated .NET syntax | Build error | Updated to .NET 8 |
| 2 | Incorrect cascade rules | Runtime error | Fixed DeleteBehavior |
| 3 | Over-engineering | Code review | Simplified |

---

## 7. Verification Methods

- End-to-end testing
- Compared with official docs
- Code review before merge
- Tested with real data

---

## 8. Team Contribution

| Member | Student ID | Role | AI Used |
|---|---|---|---|
| Ho Huy Hoang | DE180416 | Leader, External | Yes |
| Dam Nguyen Khang | DE180417 | Domain | Yes |
| Nguyen Sinh Nhat | DE180430 | Abstractions | Yes |
| Phan Tran Cong Vu | DE180494 | Infrastructure | Yes |
| Dang Cong Quoc Khanh | DE180880 | Repository | Yes |

---

## 9. Academic Integrity Commitment

- All AI usage recorded  
- Code reviewed and understood  
- Team responsible for final system  

| Representative | Date |
|---|---|
| Ho Huy Hoang | 2026-05-27 |

---

## 10. Current Session Update

| Item | Detail |
|---|---|
| Date | 2026-05-28 |
| Scope | FE-SHARED role shells for Customer, Store Owner, Admin, and Staff |
| Backend Check | Scanned `GearZone.Web` controllers and identity seeding for role coverage |
| Outcome | Shared shell frontend implemented in `GearZone-FE`; backend support confirmed for Customer, Store Owner, and Admin flows, while Staff remains shell-only |

### What was done

- Designed the role shell as a new frontend module for the current project.
- Scanned the current backend for role-seeded identity and role-protected controllers.
- Confirmed existing backend coverage for `Super Admin`, `Store Owner`, and customer-facing flows.
- Did not find a dedicated backend controller group for `Staff`.
- Implemented a shared shell layout with route aliases for `/admin/dashboard` and `/seller/dashboard`.
- Updated login redirect behavior to send users to the appropriate role shell.
- Built the frontend successfully after the changes.

### Verification

- `npm run build` in `GearZone-FE` completed successfully.
- Backend audit was performed by source inspection, not by running live API calls.

---

## 11. Cart & Checkout UI Clone (DE180430)

| Item | Detail |
|---|---|
| Date | 2026-05-30 |
| Scope | Clone buyer experience UI from source project (GearZone `feature/react-tailwind-ui`) |
| Pages | CartPage, CheckoutPage, PayOSCheckoutPage, OrderSuccessPage |
| Branch | `feature/de180430-cart-checkout` |
| Backend Check | Confirmed backend has CartController, CheckoutController, OrdersController with matching endpoints |
| Outcome | 4 pages cloned with ~95% UI fidelity, 9 small commits following project conventions |

### What was done

- Cloned `ProductCard` shared component from source project
- Created `cartApi` and `checkoutApi` modules compatible with existing `apiClient`
- Implemented CartPage with item management (qty controls, remove, order summary)
- Implemented CheckoutPage with address selection, payment method (COD/PayOS), voucher
- Implemented PayOSCheckoutPage with QR code display and payment link
- Implemented OrderSuccessPage with order confirmation and item list
- Registered all routes in App.tsx with RequireAuth protection
- No backend code modified — only UI cloned from source

### Verification

- Pages use existing Tailwind CSS v4 classes and Material Symbols icons
- API modules use existing `apiClient` with `unwrap` pattern
- Routes protected by `RequireAuth` wrapper matching existing auth patterns
- All commits follow `[DE180430] type: description` convention
