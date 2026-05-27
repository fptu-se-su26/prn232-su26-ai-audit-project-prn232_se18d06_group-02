# Changelog -- Feature -- Shipping and Logistics

All notable changes on branch `feature/shipping-logistics` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Shipping cost calculation via Goong distance API, shipment tracking, estimated delivery, address management

### Added
- ShippingService calculating cost based on distance between store and delivery address via Goong Directions API
- Shipment entity with tracking code, carrier, estimated delivery date
- Shipment status updates by seller: Preparing -> Shipped -> OutForDelivery -> Delivered
- GoongService implementing IGoongService with Directions API and Geocoding API
- UserAddress management (add/edit/delete delivery addresses with lat-long)
- Map DTOs: GoongDirectionResponse, GoongGeocodeResponse
- IShippingService contract and implementation

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
