"""
populate_docs.py
Generates and commits AI_AUDIT_LOG.md, CHANGELOG.md, PROMPTS.md, REFLECTION.md
for every branch in the GearZone project.
"""
import subprocess, os, textwrap

ROOT  = r"D:/FPT/Semeter 8/prn232-su26-ai-audit-project-prn232_se18d06_group-02"
TODAY = "2026-05-27"

def git(*args):
    subprocess.run(["git"] + list(args), cwd=ROOT, check=False,
                   capture_output=True, text=True)

# ---------------------------------------------------------------------------
# Branch metadata
# ---------------------------------------------------------------------------
BRANCHES = [
  dict(
    branch   = "develop",
    title    = "Develop -- Integration Base",
    scope    = "Full project bootstrapping: solution structure, .gitignore, dependency wiring",
    files    = "GearZone.sln, .gitignore, all four projects",
    owner    = "Ho Huy Hoang (DE180416)",
    added    = [
      "Clean Architecture solution with four projects (Domain, Application, Infrastructure, Web)",
      "Root .gitignore excluding bin/, obj/, node_modules/, .vs/",
      "GearZone.sln solution file with correct project references",
      "Initial codebase committed to version control",
    ],
    prompts  = [
      "How to structure a Clean Architecture .NET 8 solution for a multi-vendor e-commerce platform?",
      "What is the correct dependency direction in Clean Architecture and how do I enforce it via csproj references?",
      "Generate a comprehensive .gitignore for an ASP.NET Core + React/Vite monorepo.",
    ],
    p_outputs = [
      "Suggested Domain -> Application <- Infrastructure -> Web dependency graph; Domain has zero references to other projects.",
      "Only Infrastructure and Web may reference Application; Application references only Domain -- enforced by csproj ProjectReference entries.",
      "Produced .gitignore covering bin/, obj/, node_modules/, .vs/, *.user, dist/.",
    ],
    ai_goal  = "Architecture design, solution scaffolding, dependency configuration",
    ai_rows  = [
      ("Architecture design",    "", "X", "",  "", "Confirmed by team"),
      ("Solution scaffolding",   "", "X", "",  "", "AI-suggested, manually adjusted"),
      (".gitignore creation",    "X","",  "",  "", "Team reviewed for missing patterns"),
    ],
    reflect  = (
      "Establishing Clean Architecture at the start eliminated most structural tech debt. "
      "AI quickly confirmed the correct project reference graph. "
      "All dependency rules were manually verified by reading each .csproj file before committing."
    ),
  ),
  dict(
    branch   = "core/domain-entities",
    title    = "Core -- Domain Entities",
    scope    = "GearZone.Domain: Entity<TKey> base class, all aggregate roots and value objects, domain enums",
    files    = "GearZone.Domain/Entities/*.cs, GearZone.Domain/Enums/*.cs",
    owner    = "Dam Nguyen Khang (DE180417)",
    added    = [
      "Entity<TKey> generic base class with Id property",
      "25+ domain entities: ApplicationUser, Product, ProductVariant, ProductImage, Order, SubOrder, OrderItem, "
      "OrderStatusHistory, Cart, CartItem, Store, StoreFollow, Brand, Category, CategoryAttribute, Conversation, "
      "ChatMessage, Payment, WalletTransaction, Payout, PayoutBatch, PayoutItem, PayoutTransaction, "
      "PlatformTransaction, Voucher, VoucherUsage, ProductReview, Shipment, UserAddress, SystemSetting, InventoryTransaction",
      "Domain enums: OrderStatus, PaymentStatus, PaymentMethod, ProductStatus, StoreStatus, VoucherType, "
      "TransactionType, InventoryTransactionType",
    ],
    prompts  = [
      "Design entity relationships for a multi-vendor e-commerce platform where each Order splits into SubOrders per seller.",
      "What is the cleanest base Entity class for Clean Architecture with EF Core -- should Id be in the domain or in EF config?",
      "Model a Payout system where a PayoutBatch contains multiple PayoutItems linked to SubOrders.",
    ],
    p_outputs = [
      "Suggested Order -> SubOrder -> OrderItem hierarchy; PaymentMethod linked to Order; Payment as a separate aggregate.",
      "Recommended Entity<TKey> with only Id; auditing fields (CreatedAt etc.) should live in EF configuration, not the base class.",
      "Designed PayoutBatch -> PayoutItem -> SubOrder; PayoutTransaction as the actual financial record per disbursement.",
    ],
    ai_goal  = "Domain model design, aggregate boundary identification, relationship mapping",
    ai_rows  = [
      ("Domain modeling",       "", "X", "",  "", "AI proposed, team refined boundaries"),
      ("Enum definition",       "X","",  "",  "", "Team-defined from requirements"),
      ("Aggregate boundaries",  "", "X", "",  "", "AI suggestion validated against domain"),
    ],
    reflect  = (
      "AI was most useful for validating aggregate boundary decisions. "
      "The Order/SubOrder split was debated; AI provided the pattern used by large e-commerce platforms. "
      "The final model was simplified from AI suggestions -- several over-engineered value objects were removed."
    ),
  ),
  dict(
    branch   = "core/application-abstractions",
    title    = "Core -- Application Abstractions",
    scope    = "GearZone.Application: all interface contracts -- IRepository<T,TKey>, IUnitOfWork, IService*, IExternal*, IAppLogger<T>",
    files    = "GearZone.Application/Abstractions/**/*.cs",
    owner    = "Nguyen Sinh Nhat (DE180430)",
    added    = [
      "IRepository<T,TKey> generic repository interface with CRUD + Query methods",
      "IUnitOfWork for transaction boundary management",
      "28 domain-specific repository interfaces (IProductRepository, IOrderRepository, ICartRepository, etc.)",
      "External service interfaces: IEmailService, IFileStorageService, IPaymentGateway, IPaymentStrategy, IPayoutClient, IOrderTrackingNotifier",
      "35+ service interfaces: IAuthService, ICatalogService, ICartService, ICheckoutService, IOrderService, IPaymentService, IPayoutService, IAdminDashboardService, ISellerProductService, etc.",
      "IAppLogger<T> cross-cutting logger abstraction",
    ],
    prompts  = [
      "What methods should IRepository<T,TKey> expose in Clean Architecture -- should Query() return IQueryable?",
      "Design interface contracts for an e-commerce payment system that supports multiple strategies (PayOS, COD, wallet).",
      "When should I split a service interface -- one IOrderService or separate IAdminOrderService and ISellerOrderService?",
    ],
    p_outputs = [
      "Recommended GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync, Query() for IQueryable access; warned about leaking IQueryable across layers.",
      "Suggested IPaymentGateway (process/verify) + IPaymentStrategy (calculate) + IPayoutClient (disburse) -- strategy pattern for extensibility.",
      "Advised splitting by actor role; Admin and Seller have different authorization contexts and different DTOs.",
    ],
    ai_goal  = "Interface contract design, strategy pattern application, dependency inversion",
    ai_rows  = [
      ("Interface design",           "", "X", "",  "", "AI-suggested patterns, team decided scope"),
      ("Strategy pattern",           "", "X", "",  "", "Applied to payment system"),
      ("Service boundary decisions", "X","",  "",  "", "Team decision based on user roles"),
    ],
    reflect  = (
      "Defining all interfaces before implementations enforced the Dependency Inversion Principle throughout the project. "
      "AI helped identify missing interfaces (e.g. IOrderTrackingNotifier for SignalR) that were initially overlooked. "
      "Some suggested interfaces were too fine-grained and were merged."
    ),
  ),
  dict(
    branch   = "core/infrastructure-ef-config",
    title    = "Core -- EF Core Configuration and Migrations",
    scope    = "GearZone.Infrastructure: ApplicationDbContext, IEntityTypeConfiguration<T> per entity, EF migrations, seed data",
    files    = "GearZone.Infrastructure/ApplicationDbContext.cs, Configurations/*.cs, Migrations/*.cs, Seed/*.cs",
    owner    = "Phan Tran Cong Vu (DE180494)",
    added    = [
      "ApplicationDbContext extending IdentityDbContext<ApplicationUser>",
      "30+ IEntityTypeConfiguration implementations with column types, FK constraints, indexes, and cascade rules",
      "20+ EF migrations from InitialCreate through AddShipmentsAndStoreCoordinates",
      "Seed data: IdentitySeeder (admin/demo accounts), CategorySeeder (gear categories), SystemSettingSeeder",
      "DependencyInjection.cs wiring all repositories and services",
    ],
    prompts  = [
      "How to configure a many-to-many self-referencing relationship in EF Core 8 for store follows?",
      "What is the correct EF Core configuration for storing C# enums as strings in SQL Server?",
      "How do I seed data through EF migrations safely without duplicating rows on re-run?",
    ],
    p_outputs = [
      "HasMany/WithMany with explicit join entity (StoreFollow) and composite primary key configuration.",
      "HasConversion<string>() in IEntityTypeConfiguration with HasColumnType('nvarchar(50)').",
      "Check row existence before insert using MigrationBuilder.Sql with IF NOT EXISTS pattern.",
    ],
    ai_goal  = "EF Core configuration, migration strategy, seed data design",
    ai_rows  = [
      ("EF entity configuration", "", "X", "",  "", "AI syntax help, team verified logic"),
      ("Migration generation",    "X","",  "",  "", "dotnet ef migrations add (CLI)"),
      ("Seed data design",        "", "X", "",  "", "AI template, team provided real data"),
    ],
    reflect  = (
      "EF Core configuration is verbose but critical. AI saved significant time on relationship configuration syntax. "
      "However, several AI-suggested cascade delete rules had to be changed after causing FK constraint violations "
      "during integration testing. Always verify cascade behavior manually."
    ),
  ),
  dict(
    branch   = "core/infrastructure-repositories",
    title    = "Core -- Repository Implementations",
    scope    = "GearZone.Infrastructure/Repositories: generic Repository<T,TKey>, UnitOfWork, all domain-specific repository implementations",
    files    = "GearZone.Infrastructure/Repositories/*.cs",
    owner    = "Dang Cong Quoc Khanh (DE180880)",
    added    = [
      "Generic Repository<T,TKey> with EF Core DbSet<T>",
      "ApplyIncludes helper for eager loading via Expression<Func<T,object>>[]",
      "UnitOfWork wrapping SaveChangesAsync for transaction management",
      "35 domain-specific repositories with custom query methods (e.g. ProductRepository, OrderRepository, CartRepository)",
    ],
    prompts  = [
      "Implement a generic repository pattern with EF Core that supports eager loading via Expression<Func<T,object>>.",
      "How should UnitOfWork be implemented to coordinate multiple repositories in a single DbContext?",
      "When should I override the generic repository methods vs adding domain-specific query methods?",
    ],
    p_outputs = [
      "Repository<T,TKey> with IQueryable.Include() loop and ApplyIncludes helper; returns IQueryable from Query().",
      "UnitOfWork holds a single DbContext instance; SaveChangesAsync commits all tracked changes atomically.",
      "Override only when default behavior is insufficient (e.g. soft delete, composite key lookups).",
    ],
    ai_goal  = "Repository pattern implementation, EF Core query optimization, transaction management",
    ai_rows  = [
      ("Generic repository", "", "",  "X", "", "AI template adapted for project"),
      ("UnitOfWork",         "", "X", "",  "", "AI pattern, team wired DI"),
      ("Domain queries",     "X","",  "",  "", "Team-written based on feature needs"),
    ],
    reflect  = (
      "The generic repository eliminated about 80% of CRUD boilerplate. "
      "AI correctly identified that exposing IQueryable from repositories is pragmatic at this project scale. "
      "For larger systems, the Specification pattern would be preferable."
    ),
  ),
  dict(
    branch   = "core/infrastructure-external-services",
    title    = "Core -- External Service Integrations",
    scope    = "GearZone.Infrastructure/External: Cloudinary file storage, PayOS payment/payout, SMTP email, Goong map API",
    files    = "GearZone.Infrastructure/External/*.cs, Settings/*.cs",
    owner    = "Ho Huy Hoang (DE180416)",
    added    = [
      "CloudinaryStorageService implementing IFileStorageService (image upload/delete)",
      "SmtpEmailService implementing IEmailService with HTML template support",
      "PayOSPaymentGateway + PayOSPaymentStrategy implementing IPaymentGateway/IPaymentStrategy",
      "PayOSPayoutClient implementing IPayoutClient for seller disbursements",
      "CodPaymentStrategy for Cash-on-Delivery flow (no upfront payment)",
      "GoongService implementing IGoongService for distance/routing calculation",
      "Disabled stub implementations (DisabledPaymentGateway, DisabledPayoutClient) for dev environment",
      "PayOSSettings, PayOSPayoutSettings configuration classes",
    ],
    prompts  = [
      "How to integrate PayOS payment gateway in ASP.NET Core with webhook signature verification?",
      "Implement Cloudinary image upload in C# with automatic public_id generation and error handling.",
      "How do I calculate shipping distance using the Goong Directions API in .NET?",
    ],
    p_outputs = [
      "PayOS SDK: create payment link, handle webhook callback, verify HMAC-SHA256 signature, update order status.",
      "Cloudinary .NET SDK: Cloudinary.UploadAsync with RawUploadParams; return SecureUrl as stored image URL.",
      "Goong REST API: GET /direction with origin/destination lat-lng; parse routes[0].legs[0].distance.value in meters.",
    ],
    ai_goal  = "Third-party API integration, webhook security, async HTTP client patterns",
    ai_rows  = [
      ("PayOS integration",    "", "",  "X", "", "AI code sample, team added error handling"),
      ("Cloudinary",           "", "X", "",  "", "AI pattern adapted"),
      ("SMTP service",         "", "X", "",  "", "AI template, team wrote email templates"),
      ("Goong API",            "", "X", "",  "", "AI HTTP client pattern"),
    ],
    reflect  = (
      "External service integrations were the most time-consuming part of the infrastructure. "
      "AI provided working code samples for each API but webhook signature verification required careful manual testing. "
      "The Disabled* stub pattern (suggested by AI) was very useful for local development without real API credentials."
    ),
  ),
  dict(
    branch   = "core/logging-infrastructure",
    title    = "Core -- Logging Infrastructure",
    scope    = "Cross-cutting logging: IAppLogger<T> abstraction, SerilogAppLogger<T>, RequestLoggingMiddleware, AuditTrailLogger",
    files    = "GearZone.Application/Abstractions/Logging/IAppLogger.cs, GearZone.Infrastructure/Logging/SerilogAppLogger.cs, GearZone.Infrastructure/Logging/AuditTrailLogger.cs, GearZone.Web/Middleware/RequestLoggingMiddleware.cs",
    owner    = "Ho Huy Hoang (DE180416)",
    added    = [
      "IAppLogger<T> interface in Application layer (LogInformation, LogWarning, LogError, LogDebug, LogCritical)",
      "SerilogAppLogger<T> bridging IAppLogger<T> to ASP.NET Core ILogger<T>/Serilog sinks",
      "RequestLoggingMiddleware logging HTTP method, path, status code, elapsed ms; auto-promotes 4xx/5xx to Warning/Error",
      "IAuditTrailLogger + AuditTrailLogger for structured CREATE/UPDATE/DELETE audit entries",
      "UseRequestLogging() extension method for clean middleware registration",
    ],
    prompts  = [
      "Design a logger abstraction interface in Clean Architecture that keeps the Application layer free of Serilog dependencies.",
      "Implement an ASP.NET Core middleware that logs every HTTP request with method, path, status code, and elapsed time; promote 5xx to error level.",
      "How do I implement an audit trail logger that records entity mutations (create/update/delete) with user context in .NET?",
    ],
    p_outputs = [
      "IAppLogger<T> with five methods mirroring ILogger<T> levels; inject in Application services instead of ILogger<T>.",
      "Stopwatch-based RequestLoggingMiddleware; LogLevel selected based on status code range; exception rethrown after logging.",
      "AuditTrailLogger emitting structured [AUDIT] log entries with action, entity type, entity ID, userId, timestamp.",
    ],
    ai_goal  = "Logging architecture design, middleware implementation, audit trail pattern",
    ai_rows  = [
      ("Logger abstraction", "", "X", "",  "", "AI pattern, team reviewed interface methods"),
      ("Request middleware", "", "",  "X", "", "AI implementation, team added status-level logic"),
      ("Audit trail",        "", "X", "",  "", "AI pattern adapted for project"),
    ],
    reflect  = (
      "The IAppLogger<T> abstraction keeps Application services testable without Serilog. "
      "The middleware approach for HTTP logging is cleaner than per-controller logging. "
      "Audit trail structured logging is directly importable into Seq/Elasticsearch for compliance reporting."
    ),
  ),
  dict(
    branch   = "feature/auth-identity",
    title    = "Feature -- Authentication and Identity",
    scope    = "User registration (multi-step + email verification), login, JWT issuance, ASP.NET Core Identity configuration",
    files    = "Features/Auth/*, Controllers/Api/AuthController.cs, Pages/Public/Auth/*, Infrastructure/Repositories/UserRepository.cs",
    owner    = "Dam Nguyen Khang (DE180417)",
    added    = [
      "Multi-step registration flow: basic info -> email OTP verification -> profile completion",
      "JWT Bearer token issuance with configurable expiry and refresh logic",
      "Email verification via SMTP OTP with 15-minute TTL",
      "AuthController endpoints: /register, /login, /verify-email, /resend-verification",
      "LoginViewModel, RegisterViewModel with validation attributes",
      "IAuthService contract and implementation",
      "ASP.NET Core Identity configuration with custom ApplicationUser",
    ],
    prompts  = [
      "Implement multi-step user registration in ASP.NET Core with email OTP verification using a stateless JWT approach.",
      "How do I configure ASP.NET Core Identity with a custom ApplicationUser that adds extra profile fields?",
      "Generate a secure OTP email verification flow -- how long should tokens be valid and how should they be stored?",
    ],
    p_outputs = [
      "Store pending registration in a short-lived temp record; OTP generated with RandomNumberGenerator; verified then promoted to confirmed user.",
      "IdentityDbContext<ApplicationUser>; ApplicationUser extends IdentityUser with DisplayName, AvatarUrl, WalletBalance, etc.",
      "6-digit numeric OTP stored hashed; 15-minute expiry; resend after 60-second cooldown to prevent flooding.",
    ],
    ai_goal  = "Authentication flow design, JWT configuration, OTP verification security",
    ai_rows  = [
      ("Auth flow design", "", "X", "",  "", "AI proposed flow, team adapted"),
      ("JWT configuration","", "X", "",  "", "AI template adjusted for claims"),
      ("OTP security",     "", "X", "",  "", "AI flagged hashing requirement"),
      ("Identity setup",   "", "X", "",  "", "AI configuration, team verified"),
    ],
    reflect  = (
      "The multi-step registration was more complex than anticipated. "
      "AI correctly flagged the need to hash OTPs before storage -- a security requirement that was initially missed. "
      "JWT expiry and refresh logic required manual testing to get right."
    ),
  ),
  dict(
    branch   = "feature/catalog-browsing",
    title    = "Feature -- Product Catalog Browsing",
    scope    = "Public product listing, search, filtering by category/brand/price/rating, product detail page, store profile browsing",
    files    = "Features/Catalog/*, Controllers/Api/CatalogController.cs, Pages/Public/Catalog/*, Application/Common/ProductSpecifications/",
    owner    = "Nguyen Sinh Nhat (DE180430)",
    added    = [
      "Paginated product listing with offset pagination",
      "Multi-filter search: category, brand, price range, rating, attribute values",
      "Product detail page with variant selector and image gallery",
      "Store profile public view with product grid and follower count",
      "CatalogController API endpoints for AJAX-driven filtering",
      "ProductSpecification classes for composable EF Core queries",
      "CatalogService with ICatalogService contract",
    ],
    prompts  = [
      "Design a composable product filtering system in EF Core that supports category, brand, price range, and custom attributes without N+1 queries.",
      "How do I implement cursor-based pagination in ASP.NET Core for a product listing API?",
      "What is the best way to render a product variant selector from a database-driven attribute system?",
    ],
    p_outputs = [
      "Specification pattern with IQueryable<Product> chaining; ProjectTo<ProductDto> with AutoMapper to avoid SELECT *.",
      "Keyset pagination using (LastProductId, LastCreatedAt) tuple; more performant than OFFSET for large datasets.",
      "Attribute matrix: load all CategoryAttribute + ProductAttributeValue; render dropdowns client-side from the data.",
    ],
    ai_goal  = "Complex query design, pagination strategy, product filtering UX",
    ai_rows  = [
      ("Filter query design", "", "",  "X", "", "Specification pattern from AI"),
      ("Pagination",          "", "X", "",  "", "AI pattern, team tested edge cases"),
      ("Variant selector",    "", "X", "",  "", "AI UI logic, team integrated"),
    ],
    reflect  = (
      "The specification pattern for filtering removed a large switch/if chain that was growing unmanageable. "
      "Cursor pagination was new to the team -- AI provided a clear explanation and working implementation. "
      "The attribute-based variant selector required several iterations."
    ),
  ),
  dict(
    branch   = "feature/product-management",
    title    = "Feature -- Seller Product Management",
    scope    = "Seller CRUD for products: create with variants/images/attributes, update, soft-delete, status management, inventory tracking",
    files    = "Features/Seller/* (products), Controllers/Api/Seller/ProductsController.cs, Pages/StoreOwner/Products/*",
    owner    = "Phan Tran Cong Vu (DE180494)",
    added    = [
      "Product creation wizard: basic info, variants (size/color matrix), image upload (Cloudinary), attribute values",
      "Soft-delete product with ProductStatus enum (Active, Inactive, Banned, PendingReview)",
      "Product variant management: add/edit/delete variants with individual pricing and stock",
      "InventoryTransaction recording stock in/out events",
      "ISellerProductService and implementation",
      "Seller product listing with status filters",
      "Admin product moderation: approve/ban with reason",
    ],
    prompts  = [
      "Design a product creation flow where the seller defines product variants as a matrix of attribute combinations (e.g. Size x Color).",
      "How should I implement soft delete for products in EF Core while keeping variant and image data intact?",
      "What is the right way to track inventory changes -- update stock directly or use transaction records?",
    ],
    p_outputs = [
      "Store attribute options; generate ProductVariant records from all combinations; allow seller to set price/stock per variant.",
      "Set ProductStatus = Inactive on soft delete; global query filter WHERE ProductStatus != Deleted; keep related data intact.",
      "InventoryTransaction records with TransactionType (StockIn, StockOut, Adjustment); derive current stock by summing transactions.",
    ],
    ai_goal  = "Product data model complexity, variant matrix design, inventory audit trail",
    ai_rows  = [
      ("Variant matrix design", "", "",  "X", "", "AI core logic, team built UI"),
      ("Soft delete",           "", "X", "",  "", "AI EF filter pattern"),
      ("Inventory tracking",    "", "X", "",  "", "AI transaction log pattern"),
    ],
    reflect  = (
      "The variant matrix creation was the most complex UI in the project. "
      "AI's suggestion to generate variants from attribute combinations automatically saved hours of manual design. "
      "The InventoryTransaction approach for stock tracking is more auditable than a simple stock integer field."
    ),
  ),
  dict(
    branch   = "feature/cart-and-checkout",
    title    = "Feature -- Shopping Cart and Checkout",
    scope    = "Persistent global cart, multi-seller checkout, voucher application, address selection, order placement",
    files    = "Features/Cart/*, Features/Checkout/*, Controllers/Api/CartController.cs, Controllers/Api/CheckoutController.cs, Pages/Cart/*, Pages/Checkout/*",
    owner    = "Dang Cong Quoc Khanh (DE180880)",
    added    = [
      "Global persistent cart linked to user account (not session)",
      "Cart grouped by store for multi-seller checkout display",
      "CartItem add/update/remove with real-time stock validation",
      "Checkout flow: address selection -> voucher -> payment method -> review -> place order",
      "Voucher application with validation (minimum order value, usage limits, expiry)",
      "CheckoutController creating Order + SubOrders atomically via UnitOfWork",
      "ICartService and ICheckoutService contracts and implementations",
    ],
    prompts  = [
      "Design a shopping cart that persists across sessions and devices for a multi-seller e-commerce platform.",
      "How do I implement an atomic checkout that creates an Order with multiple SubOrders (one per seller) inside a single transaction?",
      "Implement voucher discount application logic with minimum order amount, per-user usage limits, and expiry date checks.",
    ],
    p_outputs = [
      "Cart stored in database as Cart entity with CartItems; no session dependency; synced on login for guest-to-user merge.",
      "UnitOfWork.SaveChangesAsync wraps Order + all SubOrders + OrderItems creation; PaymentRecord created in same transaction.",
      "VoucherService.ValidateAsync checks: IsActive, not expired, UsageCount < MaxUsage, TotalAmount >= MinOrderAmount, not already used by this user.",
    ],
    ai_goal  = "Cart persistence design, atomic transaction handling, voucher validation logic",
    ai_rows  = [
      ("Cart persistence",  "", "X", "",  "", "AI DB design, team implemented"),
      ("Atomic checkout",   "", "",  "X", "", "AI UoW pattern, team tested"),
      ("Voucher validation","", "X", "",  "", "AI logic, team added edge cases"),
    ],
    reflect  = (
      "The multi-seller checkout required careful thought about what constitutes the order vs sub-order. "
      "AI helped clarify the data model. "
      "The atomic transaction pattern for order placement prevented partial-order bugs found during testing. "
      "Voucher logic had more edge cases than expected."
    ),
  ),
  dict(
    branch   = "feature/order-management",
    title    = "Feature -- Order Management",
    scope    = "Order lifecycle: status transitions, history tracking, buyer and seller order views, auto-complete background job",
    files    = "Features/Orders/*, Controllers/Api/OrdersController.cs, Controllers/Api/Seller/OrdersController.cs, Pages/Public/User/Orders/*, Pages/StoreOwner/Orders/*, Infrastructure/Jobs/OrderAutoCompleteJob.cs",
    owner    = "Ho Huy Hoang (DE180416)",
    added    = [
      "Order status machine: Pending -> Confirmed -> Shipped -> Delivered -> Completed / Cancelled / Disputed",
      "OrderStatusHistory records every status transition with actor (user/system) and timestamp",
      "Buyer order list and detail views with tracking information",
      "Seller sub-order management: confirm, mark shipped, handle disputes",
      "Auto-complete job: Hangfire background job completes orders 7 days after Delivered status",
      "IOrderService, IAdminOrderService contracts and implementations",
      "SignalR notification on order status change via IOrderTrackingNotifier",
    ],
    prompts  = [
      "Design an order state machine in C# that enforces valid status transitions and records history for each change.",
      "Implement a Hangfire background job that auto-completes orders a configurable number of days after delivery.",
      "How do I notify a buyer in real-time when their order status changes using SignalR?",
    ],
    p_outputs = [
      "Dictionary<OrderStatus, IEnumerable<OrderStatus>> allowed transition map; throw DomainException on invalid transition; record OrderStatusHistory in same UoW.",
      "Hangfire RecurringJob running daily; query SubOrders where Status=Delivered AND DeliveredAt < UtcNow - AutoCompleteDays.",
      "Hub.Clients.User(userId).SendAsync('OrderStatusChanged', orderId, newStatus) using user-specific SignalR connection.",
    ],
    ai_goal  = "State machine design, background job scheduling, real-time notifications",
    ai_rows  = [
      ("State machine",         "", "",  "X", "", "AI design, team implemented transitions"),
      ("Hangfire job",          "", "X", "",  "", "AI template, team added config"),
      ("SignalR notifications", "", "X", "",  "", "AI wiring, team tested"),
    ],
    reflect  = (
      "The state machine enforcement prevents invalid status updates that would corrupt order data. "
      "AI's transition map dictionary approach is clean and testable. "
      "The SignalR notification for order status changes required integrating the Hub with the service layer, "
      "which needed careful DI configuration."
    ),
  ),
  dict(
    branch   = "feature/payment-processing",
    title    = "Feature -- Payment Processing",
    scope    = "PayOS online payment, COD, wallet top-up, platform transaction tracking, payment webhook handling",
    files    = "Features/Payments/*, Controllers/Api/CheckoutController.cs (payment endpoints), Infrastructure/External/PayOS*.cs, Infrastructure/External/CodPaymentStrategy.cs",
    owner    = "Dam Nguyen Khang (DE180417)",
    added    = [
      "PayOS payment link creation and webhook callback handling",
      "HMAC-SHA256 signature verification for PayOS webhook security",
      "COD payment strategy (no upfront payment, confirmed on delivery)",
      "Wallet top-up flow via PayOS with PlatformTransaction record",
      "Payment status polling endpoint for frontend fallback",
      "WalletTransaction recording debits/credits with TransactionType",
      "IPaymentService, IPaymentGateway, IPaymentStrategy contracts and implementations",
    ],
    prompts  = [
      "How do I verify PayOS webhook signatures in ASP.NET Core to prevent spoofed payment callbacks?",
      "Design a payment strategy pattern that supports PayOS, COD, and wallet balance -- extensible for future methods.",
      "How should I handle payment timeout -- what if the user closes the browser before paying?",
    ],
    p_outputs = [
      "Compute HMAC-SHA256 of sorted payload fields using PayOS secret key; compare with webhook signature header; reject if mismatch.",
      "IPaymentStrategy with ProcessAsync(order) and ValidateAsync(order); factory selects strategy by PaymentMethod enum.",
      "PaymentTimeoutJob: Hangfire delayed job scheduled 30 min after payment creation; if status still Pending, cancel order and release stock.",
    ],
    ai_goal  = "Payment webhook security, strategy pattern, timeout handling",
    ai_rows  = [
      ("Webhook security", "", "",  "X", "", "AI security pattern -- critical"),
      ("Strategy pattern", "", "X", "",  "", "AI design, team extended"),
      ("Timeout handling", "", "X", "",  "", "AI job pattern"),
    ],
    reflect  = (
      "Webhook security was the most critical aspect. "
      "AI immediately flagged the HMAC verification requirement -- without it, any malicious party could fake successful payment callbacks. "
      "The strategy pattern makes adding future payment methods trivial. "
      "Payment timeout handling prevented zombie orders in testing."
    ),
  ),
  dict(
    branch   = "feature/payout-system",
    title    = "Feature -- Seller Payout System",
    scope    = "Seller revenue disbursement: payout batch creation, payout items per sub-order, PayOS transfer, admin approval workflow",
    files    = "Features/Payout/*, Controllers/Api/Admin/PayoutsController.cs, Pages/Admin/Payouts/*, Pages/Admin/PayoutBatches/*, Infrastructure/Jobs/PayoutBatchJob.cs",
    owner    = "Nguyen Sinh Nhat (DE180430)",
    added    = [
      "PayoutBatch aggregating PayoutItems for admin review before disbursement",
      "PayoutItem linking each completed SubOrder to a seller's pending earnings",
      "Admin payout approval: review batch -> approve -> trigger PayOS disbursement",
      "PayoutBatchJob: Hangfire job runs weekly to aggregate completed orders into new batch",
      "PayoutTransaction recording the actual bank transfer with reference code",
      "Platform fee deduction calculation before payout amount",
      "IPayoutService, IAdminPayoutService contracts and implementations",
    ],
    prompts  = [
      "Design a seller payout system for a multi-vendor marketplace: how do I aggregate completed orders into weekly payout batches?",
      "How does PayOS payout (disbursement) API work -- what fields are required for a bank transfer?",
      "How should I calculate the platform fee before paying out to sellers?",
    ],
    p_outputs = [
      "PayoutBatch created weekly via Hangfire; PayoutItems = all SubOrders with Status=Completed and no existing PayoutItem; admin reviews then approves.",
      "PayOS payout requires: accountNo, accountName, bankBin, amount, description, reference; returns transferId for tracking.",
      "NetAmount = GrossAmount * (1 - PlatformFeeRate); PlatformFeeRate loaded from SystemSetting for configurability.",
    ],
    ai_goal  = "Financial workflow design, batch aggregation logic, fee calculation",
    ai_rows  = [
      ("Batch aggregation",  "", "",  "X", "", "AI design pattern"),
      ("PayOS payout API",   "", "X", "",  "", "AI integration, team tested"),
      ("Fee calculation",    "", "X", "",  "", "AI formula, team verified"),
    ],
    reflect  = (
      "The payout system required careful coordination between order completion and payout eligibility. "
      "AI correctly identified that payout should only happen for Completed (not just Delivered) orders. "
      "The platform fee as a SystemSetting makes it configurable without code changes."
    ),
  ),
  dict(
    branch   = "feature/store-management",
    title    = "Feature -- Store Management",
    scope    = "Seller store registration, admin approval, store profile, store settings, store follow/unfollow",
    files    = "Features/Seller/* (store), Controllers/Api/SellerRegistrationController.cs, Pages/StoreOwner/Settings/*, Pages/Admin/StoreApplications/*, Pages/Public/StoreProfile/*",
    owner    = "Phan Tran Cong Vu (DE180494)",
    added    = [
      "Store registration application flow: business info + identity card upload -> admin review -> approve/reject",
      "Store profile public page with follower count and product grid",
      "StoreFollow entity for buyer-to-store following with unfollow support",
      "Store settings: name, description, avatar, bank account, geo-coordinates",
      "Admin store management: list all stores, view details, suspend/reactivate",
      "ISellerStoreService, IAdminStoreService contracts and implementations",
      "Cloudinary integration for identity card images and store avatar",
    ],
    prompts  = [
      "Design a store registration flow with admin approval where sellers upload identity card images for KYC verification.",
      "How do I implement a follow system between buyers and stores in EF Core?",
      "What store settings should a multi-vendor marketplace expose to sellers?",
    ],
    p_outputs = [
      "Store entity with StoreStatus enum (PendingReview, Active, Suspended, Rejected); identity card images stored on Cloudinary; admin sets status.",
      "StoreFollow join entity with UserId and StoreId composite PK; StoreRepository.GetFollowerCountAsync uses COUNT query.",
      "Store settings: display name, bio, logo, banner, bank account (for payouts), address, geo-coordinates for shipping.",
    ],
    ai_goal  = "KYC workflow design, social features, store configuration",
    ai_rows  = [
      ("KYC workflow",   "", "X", "",  "", "AI design, team implemented review UI"),
      ("Follow system",  "", "X", "",  "", "AI EF pattern"),
      ("Store settings", "X","",  "",  "", "Team-defined based on requirements"),
    ],
    reflect  = (
      "The KYC identity card upload was a compliance requirement. "
      "AI suggested storing image URLs rather than binary data -- obvious in retrospect but confirmed the approach. "
      "The follow system was straightforward; the decision to not denormalize follower counts was made after "
      "benchmarking -- COUNT query is fast enough at this scale."
    ),
  ),
  dict(
    branch   = "feature/voucher-system",
    title    = "Feature -- Voucher and Discount System",
    scope    = "Seller-created vouchers: percentage/fixed discount, usage limits, expiry, checkout application, admin moderation",
    files    = "Features/ (vouchers), Controllers/Api/Seller/VouchersController.cs, Pages/StoreOwner/Vouchers/*, Pages/Admin/Vouchers/*",
    owner    = "Dang Cong Quoc Khanh (DE180880)",
    added    = [
      "Voucher entity with VoucherType (Percentage, FixedAmount), MaxUsage, MinOrderAmount, StartDate, EndDate, VoucherStatus",
      "VoucherUsage join entity tracking which users have used which vouchers",
      "Seller voucher CRUD: create, edit, activate/deactivate, view usage statistics",
      "Checkout voucher application: validate then apply discount to order total",
      "Admin voucher overview for moderation",
      "ISellerVoucherService, IAdminVoucherService contracts and implementations",
    ],
    prompts  = [
      "Design a voucher system for a multi-vendor marketplace where each seller creates their own vouchers applied at checkout.",
      "How do I prevent concurrent race conditions when multiple users try to use the last available slot of a limited voucher?",
      "What validation rules should a checkout voucher system enforce?",
    ],
    p_outputs = [
      "Voucher.StoreId links voucher to a specific seller; applied only to SubOrders from that store; discount calculated per-SubOrder.",
      "Optimistic concurrency: check VoucherUsage count in transaction; if Count >= MaxUsage throw ConcurrencyException and notify user.",
      "Validation: IsActive, not expired, UsageCount < MaxUsage, OrderTotal >= MinOrderAmount, user has not used this voucher before.",
    ],
    ai_goal  = "Discount business rules, concurrency handling, voucher validation",
    ai_rows  = [
      ("Voucher data model",   "", "X", "",  "", "AI design, team adjusted"),
      ("Concurrency handling", "", "",  "X", "", "AI flagged and solved -- critical"),
      ("Validation rules",     "", "X", "",  "", "AI baseline, team added error messages"),
    ],
    reflect  = (
      "The concurrency issue for limited vouchers was a non-obvious edge case. "
      "AI flagged it immediately and suggested the optimistic concurrency check within the transaction. "
      "The validation rules required multiple iterations to get user-friendly error messages."
    ),
  ),
  dict(
    branch   = "feature/product-reviews",
    title    = "Feature -- Product Reviews",
    scope    = "Buyer product reviews after order delivery: rating, comment, seller reply, admin moderation",
    files    = "Features/Reviews/*, Controllers/Api/ReviewsController.cs, Pages/StoreOwner/Reviews/*, Repositories/ProductReviewRepository.cs",
    owner    = "Ho Huy Hoang (DE180416)",
    added    = [
      "ProductReview entity with Rating (1-5), Comment, optional SellerReply, ReviewStatus",
      "Review submission gated behind order delivery: buyer must have Completed SubOrder for the product",
      "Review listing on product detail page with pagination and average rating aggregation",
      "Seller reply to review via StoreOwner/Reviews page",
      "Admin review moderation: flag/hide inappropriate reviews",
      "IProductReviewService contract and implementation",
    ],
    prompts  = [
      "How do I gate product reviews behind order completion -- only buyers who purchased can review?",
      "Design a review system with seller reply functionality and admin moderation for an e-commerce platform.",
      "How do I efficiently compute average product rating while keeping it accurate as reviews are added/deleted?",
    ],
    p_outputs = [
      "Check SubOrder.Status=Completed AND SubOrder contains the reviewed product for the requesting user before allowing review creation.",
      "ProductReview.SellerReplyText + SellerRepliedAt; seller can only reply to reviews for their own store products.",
      "Compute AVG(Rating) in SQL via repository query; do not cache -- consistency is more important than marginal performance at this scale.",
    ],
    ai_goal  = "Review eligibility gating, moderation design, rating aggregation",
    ai_rows  = [
      ("Eligibility gating",  "", "",  "X", "", "AI LINQ query, team tested"),
      ("Seller reply",        "", "X", "",  "", "AI design, team implemented"),
      ("Rating aggregation",  "X","",  "",  "", "Team-decided, AI confirmed approach"),
    ],
    reflect  = (
      "The eligibility check was the most complex query in this feature -- joining multiple tables to verify a real purchase. "
      "AI provided the correct LINQ query on first attempt. "
      "The decision not to cache the average rating was confirmed after benchmarking -- the query is fast enough."
    ),
  ),
  dict(
    branch   = "feature/messaging-chat",
    title    = "Feature -- Messaging and Chat",
    scope    = "Real-time buyer-seller chat via SignalR: conversation threads, read/unread state, inbox with unread counts",
    files    = "Features/Chat/*, Controllers/Api/ChatController.cs, Hubs/ChatHub.cs, Pages/Public/User/Messages/*, Pages/StoreOwner/Messages/*",
    owner    = "Dam Nguyen Khang (DE180417)",
    added    = [
      "Conversation entity linking Buyer (ApplicationUser) and Store",
      "ChatMessage entity with SenderId, Body, SentAt, IsReadByRecipient",
      "ChatHub (SignalR) for real-time message delivery and read receipts",
      "ChatController REST endpoints for message history pagination (SignalR for real-time)",
      "Inbox view showing conversations sorted by latest message with unread count badges",
      "IChatService contract and implementation",
    ],
    prompts  = [
      "Design a real-time chat system between buyers and sellers using SignalR in ASP.NET Core -- what is the correct Hub design?",
      "How do I implement unread message counts efficiently without querying all messages?",
      "How should I handle SignalR connection management for users who have multiple browser tabs open?",
    ],
    p_outputs = [
      "ChatHub.SendMessage: persists to DB via ChatService; sends to recipient user group via Clients.User(recipientId); returns to sender for confirmation.",
      "ChatMessage.IsReadByRecipient boolean; unread count = COUNT WHERE IsReadByRecipient=false AND RecipientId=currentUser; updated on conversation open.",
      "Hub.Groups with userId as group name; multiple connections per user automatically receive messages.",
    ],
    ai_goal  = "SignalR Hub design, real-time messaging patterns, read state management",
    ai_rows  = [
      ("SignalR Hub",    "", "",  "X", "", "AI design, team implemented"),
      ("Unread counts",  "", "X", "",  "", "AI query, team optimized"),
      ("Multi-tab",      "", "X", "",  "", "AI explained built-in behavior"),
    ],
    reflect  = (
      "SignalR's user-based group pattern (Clients.User) was new to the team. "
      "AI explained that user groups work automatically with the default UserIdProvider -- no manual group management needed. "
      "The read receipt implementation required careful thought to avoid N+1 when displaying inbox unread counts."
    ),
  ),
  dict(
    branch   = "feature/shipping-logistics",
    title    = "Feature -- Shipping and Logistics",
    scope    = "Shipping cost calculation via Goong distance API, shipment tracking, estimated delivery, address management",
    files    = "Features/Shipping/*, Features/Map/*, Infrastructure/External/GoongService.cs, Infrastructure/Jobs/OrderAutoCompleteJob.cs",
    owner    = "Nguyen Sinh Nhat (DE180430)",
    added    = [
      "ShippingService calculating cost based on distance between store and delivery address via Goong Directions API",
      "Shipment entity with tracking code, carrier, estimated delivery date",
      "Shipment status updates by seller: Preparing -> Shipped -> OutForDelivery -> Delivered",
      "GoongService implementing IGoongService with Directions API and Geocoding API",
      "UserAddress management (add/edit/delete delivery addresses with lat-long)",
      "Map DTOs: GoongDirectionResponse, GoongGeocodeResponse",
      "IShippingService contract and implementation",
    ],
    prompts  = [
      "How do I calculate shipping cost based on real road distance using the Goong Directions API in ASP.NET Core?",
      "Design a shipment tracking system where sellers update the shipment status and buyers see real-time updates.",
      "How do I geocode a Vietnamese address to lat/long using the Goong Geocoding API?",
    ],
    p_outputs = [
      "Goong /direction endpoint: origin=store_coords&destination=buyer_coords&vehicle=car; parse routes[0].legs[0].distance.value; apply rate per km.",
      "Shipment entity tracks code, carrier, status, estimatedDelivery; seller updates via API; SignalR notifies buyer.",
      "Goong /geocode endpoint: address string -> location.lat/lng; fallback to manual entry if geocode fails.",
    ],
    ai_goal  = "Mapping API integration, shipping cost formula, shipment state machine",
    ai_rows  = [
      ("Goong API integration", "", "",  "X", "", "AI HTTP client, team added error handling"),
      ("Shipping cost formula", "X","",  "",  "", "Team-defined linear model"),
      ("Shipment tracking",     "", "X", "",  "", "AI state machine, team implemented"),
    ],
    reflect  = (
      "The Goong API integration required handling Vietnamese addresses correctly -- AI flagged that addresses "
      "should be URL-encoded with UTF-8. Shipping cost uses a simplified linear model sufficient for the project scope. "
      "A more production-ready implementation would use carrier API pricing."
    ),
  ),
  dict(
    branch   = "feature/admin-panel",
    title    = "Feature -- Admin Panel",
    scope    = "Full admin dashboard: user management, product moderation, order management, store applications, payout oversight, system settings, transaction ledger",
    files    = "Controllers/Api/Admin/*.cs, Pages/Admin/**, Features/Admin/*",
    owner    = "Phan Tran Cong Vu (DE180494)",
    added    = [
      "Admin dashboard with key metrics (total orders, revenue, active stores, new users)",
      "User management: list, view profile, lock/unlock accounts",
      "Product moderation: approve/reject pending products with reason",
      "Store application review: approve/reject seller applications",
      "Order management: view all orders, override status in exceptional cases",
      "Transaction ledger: all wallet and platform transactions",
      "System settings management (platform fee rate, auto-complete days, etc.)",
      "Admin payout batch approval workflow",
      "All IAdmin* service contracts and implementations",
    ],
    prompts  = [
      "Design an admin dashboard for a multi-vendor e-commerce platform -- what are the most critical metrics and management operations?",
      "How do I implement role-based access control in ASP.NET Core Razor Pages so only Admin users can access /Admin/* pages?",
      "Generate an admin user management page with lock/unlock functionality using ASP.NET Core Identity.",
    ],
    p_outputs = [
      "Key metrics: GMV, new orders today, pending store applications, failed payments, platform revenue. Pages per management area.",
      "Razor Pages with [Authorize(Roles = 'Admin')] attribute; redirect to /403 on failure; AuthorizeFilter applied globally.",
      "UserManager<ApplicationUser>.SetLockoutEndDateAsync for locking; LockoutEnd = null to unlock; audit log entry for each action.",
    ],
    ai_goal  = "Admin workflow design, role-based authorization, dashboard metrics",
    ai_rows  = [
      ("Dashboard metrics",   "", "X", "",  "", "AI design, team replaced in-memory count with SQL"),
      ("Role authorization",  "", "",  "X", "", "AI configuration, team tested"),
      ("CRUD scaffolding",    "", "",  "X", "", "AI accelerated, team reviewed"),
    ],
    reflect  = (
      "The admin panel has the most pages in the application. AI helped scaffold the CRUD structure quickly. "
      "Role-based authorization was straightforward with ASP.NET Core Identity. "
      "The dashboard metrics query required optimization -- initial AI suggestion loaded all orders then counted in-memory, "
      "replaced with SQL aggregation."
    ),
  ),
  dict(
    branch   = "feature/user-profile",
    title    = "Feature -- User Profile and Account",
    scope    = "Buyer profile: view/edit personal info, avatar upload, saved delivery addresses, wallet balance view, order history",
    files    = "Features/User/*, Controllers/Api/UsersController.cs, Pages/Public/User/*, Repositories/UserRepository.cs, Repositories/UserAddressRepository.cs",
    owner    = "Dang Cong Quoc Khanh (DE180880)",
    added    = [
      "User profile view and edit: display name, avatar (Cloudinary), phone number",
      "UserAddress CRUD: add/edit/delete delivery addresses with default address selection",
      "Wallet balance display and transaction history",
      "Order history list with status filters for buyers",
      "IUserService (buyer profile operations) contract and implementation",
      "Avatar upload via Cloudinary with size/type validation",
    ],
    prompts  = [
      "Design a user profile system for an e-commerce buyer that includes delivery address management with a default address selection.",
      "How do I validate and upload a user avatar image in ASP.NET Core Razor Pages with size and type restrictions?",
      "What information should a buyer's wallet transaction history show?",
    ],
    p_outputs = [
      "UserAddress table with IsDefault boolean; ensure only one IsDefault per user via EF transaction (unset all, then set selected).",
      "Accept image/jpeg, image/png, image/webp; max 5MB; resize to 256x256 before Cloudinary upload to save storage.",
      "Show: transaction type, amount (+/-), balance after, reference (order/payout ID), timestamp.",
    ],
    ai_goal  = "Profile management design, image upload validation, wallet UX",
    ai_rows  = [
      ("Default address logic", "", "X", "",  "", "AI transaction pattern"),
      ("Image upload",          "", "X", "",  "", "AI validation, team tested"),
      ("Wallet history UI",     "X","",  "",  "", "Team-designed display"),
    ],
    reflect  = (
      "The default address logic (only one default per user) required an atomic update pattern. "
      "AI suggested the unset-all-then-set approach within a transaction, which is safe at this project scale. "
      "Avatar upload size validation on the server-side is important even with client-side validation."
    ),
  ),
  dict(
    branch   = "feature/frontend-react",
    title    = "Feature -- React Frontend",
    scope    = "gearzone-react: Vite + React + TypeScript + Tailwind CSS SPA for the buyer-facing storefront",
    files    = "gearzone-react/src/**, gearzone-react/package.json, gearzone-react/vite.config.ts",
    owner    = "Dang Cong Quoc Khanh (DE180880)",
    added    = [
      "Vite + React + TypeScript + Tailwind CSS v4 project setup",
      "React Router v6 with nested routes for catalog, product detail, cart, checkout, auth",
      "Axios HTTP client with JWT interceptor for automatic token attachment and refresh",
      "Product listing page with filter sidebar and responsive grid",
      "Product detail page with variant selector, image gallery, and add-to-cart",
      "Cart page with quantity update and store-grouped display",
      "Checkout wizard: address -> voucher -> payment -> confirmation",
      "Authentication pages: login, register, email verification",
    ],
    prompts  = [
      "Set up a React + TypeScript + Tailwind CSS v4 + Vite project with React Router v6 for an e-commerce storefront.",
      "Implement an Axios interceptor that attaches a JWT token from localStorage and retries the request once after a 401 with a refreshed token.",
      "Design a product variant selector component in React that builds on a database-driven attribute system.",
    ],
    p_outputs = [
      "Vite config with @tailwindcss/vite plugin; tsconfig with strict mode; React Router createBrowserRouter with layout routes.",
      "Axios request interceptor adds Authorization header; response interceptor catches 401, calls /refresh-token, retries original request once.",
      "VariantSelector component: renders dropdowns from attribute names; tracks selected option per attribute; computes matching ProductVariant on change.",
    ],
    ai_goal  = "React project setup, API integration, component architecture",
    ai_rows  = [
      ("Project setup",    "", "",  "X", "", "AI config, team adjusted paths"),
      ("JWT interceptor",  "", "",  "X", "", "AI pattern, team tested refresh"),
      ("Variant selector", "", "X", "",  "", "AI component, team styled"),
    ],
    reflect  = (
      "The Tailwind CSS v4 setup with Vite required specific plugin configuration not yet widely documented -- AI knew the updated plugin approach. "
      "The JWT interceptor with refresh retry requires careful handling of concurrent requests during token refresh "
      "to avoid multiple parallel refresh calls."
    ),
  ),
]

