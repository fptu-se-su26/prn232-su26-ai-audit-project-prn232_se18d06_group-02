# AI Prompts Log -- Core -- Repository Implementations

Branch: `core/infrastructure-repositories`
Scope: GearZone.Infrastructure/Repositories: generic Repository<T,TKey>, UnitOfWork, all domain-specific repository implementations

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Infrastructure/Repositories: generic Repository<T,TKey>, UnitOfWork, all domain-specific repository implementations

**Prompt:**
> Implement a generic repository pattern with EF Core that supports eager loading via Expression<Func<T,object>>.

**AI Output Summary:**
Repository<T,TKey> with IQueryable.Include() loop and ApplyIncludes helper; returns IQueryable from Query().

**Used in files:** GearZone.Infrastructure/Repositories/*.cs

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Infrastructure/Repositories: generic Repository<T,TKey>, UnitOfWork, all domain-specific repository implementations

**Prompt:**
> How should UnitOfWork be implemented to coordinate multiple repositories in a single DbContext?

**AI Output Summary:**
UnitOfWork holds a single DbContext instance; SaveChangesAsync commits all tracked changes atomically.

**Used in files:** GearZone.Infrastructure/Repositories/*.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Infrastructure/Repositories: generic Repository<T,TKey>, UnitOfWork, all domain-specific repository implementations

**Prompt:**
> When should I override the generic repository methods vs adding domain-specific query methods?

**AI Output Summary:**
Override only when default behavior is insufficient (e.g. soft delete, composite key lookups).

**Used in files:** GearZone.Infrastructure/Repositories/*.cs

---
