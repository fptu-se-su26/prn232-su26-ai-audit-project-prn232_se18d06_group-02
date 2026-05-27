# AI Prompts Log -- Feature -- Voucher and Discount System

Branch: `feature/voucher-system`
Scope: Seller-created vouchers: percentage/fixed discount, usage limits, expiry, checkout application, admin moderation

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Seller-created vouchers: percentage/fixed discount, usage limits, expiry, checkout application, admin moderation

**Prompt:**
> Design a voucher system for a multi-vendor marketplace where each seller creates their own vouchers applied at checkout.

**AI Output Summary:**
Voucher.StoreId links voucher to a specific seller; applied only to SubOrders from that store; discount calculated per-SubOrder.

**Used in files:** Features/ (vouchers), Controllers/Api/Seller/VouchersController.cs, Pages/StoreOwner/Vouchers/*, Pages/Admin/Vouchers/*

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Seller-created vouchers: percentage/fixed discount, usage limits, expiry, checkout application, admin moderation

**Prompt:**
> How do I prevent concurrent race conditions when multiple users try to use the last available slot of a limited voucher?

**AI Output Summary:**
Optimistic concurrency: check VoucherUsage count in transaction; if Count >= MaxUsage throw ConcurrencyException and notify user.

**Used in files:** Features/ (vouchers), Controllers/Api/Seller/VouchersController.cs, Pages/StoreOwner/Vouchers/*, Pages/Admin/Vouchers/*

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Seller-created vouchers: percentage/fixed discount, usage limits, expiry, checkout application, admin moderation

**Prompt:**
> What validation rules should a checkout voucher system enforce?

**AI Output Summary:**
Validation: IsActive, not expired, UsageCount < MaxUsage, OrderTotal >= MinOrderAmount, user has not used this voucher before.

**Used in files:** Features/ (vouchers), Controllers/Api/Seller/VouchersController.cs, Pages/StoreOwner/Vouchers/*, Pages/Admin/Vouchers/*

---