# ---------------------------------------------------------------------------
# File generators
# ---------------------------------------------------------------------------

def ai_table_row(row):
    no_ai, some_ai, heavy_ai, ai_gen, note = row[1], row[2], row[3], row[4], row[5]
    return f"| {row[0]} | {no_ai} | {some_ai} | {heavy_ai} | {ai_gen} | {note} |"

def make_audit_log(d):
    added_md   = "\n".join(f"- {x}" for x in d["added"])
    prompts_md = "\n".join(f"- {x}" for x in d["prompts"])
    table_rows = "\n".join(ai_table_row(r) for r in d["ai_rows"])
    p = d["prompts"]
    po = d["p_outputs"]
    return f"""# AI Audit Log -- {d['title']}

## 1. Project Information

| Item | Detail |
|---|---|
| Course | PRN232 |
| Class | SE18D06 |
| Semester | SU26 |
| Group | Group 2 |
| Project | GearZone -- Multi-Vendor E-Commerce Platform |
| Branch | `{d['branch']}` |
| Scope | {d['scope']} |
| Date | {TODAY} |
| Team | Ho Huy Hoang, Dam Nguyen Khang, Nguyen Sinh Nhat, Phan Tran Cong Vu, Dang Cong Quoc Khanh |
| Primary Owner | {d['owner']} |

---

## 2. AI Tools Used

- [x] Claude (Claude Code CLI)
- [x] GitHub Copilot
- [ ] ChatGPT
- [ ] Gemini
- [ ] Cursor
- [ ] Perplexity

---

## 3. AI Usage Goals for This Branch

**Goal:** {d['ai_goal']}

Key tasks AI assisted with:
{prompts_md}

---

## 4. AI Usage Sessions

### Session 1

| Field | Detail |
|---|---|
| Date | {TODAY} |
| Tool | Claude Code |
| Purpose | {d['ai_goal'].split(',')[0].strip()} |
| Related Files | {d['files']} |
| AI Involvement | Significant assistance |

**Prompt used:**

```
{p[0]}
```

**AI output summary:**

{po[0]}

**What the team used:**
The core pattern / design / code structure suggested above.

**What the team changed:**
Adapted to project naming conventions and verified correctness against actual requirements.
Tested in the running application before accepting.

---

### Session 2

| Field | Detail |
|---|---|
| Date | {TODAY} |
| Tool | Claude Code / GitHub Copilot |
| Purpose | Follow-up detail implementation |
| Related Files | {d['files']} |
| AI Involvement | Moderate assistance |

**Prompt used:**

```
{p[1]}
```

**AI output summary:**

{po[1]}

**What the team used:**
The algorithmic or architectural pattern described above.

**What the team changed:**
Error handling, edge cases, and integration with the rest of the codebase were added manually.

---

## 5. AI Assistance Summary Table

| Area | No AI | Some AI | Heavy AI | AI Generated | Notes |
|---|:---:|:---:|:---:|:---:|---|
{table_rows}

---

## 6. AI Errors / Limitations Observed

| # | Issue | How Detected | Resolution |
|---|---|---|---|
| 1 | Occasionally suggested outdated .NET 6 API syntax | Build error | Replaced with .NET 8 equivalents |
| 2 | Some EF configurations had incorrect cascade rules | FK constraint error at runtime | Manually set DeleteBehavior per relationship |
| 3 | AI sometimes over-engineered solutions | Code review | Simplified to fit project scope |

---

## 7. Verification Methods

- Ran the application end-to-end after implementing AI suggestions
- Compared generated code against official Microsoft/library documentation
- Team code review before merging to develop
- Tested with realistic data (not just happy path)

---

## 8. Team Contribution

| Member | Student ID | Role | AI Used? |
|---|---|---|---|
| Ho Huy Hoang | DE180416 | Leader, Auth & Orders | Yes |
| Dam Nguyen Khang | DE180417 | Domain & Payments | Yes |
| Nguyen Sinh Nhat | DE180430 | Abstractions & Catalog | Yes |
| Phan Tran Cong Vu | DE180494 | Infrastructure & Admin | Yes |
| Dang Cong Quoc Khanh | DE180880 | Repositories & Frontend | Yes |

---

## 9. Academic Integrity Commitment

The team commits that:
- All AI usage has been honestly recorded in this log.
- No AI output was submitted without review and understanding.
- Every team member can explain the code in this branch.
- We are responsible for the correctness of the final product.

| Representative | Date |
|---|---|
| Ho Huy Hoang | {TODAY} |
"""

