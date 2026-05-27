# Changelog -- Feature -- Admin Panel

All notable changes on branch `feature/admin-panel` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Full admin dashboard: user management, product moderation, order management, store applications, payout oversight, system settings, transaction ledger

### Added
- Admin dashboard with key metrics (total orders, revenue, active stores, new users)
- User management: list, view profile, lock/unlock accounts
- Product moderation: approve/reject pending products with reason
- Store application review: approve/reject seller applications
- Order management: view all orders, override status in exceptional cases
- Transaction ledger: all wallet and platform transactions
- System settings management (platform fee rate, auto-complete days, etc.)
- Admin payout batch approval workflow
- All IAdmin* service contracts and implementations

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
