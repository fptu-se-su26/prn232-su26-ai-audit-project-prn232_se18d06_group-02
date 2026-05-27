# Changelog -- Feature -- Order Management

All notable changes on branch `feature/order-management` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Order lifecycle: status transitions, history tracking, buyer and seller order views, auto-complete background job

### Added
- Order status machine: Pending -> Confirmed -> Shipped -> Delivered -> Completed / Cancelled / Disputed
- OrderStatusHistory records every status transition with actor (user/system) and timestamp
- Buyer order list and detail views with tracking information
- Seller sub-order management: confirm, mark shipped, handle disputes
- Auto-complete job: Hangfire background job completes orders 7 days after Delivered status
- IOrderService, IAdminOrderService contracts and implementations
- SignalR notification on order status change via IOrderTrackingNotifier

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
