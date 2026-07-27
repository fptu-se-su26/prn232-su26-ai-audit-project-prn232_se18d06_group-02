# AI_AUDIT_LOG.md

## Log #01
- Date: 2026-06-15
- Author: DE180494
- AI Tool: Codex
- Purpose: Implementation support for a new React customer profile account center and related customer self-service workflows in `GearZone-FE`.
- Prompt Reference: PROMPTS.md#prompt-01
- AI Output Summary: Helped implement a React account center with account editing, order history, buyer messages, address management, review history, password updates, seller application status, and seller registration navigation. Also helped add missing backend endpoints for profile update and password change, adjust seller registration file upload handling, add frontend API helpers, register routes, and verify the project build.
- Human Decision: I reviewed the feature direction, kept the work focused on the current GearZone API + React architecture, and selected the final scope for customer self-service features.
- Applied To: `GearZone.Web/Controllers/Api/UsersController.cs`, `GearZone.Web/Controllers/Api/SellerRegistrationController.cs`, `GearZone-FE/src/pages/ProfilePage.tsx`, `GearZone-FE/src/pages/RegisterSellerPage.tsx`, `GearZone-FE/src/api/users.ts`, `GearZone-FE/src/api/seller.ts`, `GearZone-FE/src/App.tsx`, `GearZone-FE/src/components/layout/SiteLayout.tsx`
- Verification: Ran `dotnet build GearZone.sln` successfully. Ran `npm install` in `GearZone-FE` to restore missing frontend dependencies, then ran `npm run build` successfully. The frontend build completed with Vite/Rolldown warnings from SignalR annotation handling and chunk size, but no build errors.

## Usage Note
AI was used as an implementation assistant for building a new customer-facing React profile feature in the current GearZone system. The work focused on API integration, route setup, account center UI, profile mutations, address management, review/order workflows, seller registration, and local build verification. Final review, selected scope, and responsibility remained with the author.

## Log #02
- Date: 2026-07-27
- Author: DE180494
- AI Tool: Codex
- Purpose: Implementation support for realtime seller notification when a customer places an order.
- Prompt Reference: PROMPTS.md#prompt-02
- AI Output Summary: Helped extend the order tracking SignalR hub with a seller order channel, emit `SellerOrderCreated` notifications after checkout succeeds, connect the Store Owner layout to the hub, show toast notifications for new orders, refresh the seller order list when it is open, and harden toast message rendering.
- Human Decision: I kept the feature scoped to new-order seller awareness and reused the existing `OrderTrackingHub`/`IOrderTrackingNotifier` infrastructure instead of introducing a separate notification subsystem.
- Applied To: `GearZone.Application/Abstractions/External/IOrderTrackingNotifier.cs`, `GearZone.Application/Features/Checkout/CheckoutService.cs`, `GearZone.Application/Features/Orders/OrderService.cs`, `GearZone.Web/Hubs/OrderTrackingHub.cs`, `GearZone.Web/Hubs/SignalROrderTrackingNotifier.cs`, `GearZone.Web/Pages/Shared/_StoreOwnerLayout.cshtml`, `GearZone.Web/wwwroot/js/toast.js`
- Verification: Ran `dotnet test GearZone.sln` successfully. Result: 65 passed, 0 failed, 0 skipped.
