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
# AI Prompts Log 

Branch: `core/infrastructure-ef-config`
Scope: GearZone.Infrastructure: ApplicationDbContext, IEntityTypeConfiguration<T> per entity, EF migrations, seed data

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

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
## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Infrastructure: ApplicationDbContext, IEntityTypeConfiguration<T> per entity, EF migrations, seed data

**Prompt:**
> How to configure a many-to-many self-referencing relationship in EF Core 8 for store follows?

**AI Output Summary:**
HasMany/WithMany with explicit join entity (StoreFollow) and composite primary key configuration.

**Used in files:** GearZone.Infrastructure/ApplicationDbContext.cs, Configurations/*.cs, Migrations/*.cs, Seed/*.cs
# AI Prompts Log 
Branch: `core/domain-entities`
Scope: GearZone.Domain: Entity<TKey> base class, all aggregate roots and value objects, domain enums

Branch: `core/application-abstractions`
Scope: GearZone.Application: all interface contracts -- IRepository<T,TKey>, IUnitOfWork, IService*, IExternal*, IAppLogger<T>

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

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
## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Domain: Entity<TKey> base class, all aggregate roots and value objects, domain enums

**Prompt:**
> Design entity relationships for a multi-vendor e-commerce platform where each Order splits into SubOrders per seller.

**AI Output Summary:**
Suggested Order -> SubOrder -> OrderItem hierarchy; PaymentMethod linked to Order; Payment as a separate aggregate.

**Used in files:** GearZone.Domain/Entities/*.cs, GearZone.Domain/Enums/*.cs
**Context:** GearZone.Application: all interface contracts -- IRepository<T,TKey>, IUnitOfWork, IService*, IExternal*, IAppLogger<T>

**Prompt:**
> What methods should IRepository<T,TKey> expose in Clean Architecture -- should Query() return IQueryable?

**AI Output Summary:**
Recommended GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync, Query() for IQueryable access; warned about leaking IQueryable across layers.

**Used in files:** GearZone.Application/Abstractions/**/*.cs
**Context:** GearZone.Infrastructure/External: Cloudinary file storage, PayOS payment/payout, SMTP email, Goong map API

**Prompt:**
> How to integrate PayOS payment gateway in ASP.NET Core with webhook signature verification?

**AI Output Summary:**
PayOS SDK: create payment link, handle webhook callback, verify HMAC-SHA256 signature, update order status.

**Used in files:** GearZone.Infrastructure/External/*.cs, Settings/*.cs

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Infrastructure: ApplicationDbContext, IEntityTypeConfiguration<T> per entity, EF migrations, seed data

**Prompt:**
> What is the correct EF Core configuration for storing C# enums as strings in SQL Server?

**AI Output Summary:**
HasConversion<string>() in IEntityTypeConfiguration with HasColumnType('nvarchar(50)').

**Used in files:** GearZone.Infrastructure/ApplicationDbContext.cs, Configurations/*.cs, Migrations/*.cs, Seed/*.cs
**Context:** GearZone.Domain: Entity<TKey> base class, all aggregate roots and value objects, domain enums

**Prompt:**
> What is the cleanest base Entity class for Clean Architecture with EF Core -- should Id be in the domain or in EF config?

**AI Output Summary:**
Recommended Entity<TKey> with only Id; auditing fields (CreatedAt etc.) should live in EF configuration, not the base class.

**Used in files:** GearZone.Domain/Entities/*.cs, GearZone.Domain/Enums/*.cs
**Context:** GearZone.Application: all interface contracts -- IRepository<T,TKey>, IUnitOfWork, IService*, IExternal*, IAppLogger<T>

**Prompt:**
> Design interface contracts for an e-commerce payment system that supports multiple strategies (PayOS, COD, wallet).

**AI Output Summary:**
Suggested IPaymentGateway (process/verify) + IPaymentStrategy (calculate) + IPayoutClient (disburse) -- strategy pattern for extensibility.

**Used in files:** GearZone.Application/Abstractions/**/*.cs
**Context:** GearZone.Infrastructure/External: Cloudinary file storage, PayOS payment/payout, SMTP email, Goong map API

**Prompt:**
> Implement Cloudinary image upload in C# with automatic public_id generation and error handling.

**AI Output Summary:**
Cloudinary .NET SDK: Cloudinary.UploadAsync with RawUploadParams; return SecureUrl as stored image URL.

**Used in files:** GearZone.Infrastructure/External/*.cs, Settings/*.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Infrastructure: ApplicationDbContext, IEntityTypeConfiguration<T> per entity, EF migrations, seed data

**Prompt:**
> How do I seed data through EF migrations safely without duplicating rows on re-run?

**AI Output Summary:**
Check row existence before insert using MigrationBuilder.Sql with IF NOT EXISTS pattern.

**Used in files:** GearZone.Infrastructure/ApplicationDbContext.cs, Configurations/*.cs, Migrations/*.cs, Seed/*.cs
**Context:** GearZone.Domain: Entity<TKey> base class, all aggregate roots and value objects, domain enums

**Prompt:**
> Model a Payout system where a PayoutBatch contains multiple PayoutItems linked to SubOrders.

**AI Output Summary:**
Designed PayoutBatch -> PayoutItem -> SubOrder; PayoutTransaction as the actual financial record per disbursement.

**Used in files:** GearZone.Domain/Entities/*.cs, GearZone.Domain/Enums/*.cs
**Context:** GearZone.Application: all interface contracts -- IRepository<T,TKey>, IUnitOfWork, IService*, IExternal*, IAppLogger<T>

**Prompt:**
> When should I split a service interface -- one IOrderService or separate IAdminOrderService and ISellerOrderService?

**AI Output Summary:**
Advised splitting by actor role; Admin and Seller have different authorization contexts and different DTOs.

**Used in files:** GearZone.Application/Abstractions/**/*.cs
**Context:** GearZone.Infrastructure/External: Cloudinary file storage, PayOS payment/payout, SMTP email, Goong map API

**Prompt:**
> How do I calculate shipping distance using the Goong Directions API in .NET?

**AI Output Summary:**
Goong REST API: GET /direction with origin/destination lat-lng; parse routes[0].legs[0].distance.value in meters.

**Used in files:** GearZone.Infrastructure/External/*.cs, Settings/*.cs

---

## Prompt 4 -- 2026-05-28

**Tool:** Claude Code
**Context:** GearZone-FE role-shell implementation in the current project

**Prompt:**
> Build a shared role-shell frontend for the current GearZone-FE project with separate experiences for Customer, Store Owner, Admin, and Staff. Keep the implementation aligned with the existing backend role model, verify which role-specific backend endpoints are already available, and document the work in `docs/PROMPTS.md`, `docs/AI_AUDIT_LOG.md`, `docs/CHANGELOG.md`, and `docs/REFLECTION.md`. If any backend capability is missing, call it out clearly.

**AI Output Summary:**
Created a shared role-shell frontend architecture, added role-aware routing and login redirects, and checked backend readiness by scanning API controllers and role seeding.

**Used in files:** `GearZone-FE/src/lib/roleShell.ts`, `GearZone-FE/src/components/shell/RoleShell.tsx`, `GearZone-FE/src/pages/*.tsx`, `GearZone-FE/src/App.tsx`, `GearZone-FE/src/contexts/*`
