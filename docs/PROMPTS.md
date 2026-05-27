# AI Prompts Log 

Branch: `core/infrastructure-ef-config`
Scope: GearZone.Infrastructure: ApplicationDbContext, IEntityTypeConfiguration<T> per entity, EF migrations, seed data

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

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
