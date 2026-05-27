# Changelog -- Core -- External Service Integrations

All notable changes on branch `core/infrastructure-external-services` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
GearZone.Infrastructure/External: Cloudinary file storage, PayOS payment/payout, SMTP email, Goong map API

### Added
- CloudinaryStorageService implementing IFileStorageService (image upload/delete)
- SmtpEmailService implementing IEmailService with HTML template support
- PayOSPaymentGateway + PayOSPaymentStrategy implementing IPaymentGateway/IPaymentStrategy
- PayOSPayoutClient implementing IPayoutClient for seller disbursements
- CodPaymentStrategy for Cash-on-Delivery flow (no upfront payment)
- GoongService implementing IGoongService for distance/routing calculation
- Disabled stub implementations (DisabledPaymentGateway, DisabledPayoutClient) for dev environment
- PayOSSettings, PayOSPayoutSettings configuration classes

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
