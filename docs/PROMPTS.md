# AI Prompts Log -- Feature -- Payment Processing

Branch: `feature/payment-processing`
Scope: PayOS online payment, COD, wallet top-up, platform transaction tracking, payment webhook handling

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** PayOS online payment, COD, wallet top-up, platform transaction tracking, payment webhook handling

**Prompt:**
> How do I verify PayOS webhook signatures in ASP.NET Core to prevent spoofed payment callbacks?

**AI Output Summary:**
Compute HMAC-SHA256 of sorted payload fields using PayOS secret key; compare with webhook signature header; reject if mismatch.

**Used in files:** Features/Payments/*, Controllers/Api/CheckoutController.cs (payment endpoints), Infrastructure/External/PayOS*.cs, Infrastructure/External/CodPaymentStrategy.cs

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** PayOS online payment, COD, wallet top-up, platform transaction tracking, payment webhook handling

**Prompt:**
> Design a payment strategy pattern that supports PayOS, COD, and wallet balance -- extensible for future methods.

**AI Output Summary:**
IPaymentStrategy with ProcessAsync(order) and ValidateAsync(order); factory selects strategy by PaymentMethod enum.

**Used in files:** Features/Payments/*, Controllers/Api/CheckoutController.cs (payment endpoints), Infrastructure/External/PayOS*.cs, Infrastructure/External/CodPaymentStrategy.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** PayOS online payment, COD, wallet top-up, platform transaction tracking, payment webhook handling

**Prompt:**
> How should I handle payment timeout -- what if the user closes the browser before paying?

**AI Output Summary:**
PaymentTimeoutJob: Hangfire delayed job scheduled 30 min after payment creation; if status still Pending, cancel order and release stock.

**Used in files:** Features/Payments/*, Controllers/Api/CheckoutController.cs (payment endpoints), Infrastructure/External/PayOS*.cs, Infrastructure/External/CodPaymentStrategy.cs

---
