# AI Prompts Log -- Core -- EF Core Configuration and Migrations

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

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Infrastructure: ApplicationDbContext, IEntityTypeConfiguration<T> per entity, EF migrations, seed data

**Prompt:**
> What is the correct EF Core configuration for storing C# enums as strings in SQL Server?

**AI Output Summary:**
HasConversion<string>() in IEntityTypeConfiguration with HasColumnType('nvarchar(50)').

**Used in files:** GearZone.Infrastructure/ApplicationDbContext.cs, Configurations/*.cs, Migrations/*.cs, Seed/*.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Infrastructure: ApplicationDbContext, IEntityTypeConfiguration<T> per entity, EF migrations, seed data

**Prompt:**
> How do I seed data through EF migrations safely without duplicating rows on re-run?

**AI Output Summary:**
Check row existence before insert using MigrationBuilder.Sql with IF NOT EXISTS pattern.

**Used in files:** GearZone.Infrastructure/ApplicationDbContext.cs, Configurations/*.cs, Migrations/*.cs, Seed/*.cs

---
