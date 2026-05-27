# Changelog -- Feature -- User Profile and Account

All notable changes on branch `feature/user-profile` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Buyer profile: view/edit personal info, avatar upload, saved delivery addresses, wallet balance view, order history

### Added
- User profile view and edit: display name, avatar (Cloudinary), phone number
- UserAddress CRUD: add/edit/delete delivery addresses with default address selection
- Wallet balance display and transaction history
- Order history list with status filters for buyers
- IUserService (buyer profile operations) contract and implementation
- Avatar upload via Cloudinary with size/type validation

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