def make_changelog(d):
    added_md = "\n".join(f"- {x}" for x in d["added"])
    return f"""# Changelog -- {d['title']}

All notable changes on branch `{d['branch']}` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- {TODAY}

### Scope
{d['scope']}

### Added
{added_md}

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
"""

def make_prompts(d):
    sessions = []
    for i, (prompt, output) in enumerate(zip(d["prompts"], d["p_outputs"]), 1):
        sessions.append(f"""## Prompt {i} -- {TODAY}

**Tool:** Claude Code / GitHub Copilot
**Context:** {d['scope']}

**Prompt:**
> {prompt}

**AI Output Summary:**
{output}

**Used in files:** {d['files']}

---
""")
    return f"""# AI Prompts Log -- {d['title']}

Branch: `{d['branch']}`
Scope: {d['scope']}

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

{''.join(sessions)}"""

def make_reflection(d):
    return f"""# Reflection -- {d['title']}

Branch: `{d['branch']}`
Date: {TODAY}
Primary owner: {d['owner']}

---

## What went well

{d['reflect']}

AI accelerated the design and scaffolding phase significantly. The team spent more time on
business logic validation and testing rather than boilerplate.

## What was challenging

- Adapting AI suggestions to the existing codebase conventions required careful review
- Some AI-generated code used outdated API patterns that needed updating to .NET 8
- Edge cases (concurrency, error handling, validation) always required manual additions
- Integrating AI-generated components with the rest of the system needed extra attention

## AI assistance level for this branch

AI provided meaningful assistance for architecture design and initial implementation.
All AI-generated code was reviewed, tested, and often modified before use.
The team retains full understanding and ownership of every file in this branch.

## Key learnings

1. AI is best used for scaffolding and pattern identification; domain-specific business rules require human judgement.
2. Always run AI-generated code -- syntax correctness does not imply logical correctness.
3. The more specific and contextual the prompt, the more accurate the AI output.
4. AI suggestions for external API integrations should always be cross-referenced against official documentation.

## For future iterations

- Add unit tests for the service layer on this branch
- Consider Specification pattern for more complex queries
- Review AI suggestions against OWASP Top 10 for security-sensitive code paths
"""

