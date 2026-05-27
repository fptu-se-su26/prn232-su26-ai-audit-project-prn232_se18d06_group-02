# Changelog -- Feature -- Product Catalog Browsing

All notable changes on branch `feature/catalog-browsing` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Public product listing, search, filtering by category/brand/price/rating, product detail page, store profile browsing

### Added
- Paginated product listing with offset pagination
- Multi-filter search: category, brand, price range, rating, attribute values
- Product detail page with variant selector and image gallery
- Store profile public view with product grid and follower count
- CatalogController API endpoints for AJAX-driven filtering
- ProductSpecification classes for composable EF Core queries
- CatalogService with ICatalogService contract

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
