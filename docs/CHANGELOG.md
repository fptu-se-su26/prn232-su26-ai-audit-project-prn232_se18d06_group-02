# Changelog -- Feature -- Seller Payout System

All notable changes on branch `feature/payout-system` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Seller revenue disbursement: payout batch creation, payout items per sub-order, PayOS transfer, admin approval workflow

### Added
- PayoutBatch aggregating PayoutItems for admin review before disbursement
- PayoutItem linking each completed SubOrder to a seller's pending earnings
- Admin payout approval: review batch -> approve -> trigger PayOS disbursement
- PayoutBatchJob: Hangfire job runs weekly to aggregate completed orders into new batch
- PayoutTransaction recording the actual bank transfer with reference code
- Platform fee deduction calculation before payout amount
- IPayoutService, IAdminPayoutService contracts and implementations

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
