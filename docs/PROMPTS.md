# AI Prompts Log -- Feature -- Shopping Cart and Checkout

Branch: `feature/cart-and-checkout`
Scope: Persistent global cart, multi-seller checkout, voucher application, address selection, order placement

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Persistent global cart, multi-seller checkout, voucher application, address selection, order placement

**Prompt:**
> Design a shopping cart that persists across sessions and devices for a multi-seller e-commerce platform.

**AI Output Summary:**
Cart stored in database as Cart entity with CartItems; no session dependency; synced on login for guest-to-user merge.

**Used in files:** Features/Cart/*, Features/Checkout/*, Controllers/Api/CartController.cs, Controllers/Api/CheckoutController.cs, Pages/Cart/*, Pages/Checkout/*

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Persistent global cart, multi-seller checkout, voucher application, address selection, order placement

**Prompt:**
> How do I implement an atomic checkout that creates an Order with multiple SubOrders (one per seller) inside a single transaction?

**AI Output Summary:**
UnitOfWork.SaveChangesAsync wraps Order + all SubOrders + OrderItems creation; PaymentRecord created in same transaction.

**Used in files:** Features/Cart/*, Features/Checkout/*, Controllers/Api/CartController.cs, Controllers/Api/CheckoutController.cs, Pages/Cart/*, Pages/Checkout/*

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Persistent global cart, multi-seller checkout, voucher application, address selection, order placement

**Prompt:**
> Implement voucher discount application logic with minimum order amount, per-user usage limits, and expiry date checks.

**AI Output Summary:**
VoucherService.ValidateAsync checks: IsActive, not expired, UsageCount < MaxUsage, TotalAmount >= MinOrderAmount, not already used by this user.

**Used in files:** Features/Cart/*, Features/Checkout/*, Controllers/Api/CartController.cs, Controllers/Api/CheckoutController.cs, Pages/Cart/*, Pages/Checkout/*

---
