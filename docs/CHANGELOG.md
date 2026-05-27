# Changelog -- Core -- EF Core Configuration and Migrations

All notable changes on branch `core/infrastructure-ef-config` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
GearZone.Infrastructure: ApplicationDbContext, IEntityTypeConfiguration<T> per entity, EF migrations, seed data

### Added
- ApplicationDbContext extending IdentityDbContext<ApplicationUser>
- 30+ IEntityTypeConfiguration implementations with column types, FK constraints, indexes, and cascade rules
- 20+ EF migrations from InitialCreate through AddShipmentsAndStoreCoordinates
- Seed data: IdentitySeeder (admin/demo accounts), CategorySeeder (gear categories), SystemSettingSeeder
- DependencyInjection.cs wiring all repositories and services

### Changed
- Adapted existing code patterns to align with Clean Architecture conventions

### Fixed
- N/A (initial implementation on this branch)

### Notes
- All changes target `develop` as the merge destination
- No direct commits to `main`

---

## Previous Releases
See `main` branch CHANGELOG for project-level release history.
