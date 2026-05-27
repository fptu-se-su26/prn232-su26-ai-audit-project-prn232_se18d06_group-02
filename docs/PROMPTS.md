# AI Prompts Log
Branch: `core/infrastructure-repositories`
Scope: GearZone.Infrastructure/Repositories: generic Repository<T,TKey>, UnitOfWork, all domain-specific repository implementations

Branch: `core/logging-infrastructure`
Scope: Cross-cutting logging: IAppLogger<T> abstraction, SerilogAppLogger<T>, RequestLoggingMiddleware, AuditTrailLogger

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
**Context:** Cross-cutting logging: IAppLogger<T> abstraction, SerilogAppLogger<T>, RequestLoggingMiddleware, AuditTrailLogger

**Prompt:**
> Design a logger abstraction interface in Clean Architecture that keeps the Application layer free of Serilog dependencies.

**AI Output Summary:**
IAppLogger<T> with five methods mirroring ILogger<T> levels; inject in Application services instead of ILogger<T>.

**Used in files:** GearZone.Application/Abstractions/Logging/IAppLogger.cs, GearZone.Infrastructure/Logging/SerilogAppLogger.cs, GearZone.Infrastructure/Logging/AuditTrailLogger.cs, GearZone.Web/Middleware/RequestLoggingMiddleware.cs

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Infrastructure/Repositories: generic Repository<T,TKey>, UnitOfWork, all domain-specific repository implementations

**Prompt:**
> How should UnitOfWork be implemented to coordinate multiple repositories in a single DbContext?

**AI Output Summary:**
UnitOfWork holds a single DbContext instance; SaveChangesAsync commits all tracked changes atomically.

**Used in files:** GearZone.Infrastructure/Repositories/*.cs
**Context:** Cross-cutting logging: IAppLogger<T> abstraction, SerilogAppLogger<T>, RequestLoggingMiddleware, AuditTrailLogger

**Prompt:**
> Implement an ASP.NET Core middleware that logs every HTTP request with method, path, status code, and elapsed time; promote 5xx to error level.

**AI Output Summary:**
Stopwatch-based RequestLoggingMiddleware; LogLevel selected based on status code range; exception rethrown after logging.

**Used in files:** GearZone.Application/Abstractions/Logging/IAppLogger.cs, GearZone.Infrastructure/Logging/SerilogAppLogger.cs, GearZone.Infrastructure/Logging/AuditTrailLogger.cs, GearZone.Web/Middleware/RequestLoggingMiddleware.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Infrastructure/Repositories: generic Repository<T,TKey>, UnitOfWork, all domain-specific repository implementations

**Prompt:**
> When should I override the generic repository methods vs adding domain-specific query methods?

**AI Output Summary:**
Override only when default behavior is insufficient (e.g. soft delete, composite key lookups).

**Used in files:** GearZone.Infrastructure/Repositories/*.cs
**Context:** Cross-cutting logging: IAppLogger<T> abstraction, SerilogAppLogger<T>, RequestLoggingMiddleware, AuditTrailLogger

**Prompt:**
> How do I implement an audit trail logger that records entity mutations (create/update/delete) with user context in .NET?

**AI Output Summary:**
AuditTrailLogger emitting structured [AUDIT] log entries with action, entity type, entity ID, userId, timestamp.

**Used in files:** GearZone.Application/Abstractions/Logging/IAppLogger.cs, GearZone.Infrastructure/Logging/SerilogAppLogger.cs, GearZone.Infrastructure/Logging/AuditTrailLogger.cs, GearZone.Web/Middleware/RequestLoggingMiddleware.cs

---
