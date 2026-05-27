# Changelog -- Feature -- Payment Processing

All notable changes on branch `feature/payment-processing` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
PayOS online payment, COD, wallet top-up, platform transaction tracking, payment webhook handling

### Added
- PayOS payment link creation and webhook callback handling
- HMAC-SHA256 signature verification for PayOS webhook security
- COD payment strategy (no upfront payment, confirmed on delivery)
- Wallet top-up flow via PayOS with PlatformTransaction record
- Payment status polling endpoint for frontend fallback
- WalletTransaction recording debits/credits with TransactionType
- IPaymentService, IPaymentGateway, IPaymentStrategy contracts and implementations

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
