# Changelog -- Core -- Repository Implementations

All notable changes on branch `core/infrastructure-repositories` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
GearZone.Infrastructure/Repositories: generic Repository<T,TKey>, UnitOfWork, all domain-specific repository implementations

### Added
- Generic Repository<T,TKey> with EF Core DbSet<T>
- ApplyIncludes helper for eager loading via Expression<Func<T,object>>[]
- UnitOfWork wrapping SaveChangesAsync for transaction management
- 35 domain-specific repositories with custom query methods (e.g. ProductRepository, OrderRepository, CartRepository)

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