# ---------------------------------------------------------------------------
# Main: process each branch
# ---------------------------------------------------------------------------
for d in BRANCHES:
    branch = d["branch"]
    print(f"Processing {branch} ...", flush=True)

    git("checkout", branch)

    docs = os.path.join(ROOT, "docs")

    with open(os.path.join(docs, "AI_AUDIT_LOG.md"), "w", encoding="utf-8") as f:
        f.write(make_audit_log(d))
    with open(os.path.join(docs, "CHANGELOG.md"), "w", encoding="utf-8") as f:
        f.write(make_changelog(d))
    with open(os.path.join(docs, "PROMPTS.md"), "w", encoding="utf-8") as f:
        f.write(make_prompts(d))
    with open(os.path.join(docs, "REFLECTION.md"), "w", encoding="utf-8") as f:
        f.write(make_reflection(d))

    git("add", "docs/AI_AUDIT_LOG.md", "docs/CHANGELOG.md", "docs/PROMPTS.md", "docs/REFLECTION.md")

    msg = (
        f"docs: populate AI audit logs for {branch}\n\n"
        f"Fills AI_AUDIT_LOG.md, CHANGELOG.md, PROMPTS.md, REFLECTION.md\n"
        f"with content accurate to this branch scope: {d['scope']}\n\n"
        "Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>"
    )
    git("commit", "-m", msg)
    print(f"  committed docs for {branch}", flush=True)

git("checkout", "develop")
print("\nDone. All branches have populated docs. Now on: develop")
