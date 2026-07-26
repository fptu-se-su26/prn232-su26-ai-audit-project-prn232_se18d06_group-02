# Promotion Campaign & Voucher

## Scope

This release adds seller-funded product campaigns and extends seller vouchers to
support both order and shipping discounts. The existing Super Admin platform
voucher module remains compatible.

Campaign discounts are applied automatically to every active variant of a
selected product. Checkout can then apply at most one order voucher and one
shipping voucher. Each voucher slot accepts either a seller voucher or a
platform voucher, never both.

## Clean Architecture placement

- `GearZone.Domain`: campaign/reservation entities, lifecycle enums, and order
  financial snapshots.
- `GearZone.Application`: campaign management, centralized promotion pricing,
  quota/voucher lifecycle, authoritative checkout quote, order snapshots, and
  commission calculations.
- `GearZone.Infrastructure`: EF Core mappings, atomic conditional updates,
  repositories, transaction support, and migration.
- `GearZone.Api`: seller promotion endpoints and authoritative checkout quote.
- `GearZone.Web`: seller campaign management, seller voucher type selection,
  sale-price presentation, checkout quote integration, and order snapshots.

## API contracts

Seller campaign endpoints require the `Store Owner` role:

- `GET /api/seller/promotions`
- `GET /api/seller/promotions/{id}`
- `GET /api/seller/promotions/products`
- `POST /api/seller/promotions`
- `PUT /api/seller/promotions/{id}`
- `PATCH /api/seller/promotions/{id}/toggle-status`

Checkout:

- `POST /api/checkout/quote` reloads cart items, prices, campaign eligibility,
  shipping, and voucher eligibility from the database.
- `POST /api/checkout` accepts `RequestId` and uses the same pricing engine as
  quote.
- Legacy voucher endpoints now require cart item IDs and delegate to the
  authoritative quote flow; client-provided merchandise and shipping totals are
  no longer accepted.

Expected error semantics:

- `400`: malformed input or business validation.
- `404`: resource/store ownership is not available to the caller.
- `409`: overlapping campaign, stale checkout quote, stock/quota race,
  concurrency conflict, or checkout idempotency conflict.

## Reservation lifecycle

1. Quote only calculates current eligibility; it does not reserve stock or
   discount capacity.
2. Place order runs inside one database transaction and conditionally reserves
   stock, campaign quantity, and voucher usage before writing order snapshots.
3. PayOS confirmation redeems all reservations atomically.
4. A seller approving COD redeems reservations for that store.
5. Cancellation, timeout, rejection, payment-link compensation, and future
   refund flows use the same idempotent release services.

`Voucher.UsedCount` includes reserved and redeemed usages. Releasing a usage
decrements it exactly once.

## Financial rules

- `OrderItem.UnitPriceSnapshot` is the effective campaign price.
- `OrderItem.OriginalUnitPriceSnapshot` and promotion fields preserve the sale
  explanation shown to the buyer.
- `SubOrder.CommissionableAmount` is merchandise after campaign and seller
  order voucher.
- A platform voucher does not reduce seller commissionable amount or payout.
- Shipping discounts do not participate in commission.

## Database rollout

Migration:

`20260726174949_AddPromotionCampaignsAndCheckoutPricing`

Before production rollout, back up the database and apply the migration first
to a staging copy:

```powershell
dotnet ef database update `
  --project GearZone.Infrastructure `
  --startup-project GearZone.Web `
  --context ApplicationDbContext
```

The migration backfills historical rows without recalculating existing
financial results:

- original item price = existing `UnitPriceSnapshot`;
- historical promotion and seller voucher discounts = 0;
- historical commissionable amount = existing sub-order subtotal;
- historical net shipping fee = existing shipping fee;
- historical voucher usages = `Redeemed`.

Generate and review rollback SQL before using it:

```powershell
dotnet ef migrations script `
  20260726174949_AddPromotionCampaignsAndCheckoutPricing `
  20260726110717_AddAiChatAndKnowledge `
  --project GearZone.Infrastructure `
  --startup-project GearZone.Web `
  --context ApplicationDbContext
```

Rollback removes campaign tables and the new financial snapshot columns, so
export or back up new promotion data before rolling back.

## Verification

- `dotnet build GearZone.sln --no-restore`: passed.
- `dotnet test GearZone.Tests/GearZone.Tests.csproj --no-restore`: 64/64
  passed.
- Forward and rollback migration SQL generation: passed.
- Staging database migration and browser acceptance remain deployment
  checkpoints because they require the target database, PayOS sandbox, and
  seller/buyer test accounts.
