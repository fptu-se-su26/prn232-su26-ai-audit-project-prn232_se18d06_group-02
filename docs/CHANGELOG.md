# Changelog 

All notable changes on branch `core/logging-infrastructure` are documented here.
All notable changes on branch `core/infrastructure-ef-config` are documented here.
All notable changes on branch `core/application-abstractions` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
GearZone.Infrastructure/Repositories: generic Repository<T,TKey>, UnitOfWork, all domain-specific repository implementations

### Added
- Generic Repository<T,TKey> with EF Core DbSet<T>
- ApplyIncludes helper for eager loading via Expression<Func<T,object>>[]
- UnitOfWork wrapping SaveChangesAsync for transaction management
- 35 domain-specific repositories with custom query methods (e.g. ProductRepository, OrderRepository, CartRepository)
Cross-cutting logging: IAppLogger<T> abstraction, SerilogAppLogger<T>, RequestLoggingMiddleware, AuditTrailLogger

### Added
- IAppLogger<T> interface in Application layer (LogInformation, LogWarning, LogError, LogDebug, LogCritical)
- SerilogAppLogger<T> bridging IAppLogger<T> to ASP.NET Core ILogger<T>/Serilog sinks
- RequestLoggingMiddleware logging HTTP method, path, status code, elapsed ms; auto-promotes 4xx/5xx to Warning/Error
- IAuditTrailLogger + AuditTrailLogger for structured CREATE/UPDATE/DELETE audit entries
- UseRequestLogging() extension method for clean middleware registration
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

---

## [Unreleased] -- 2026-05-28

### Scope
GearZone-FE: FE-SHARED role shells for Customer, Store Owner, Admin, and Staff

### Added
- Shared role-shell configuration in `src/lib/roleShell.ts`
- Reusable dashboard shell component in `src/components/shell/RoleShell.tsx`
- Landing page for role selection in `src/pages/HomePage.tsx`
- Role-specific shell pages for Customer, Store Owner, Admin, and Staff
- Route guards and aliases for `/admin/dashboard` and `/seller/dashboard`
- Role-aware login redirect based on authenticated user role

### Changed
- Updated auth context login to return the backend login payload so the UI can route by role
- Replaced the placeholder home screen with a role-aware landing page

### Verification
- `npm run build` completed successfully in `GearZone-FE`

### Backend audit result
- Customer flows: supported by existing public/customer-facing controllers
- Store Owner flows: supported by existing Seller controllers
- Admin flows: supported by existing Admin controllers
- Staff flows: no dedicated Staff controller group found; frontend shell is ready, backend contract still pending

---

## [feature/de180430-order-tracking-review] -- 2026-05-30

### Scope
Clone order tracking + review UI from source project to GearZone-FE React SPA

### Added
- `ordersApi` module: payment status, track, trackLive operations
- `reviewsApi` module: getEditor, submit operations
- `OrderTrackPage`: order summary card, status badge, vertical tracking timeline
- `WriteReviewPage`: star rating widget (1-5 with hover), comment textarea, character counter

### Changed
- `App.tsx`: added routes for `/orders/track/:subOrderId` and `/write-review/:orderItemId`

### Notes
- No backend code modified
- 6 small commits following `[DE180430] type: description` convention
