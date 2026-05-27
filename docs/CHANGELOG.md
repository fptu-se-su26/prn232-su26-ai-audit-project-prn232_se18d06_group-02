# Changelog 

All notable changes on branch `core/application-abstractions` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
GearZone.Application: all interface contracts -- IRepository<T,TKey>, IUnitOfWork, IService*, IExternal*, IAppLogger<T>

### Added
- IRepository<T,TKey> generic repository interface with CRUD + Query methods
- IUnitOfWork for transaction boundary management
- 28 domain-specific repository interfaces (IProductRepository, IOrderRepository, ICartRepository, etc.)
- External service interfaces: IEmailService, IFileStorageService, IPaymentGateway, IPaymentStrategy, IPayoutClient, IOrderTrackingNotifier
- 35+ service interfaces: IAuthService, ICatalogService, ICartService, ICheckoutService, IOrderService, IPaymentService, IPayoutService, IAdminDashboardService, ISellerProductService, etc.
- IAppLogger<T> cross-cutting logger abstraction
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
