# AI Prompts Log -- Core -- Domain Entities

Branch: `core/domain-entities`
Scope: GearZone.Domain: Entity<TKey> base class, all aggregate roots and value objects, domain enums

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Domain: Entity<TKey> base class, all aggregate roots and value objects, domain enums

**Prompt:**
> Design entity relationships for a multi-vendor e-commerce platform where each Order splits into SubOrders per seller.

**AI Output Summary:**
Suggested Order -> SubOrder -> OrderItem hierarchy; PaymentMethod linked to Order; Payment as a separate aggregate.

**Used in files:** GearZone.Domain/Entities/*.cs, GearZone.Domain/Enums/*.cs

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Domain: Entity<TKey> base class, all aggregate roots and value objects, domain enums

**Prompt:**
> What is the cleanest base Entity class for Clean Architecture with EF Core -- should Id be in the domain or in EF config?

**AI Output Summary:**
Recommended Entity<TKey> with only Id; auditing fields (CreatedAt etc.) should live in EF configuration, not the base class.

**Used in files:** GearZone.Domain/Entities/*.cs, GearZone.Domain/Enums/*.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Domain: Entity<TKey> base class, all aggregate roots and value objects, domain enums

**Prompt:**
> Model a Payout system where a PayoutBatch contains multiple PayoutItems linked to SubOrders.

**AI Output Summary:**
Designed PayoutBatch -> PayoutItem -> SubOrder; PayoutTransaction as the actual financial record per disbursement.

**Used in files:** GearZone.Domain/Entities/*.cs, GearZone.Domain/Enums/*.cs

---
