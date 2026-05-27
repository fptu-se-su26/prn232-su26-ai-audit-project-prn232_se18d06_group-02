# Changelog 

All notable changes on branch `core/infrastructure-ef-config` are documented here.

All notable changes on branch `core/application-abstractions` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
GearZone.Infrastructure: ApplicationDbContext, IEntityTypeConfiguration<T> per entity, EF migrations, seed data

### Added
- ApplicationDbContext extending IdentityDbContext<ApplicationUser>
- 30+ IEntityTypeConfiguration implementations with column types, FK constraints, indexes, and cascade rules
- 20+ EF migrations from InitialCreate through AddShipmentsAndStoreCoordinates
- Seed data: IdentitySeeder (admin/demo accounts), CategorySeeder (gear categories), SystemSettingSeeder
- DependencyInjection.cs wiring all repositories and services
GearZone.Domain: Entity<TKey> base class, all aggregate roots and value objects, domain enums

### Added
- Entity<TKey> generic base class with Id property
- 25+ domain entities: ApplicationUser, Product, ProductVariant, ProductImage, Order, SubOrder, OrderItem, OrderStatusHistory, Cart, CartItem, Store, StoreFollow, Brand, Category, CategoryAttribute, Conversation, ChatMessage, Payment, WalletTransaction, Payout, PayoutBatch, PayoutItem, PayoutTransaction, PlatformTransaction, Voucher, VoucherUsage, ProductReview, Shipment, UserAddress, SystemSetting, InventoryTransaction
- Domain enums: OrderStatus, PaymentStatus, PaymentMethod, ProductStatus, StoreStatus, VoucherType, TransactionType, InventoryTransactionType
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
