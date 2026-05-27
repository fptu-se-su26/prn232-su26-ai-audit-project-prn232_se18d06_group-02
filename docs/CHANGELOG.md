# Changelog -- Core -- Domain Entities

All notable changes on branch `core/domain-entities` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
GearZone.Domain: Entity<TKey> base class, all aggregate roots and value objects, domain enums

### Added
- Entity<TKey> generic base class with Id property
- 25+ domain entities: ApplicationUser, Product, ProductVariant, ProductImage, Order, SubOrder, OrderItem, OrderStatusHistory, Cart, CartItem, Store, StoreFollow, Brand, Category, CategoryAttribute, Conversation, ChatMessage, Payment, WalletTransaction, Payout, PayoutBatch, PayoutItem, PayoutTransaction, PlatformTransaction, Voucher, VoucherUsage, ProductReview, Shipment, UserAddress, SystemSetting, InventoryTransaction
- Domain enums: OrderStatus, PaymentStatus, PaymentMethod, ProductStatus, StoreStatus, VoucherType, TransactionType, InventoryTransactionType

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
