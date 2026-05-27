# Changelog -- Feature -- Seller Product Management

All notable changes on branch `feature/product-management` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Seller CRUD for products: create with variants/images/attributes, update, soft-delete, status management, inventory tracking

### Added
- Product creation wizard: basic info, variants (size/color matrix), image upload (Cloudinary), attribute values
- Soft-delete product with ProductStatus enum (Active, Inactive, Banned, PendingReview)
- Product variant management: add/edit/delete variants with individual pricing and stock
- InventoryTransaction recording stock in/out events
- ISellerProductService and implementation
- Seller product listing with status filters
- Admin product moderation: approve/ban with reason

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
