# Changelog -- Feature -- Store Management

All notable changes on branch `feature/store-management` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Seller store registration, admin approval, store profile, store settings, store follow/unfollow

### Added
- Store registration application flow: business info + identity card upload -> admin review -> approve/reject
- Store profile public page with follower count and product grid
- StoreFollow entity for buyer-to-store following with unfollow support
- Store settings: name, description, avatar, bank account, geo-coordinates
- Admin store management: list all stores, view details, suspend/reactivate
- ISellerStoreService, IAdminStoreService contracts and implementations
- Cloudinary integration for identity card images and store avatar

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
