# Changelog -- Feature -- Shopping Cart and Checkout

All notable changes on branch `feature/cart-and-checkout` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Persistent global cart, multi-seller checkout, voucher application, address selection, order placement

### Added
- Global persistent cart linked to user account (not session)
- Cart grouped by store for multi-seller checkout display
- CartItem add/update/remove with real-time stock validation
- Checkout flow: address selection -> voucher -> payment method -> review -> place order
- Voucher application with validation (minimum order value, usage limits, expiry)
- CheckoutController creating Order + SubOrders atomically via UnitOfWork
- ICartService and ICheckoutService contracts and implementations

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
