# Changelog -- Feature -- React Frontend

All notable changes on branch `feature/frontend-react` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
gearzone-react: Vite + React + TypeScript + Tailwind CSS SPA for the buyer-facing storefront

### Added
- Vite + React + TypeScript + Tailwind CSS v4 project setup
- React Router v6 with nested routes for catalog, product detail, cart, checkout, auth
- Axios HTTP client with JWT interceptor for automatic token attachment and refresh
- Product listing page with filter sidebar and responsive grid
- Product detail page with variant selector, image gallery, and add-to-cart
- Cart page with quantity update and store-grouped display
- Checkout wizard: address -> voucher -> payment -> confirmation
- Authentication pages: login, register, email verification

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
