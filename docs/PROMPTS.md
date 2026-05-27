# AI Prompts Log -- Feature -- Order Management

Branch: `feature/order-management`
Scope: Order lifecycle: status transitions, history tracking, buyer and seller order views, auto-complete background job

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Order lifecycle: status transitions, history tracking, buyer and seller order views, auto-complete background job

**Prompt:**
> Design an order state machine in C# that enforces valid status transitions and records history for each change.

**AI Output Summary:**
Dictionary<OrderStatus, IEnumerable<OrderStatus>> allowed transition map; throw DomainException on invalid transition; record OrderStatusHistory in same UoW.

**Used in files:** Features/Orders/*, Controllers/Api/OrdersController.cs, Controllers/Api/Seller/OrdersController.cs, Pages/Public/User/Orders/*, Pages/StoreOwner/Orders/*, Infrastructure/Jobs/OrderAutoCompleteJob.cs

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Order lifecycle: status transitions, history tracking, buyer and seller order views, auto-complete background job

**Prompt:**
> Implement a Hangfire background job that auto-completes orders a configurable number of days after delivery.

**AI Output Summary:**
Hangfire RecurringJob running daily; query SubOrders where Status=Delivered AND DeliveredAt < UtcNow - AutoCompleteDays.

**Used in files:** Features/Orders/*, Controllers/Api/OrdersController.cs, Controllers/Api/Seller/OrdersController.cs, Pages/Public/User/Orders/*, Pages/StoreOwner/Orders/*, Infrastructure/Jobs/OrderAutoCompleteJob.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Order lifecycle: status transitions, history tracking, buyer and seller order views, auto-complete background job

**Prompt:**
> How do I notify a buyer in real-time when their order status changes using SignalR?

**AI Output Summary:**
Hub.Clients.User(userId).SendAsync('OrderStatusChanged', orderId, newStatus) using user-specific SignalR connection.

**Used in files:** Features/Orders/*, Controllers/Api/OrdersController.cs, Controllers/Api/Seller/OrdersController.cs, Pages/Public/User/Orders/*, Pages/StoreOwner/Orders/*, Infrastructure/Jobs/OrderAutoCompleteJob.cs

---
