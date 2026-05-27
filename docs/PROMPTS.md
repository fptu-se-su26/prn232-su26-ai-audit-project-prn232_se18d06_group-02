# AI Prompts Log -- Core -- Application Abstractions

Branch: `core/application-abstractions`
Scope: GearZone.Application: all interface contracts -- IRepository<T,TKey>, IUnitOfWork, IService*, IExternal*, IAppLogger<T>

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Application: all interface contracts -- IRepository<T,TKey>, IUnitOfWork, IService*, IExternal*, IAppLogger<T>

**Prompt:**
> What methods should IRepository<T,TKey> expose in Clean Architecture -- should Query() return IQueryable?

**AI Output Summary:**
Recommended GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync, Query() for IQueryable access; warned about leaking IQueryable across layers.

**Used in files:** GearZone.Application/Abstractions/**/*.cs

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Application: all interface contracts -- IRepository<T,TKey>, IUnitOfWork, IService*, IExternal*, IAppLogger<T>

**Prompt:**
> Design interface contracts for an e-commerce payment system that supports multiple strategies (PayOS, COD, wallet).

**AI Output Summary:**
Suggested IPaymentGateway (process/verify) + IPaymentStrategy (calculate) + IPayoutClient (disburse) -- strategy pattern for extensibility.

**Used in files:** GearZone.Application/Abstractions/**/*.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Application: all interface contracts -- IRepository<T,TKey>, IUnitOfWork, IService*, IExternal*, IAppLogger<T>

**Prompt:**
> When should I split a service interface -- one IOrderService or separate IAdminOrderService and ISellerOrderService?

**AI Output Summary:**
Advised splitting by actor role; Admin and Seller have different authorization contexts and different DTOs.

**Used in files:** GearZone.Application/Abstractions/**/*.cs

---
