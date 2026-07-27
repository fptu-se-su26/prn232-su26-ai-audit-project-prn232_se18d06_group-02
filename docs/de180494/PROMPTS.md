# PROMPTS.md

## Prompt #01

- Date: 2026-06-15
- AI Tool: Codex
- Author: DE180494
- Purpose: Build a new React customer profile account center for the current GearZone API + React project.

### Prompt
Please implement a complete customer profile account center in the current GearZone project. The project uses ASP.NET Core APIs and a React frontend in `GearZone-FE`. The account center should support account information, customer orders, buyer messages, shipping addresses, product reviews, password changes, seller application status, and seller registration navigation. Use the existing backend APIs where available, add small missing API endpoints only when required, wire the necessary frontend routes, and keep the UI consistent with the current GearZone customer experience.

### Follow-up Prompt
Implement the plan.

### Expected Output
- A complete React profile page available at `/profile`.
- Working tabs for account information, orders, messages, addresses, reviews, and password changes.
- Order filtering, search, tracking links, and review action links.
- Address CRUD and set-default behavior aligned with backend DTOs.
- Review history with seller replies and edit-review navigation.
- Seller application status display and seller registration workflow.
- Backend endpoints for updating profile information and changing password.
- React routes for profile, order tracking, review writing, and seller registration.
- Successful backend and frontend build verification.

### Evaluation
The prompt was useful because it described a complete customer self-service workflow instead of a small UI-only task. The generated result still required manual review to confirm API response shapes, DTO field names, enum handling, file upload binding, route protection, and build output.

## Prompt #02

- Date: 2026-07-27
- AI Tool: Codex
- Author: DE180494
- Purpose: Add realtime seller notification when a customer places an order.

### Prompt
Checkout to a new branch with code DE180494, then add a feature so when a customer places an order it notifies the seller in realtime and records the log in the DE180494 docs folder.

### Expected Output
- A new `feature/de180494-*` branch for the work.
- A realtime SignalR notification to the relevant seller when checkout creates an order/sub-order.
- Store Owner UI feedback without requiring a manual refresh.
- Updated `docs/de180494` changelog, prompt log, and AI audit log.
- Backend build/test verification.

### Evaluation
The prompt was useful because it specified both the behavior and the documentation destination. The implementation reused the existing order tracking SignalR infrastructure and kept the feature focused on seller awareness for newly placed orders.
