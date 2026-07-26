# Changelog -- Develop -- Integration Base

All notable changes on branch `develop` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Full project bootstrapping: solution structure, .gitignore, dependency wiring

### Added
- Clean Architecture solution with four projects (Domain, Application, Infrastructure, Web)
- Root .gitignore excluding bin/, obj/, node_modules/, .vs/
- GearZone.sln solution file with correct project references
- Initial codebase committed to version control
- Customer-facing campaign sale badges, original-price comparison, savings,
  and campaign details on search results and product detail pages
- Seller-owned promotion campaigns with product selection, derived lifecycle
  status, quota progress, overlap protection, and pause/resume controls
- Atomic campaign and voucher reservation lifecycle for checkout, PayOS, COD,
  cancellation, timeout, rejection, and compensation
- Authoritative checkout quote API with campaign pricing, seller/platform order
  and shipping vouchers, server-calculated shipping, and available-voucher
  eligibility
- Order item, sub-order, shipment, and order-level financial snapshots for
  promotion display, commission, payout, and idempotent checkout
- Seller promotion Razor pages and promotion-aware catalog, product, cart,
  checkout, and order-detail presentation
- EF Core migration
  `20260726174949_AddPromotionCampaignsAndCheckoutPricing`
- Promotion/voucher pricing, lifecycle, quota, category-scope, persistence
  constraint, and commission tests

### Changed
- Adapted existing code patterns to align with Clean Architecture conventions
- Seller vouchers now support both order and shipping discount types
- Catalog filtering and sorting use effective campaign prices
- Seller revenue/reporting uses commissionable/net amounts after seller-funded
  discounts; platform vouchers do not reduce seller payout
- Legacy voucher checkout calls no longer accept client-calculated merchandise
  or shipping totals

### Fixed
- Prevented duplicate checkout orders through `CheckoutRequestId`
- Prevented stock, campaign quota, and voucher usage from being partially
  persisted across checkout/payment state transitions
- Prevented checkout from throwing `DbUpdateConcurrencyException` when a
  purchased cart line was already cleared by a repeated/concurrent request
- Isolated post-payment persistence from stale quote/reservation tracking so
  row-version changes made by conditional updates cannot leak into the payment
  `SaveChanges` batch

### Notes
- All changes target `develop` as the merge destination
- No direct commits to `main`

---

## Previous Releases
See `main` branch CHANGELOG for project-level release history.
