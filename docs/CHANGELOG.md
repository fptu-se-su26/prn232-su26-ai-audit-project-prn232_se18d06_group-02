# Changelog -- Feature -- Voucher and Discount System

All notable changes on branch `feature/voucher-system` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Seller-created vouchers: percentage/fixed discount, usage limits, expiry, checkout application, admin moderation

### Added
- Voucher entity with VoucherType (Percentage, FixedAmount), MaxUsage, MinOrderAmount, StartDate, EndDate, VoucherStatus
- VoucherUsage join entity tracking which users have used which vouchers
- Seller voucher CRUD: create, edit, activate/deactivate, view usage statistics
- Checkout voucher application: validate then apply discount to order total
- Admin voucher overview for moderation
- ISellerVoucherService, IAdminVoucherService contracts and implementations

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
