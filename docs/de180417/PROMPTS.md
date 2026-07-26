# PROMPTS.md

## Prompt #01

- Date: 2026-05-29
- AI Tool: Codex
- Author: DE180417
- Purpose: Build a new React Admin Platform Overview dashboard for GearZone

### Prompt
Please implement a new React-based Admin Platform Overview dashboard for `GearZone-FE`. The feature should provide a full admin workspace with a reusable sidebar/header layout, protected routing, dashboard data loading, period filtering, KPI cards, revenue visualization, revenue distribution summary, order status breakdown, top stores table, user growth visualization, and dispute summary sections.

Follow the existing React, TypeScript, Vite, Tailwind, API client, and authentication patterns already used in the project. Use the existing backend dashboard API as the data source and keep the implementation focused on the admin dashboard feature. Export buttons should be rendered as UI placeholders unless an export API is available. The page should be accessible at `/admin/dashboard` for authorized admin users.

### Expected Output
- A reusable React admin layout with sidebar, header, breadcrumb, and profile area
- A typed admin dashboard API client
- A protected `/admin/dashboard` route
- Dashboard overview UI with loading, error, empty, and data states
- Native SVG or CSS visualizations without introducing unnecessary chart dependencies
- Successful frontend validation with lint and production build commands

### Evaluation
This prompt defines the feature as a new admin dashboard implementation and gives clear boundaries around routing, data loading, UI behavior, and verification. It is specific enough to keep the work focused while still allowing the implementation to follow the existing project conventions.

## Prompt #02

- Date: 2026-05-29
- AI Tool: Codex
- Author: DE180417
- Purpose: Build new React Store Applications management screens for the admin area

### Prompt
Please implement new React admin screens for managing store applications in `GearZone-FE`. The feature should include a Store Applications list page and a Store Application detail page. The list page should provide application statistics, search, status filtering, date filtering, pagination, table rows, empty states, and a CSV export placeholder. The detail page should show application status, store information, owner information, banking details, metadata, application history, document previews, and action controls for pending applications.

Use the existing backend Store Applications API for list, detail, approve, reject, and request-info actions. Add a small missing backend stats endpoint if needed so the frontend can display the summary cards. Keep the route structure under `/admin/store-applications` and `/admin/store-applications/:id`, reuse the React admin layout, and keep role protection consistent with the admin area.

### Expected Output
- Typed Store Applications API support in the frontend admin API module
- `GET /api/admin/store-applications/stats` support in the backend API
- A protected Store Applications list route
- A protected Store Application detail route
- Approve, reject, and request-info action modals for pending applications
- Validation for rejection reason and request-info note fields
- Successful frontend and backend build verification

### Evaluation
This prompt describes Store Applications as a new admin management feature and separates list, detail, action, routing, and verification requirements. It also identifies the only backend API gap needed by the frontend instead of expanding the scope into unrelated admin modules.

## Prompt #03

- Date: 2026-05-29
- AI Tool: Codex
- Author: DE180417
- Purpose: Debug admin route redirection after login

### Prompt
Please investigate why an authenticated admin user is redirected back to the home page when opening protected admin React routes. Check the frontend auth guard, the current-user API response, and the backend auth service that returns the logged-in user. Keep the fix focused on the admin route access issue and avoid unrelated authentication changes.

### Expected Output
- Identify whether the frontend role guard is receiving the expected role value
- Identify whether `/api/auth/me` returns the admin role correctly
- Apply the smallest safe backend or frontend fix needed for the protected admin routes
- Verify the fix with relevant build commands

### Evaluation
This prompt keeps the debugging task narrow: it focuses on the route guard, the current-user response, and the admin redirect behavior. The resulting fix was easier to review because it only changed role hydration in the current-user flow.

## Prompt #04

- Date: 2026-05-29
- AI Tool: Codex
- Author: DE180417
- Purpose: Complete the Admin Store Applications review workflow with list, detail, and action states

### Prompt
Please complete the new Admin Store Applications review workflow in `GearZone-FE`. The list screen should load application statistics, support backend-compatible query parameters, display a searchable and filterable table, handle pagination, and provide a clear empty state. The detail screen should present all important application information for admin review, including store profile data, owner identity data, banking setup, metadata, history, and document previews.

For pending applications, add a sticky review action area with approve, reject, and request-information actions. The reject modal must support quick reason chips, require a reason, and enforce a 500-character limit. The request-information modal must require a note. After each successful action, refresh the detail page so the status and available actions are updated. Keep the implementation consistent with the existing React admin layout and API response envelope.

### Expected Output
- Store Applications stats cards and table data loaded from the admin API
- Search, status, date range, and pagination state sent with backend-compatible query keys
- Numeric StoreStatus handling with readable labels and consistent badge styling
- Detail cards for store, owner, banking, metadata, history, and documents
- Pending-only approve, reject, and request-info controls
- Modal validation before API calls
- Detail refresh after successful review actions

### Evaluation
This prompt gives more detail about the actual admin review workflow, not only the page shell. It makes the expected behavior of list filters, detail sections, status handling, and action validation explicit, which helps keep the final feature testable and aligned with admin user needs.

## Prompt #05

- Date: 2026-06-06
- AI Tool: Codex
- Author: DE180417
- Purpose: Restore the local database and confirm the admin dashboard entry route

### Prompt
Please restore the local GearZone project database by applying the available Entity Framework database updates from the infrastructure project with the web project as the startup project. After the database is available, confirm the route that opens the admin dashboard in the React frontend.

### Expected Output
- Run the correct `dotnet ef database update` command for the current solution structure
- Confirm that the admin dashboard can be opened through `/admin/dashboard`
- Keep the task limited to local project setup and route confirmation

### Evaluation
This prompt separates environment recovery from feature development. It makes the database restore command explicit and confirms the admin entry path without changing unrelated application code.

## Prompt #06

- Date: 2026-06-06
- AI Tool: Codex
- Author: DE180417
- Purpose: Build new React Store Management and User Management screens for the admin area

### Prompt
Please implement new React admin screens for Store Management and User Management in `GearZone-FE`. The Store Management screen should show approved active stores with summary cards, search, status/date filters, pagination, store profile navigation, and status action support. The User Management screen should provide user statistics, search, role filtering, active/inactive filtering, pagination, create and edit forms, soft delete, and restore behavior.

Use the existing admin API style and add any small backend endpoint support needed for store statistics or active-store listing. Keep both screens inside the protected admin route structure, reuse the admin layout, and keep the implementation focused on the admin store and user workflows.

### Expected Output
- `GearZone-FE/src/pages/AdminStoresPage.tsx`
- `GearZone-FE/src/pages/AdminUsersPage.tsx`
- Typed store and user API support in `GearZone-FE/src/api/admin.ts`
- Backend store management support for active store listing and store statistics
- Protected routes for `/admin/stores`, `/admin/stores/:id`, and `/admin/users`
- Successful frontend and backend build verification

### Evaluation
This prompt treats Store Management and User Management as new admin modules. It defines the list, filter, action, routing, and API expectations clearly while keeping backend changes limited to the data required by the React screens.

## Prompt #07

- Date: 2026-06-06
- AI Tool: Codex
- Author: DE180417
- Purpose: Build new React Order Management screens for the admin area

### Prompt
Please implement new React admin screens for Order Management in `GearZone-FE`. The feature should include an order list page and an order detail page. The list page should show order statistics, search, payment status filtering, date and total filters, sorting, pagination, empty states, and row navigation. The detail page should show general order information, sub-orders grouped by store, product line items, financial summary, payment information, shipping details, and order history.

Use the existing admin order API endpoints, keep routes protected for admin users, and keep the implementation consistent with the existing React admin layout and typed API client.

### Expected Output
- `GearZone-FE/src/pages/AdminOrdersPage.tsx`
- `GearZone-FE/src/pages/AdminOrderDetailPage.tsx`
- Typed order list and detail API support in `GearZone-FE/src/api/admin.ts`
- Protected routes for `/admin/orders`, `/admin/orders/detail`, and `/admin/orders/:id`
- Search, filter, sort, loading, error, empty, and pagination states on the list page
- Detailed order sections for store shipments, payment status, logistics, and status history
- Successful frontend build verification

### Evaluation
This prompt defines Order Management as a complete new admin workflow with separate list and detail responsibilities. It also keeps the implementation testable by naming the required data states and route behavior.

## Prompt #08

- Date: 2026-06-06
- AI Tool: Codex
- Author: DE180417
- Purpose: Build new React Product Management screens for the admin area

### Prompt
Please implement new React admin screens for Product Management in `GearZone-FE`. The feature should include a product list page and a product detail page. The list page should provide product statistics, search by product name, SKU, or store, status and brand quick filters, advanced filters for category, store, price, created date, and out-of-stock state, table sorting, pagination, row actions, and bulk actions. The detail page should show product gallery, category/status badges, brand, SKU, store link, technical specifications, variants, commercial insights, description, store summary, and a sticky admin action bar.

Use the existing admin product API endpoints for list, metadata, detail, approve, reject, suspend, delete, and bulk status actions. Keep route protection consistent with the admin area and keep unsupported export or edit-spec actions as UI placeholders until matching APIs exist.

### Expected Output
- `GearZone-FE/src/pages/AdminProductsPage.tsx`
- `GearZone-FE/src/pages/AdminProductDetailPage.tsx`
- Typed product API support in `GearZone-FE/src/api/admin.ts`
- Protected routes for `/admin/products` and `/admin/products/:id`
- Product list filters, sorting, pagination, selection, and action modal behavior
- Product detail gallery, specifications, variants, store information, and action bar
- Successful frontend build verification

### Evaluation
This prompt describes Product Management as a new React admin module and defines both the operational list workflow and the review-focused detail workflow. It also explicitly limits the scope where APIs are not available, which keeps the final implementation aligned with the backend contract.

## Prompt #09

- Date: 2026-06-07
- AI Tool: Codex
- Author: DE180417
- Purpose: Build new React Category, Brand, and Voucher Management modules for the admin area

### Prompt
Please implement new React admin screens for Category Management, Brand Management, and Voucher Management in `GearZone-FE`. The Category Management module should support hierarchical category listing, search, status filtering, create and edit forms, soft delete, parent category assignment, visibility control, slug generation, and category attribute/option editing. The Brand Management module should support brand statistics, search, approval filtering, pagination, create and edit modal forms, brand logo upload or URL input, approve actions, and delete confirmation. The Voucher Management module should support voucher KPI cards, status tabs, search, advanced filters, sorting, ticket-style voucher cards, pagination, create/edit workflows, duplicate support, status toggling, discount validation, usage lifecycle controls, and real-time voucher preview.

Use the existing admin layout, route protection, and API response envelope. Add only the backend API support required for the React screens, such as category attribute persistence, brand multipart form handling, and voucher summary data. Keep the implementation focused on the admin catalog and marketing workflows.

### Expected Output
- `GearZone-FE/src/pages/AdminCategoriesPage.tsx`
- `GearZone-FE/src/pages/AdminBrandsPage.tsx`
- `GearZone-FE/src/pages/AdminVouchersPage.tsx`
- Typed Category, Brand, and Voucher API support in `GearZone-FE/src/api/admin.ts`
- Protected routes for `/admin/categories`, `/admin/categories/create`, `/admin/categories/:id/edit`, `/admin/brands`, `/admin/vouchers`, `/admin/vouchers/create`, and `/admin/vouchers/edit/:id`
- Backend API support for category query and attributes, brand form-data logo handling, and voucher summary data
- Search, filtering, pagination, loading, error, empty, confirmation, and validation states for the new admin modules
- Successful frontend and backend build verification

### Evaluation
This prompt defines Category, Brand, and Voucher Management as new React admin modules with clear catalog and marketing responsibilities. It separates list workflows from create/edit workflows, names the required backend support, and keeps the expected UI behavior tied to admin operations rather than unrelated refactoring.

## Prompt #10

- Date: 2026-07-18
- AI Tool: Codex
- Author: Đàm Nguyên Khang (DE180417)
- Purpose: Implement Admin Reports / Business Intelligence v1

### Prompt
Implement the approved Admin Reports / Business Intelligence plan with three Super Admin tabs (Overview, Orders, Sellers), shared Vietnam-time date filters, comparison metrics, zero-filled charts, seller filtering/sorting/paging, CSV/XLSX/PDF exports, and manually generated OpenAI or Gemini insights. Keep report output operational when AI is disabled, do not send PII to AI, cache deterministic reports and insights separately, and add xUnit tests using SQLite in-memory.

### Expected Output
- `/admin/reports` Razor UI and `Reports & BI` sidebar navigation
- Protected report, export, and insight API endpoints
- Paid GMV and operational metrics computed from existing entities
- Structured OpenAI/Gemini provider integrations with evidence-key validation
- Deterministic CSV/XLSX/PDF exports
- Automated period, aggregation, AI cache, export, and API contract tests
- Successful `dotnet test` and solution build

### Evaluation
The prompt was detailed enough to keep formulas, privacy constraints, API behavior, and UI expectations aligned. Manual review remains necessary for production database query plans, provider model availability, QuestPDF licensing before commercial use, and end-to-end browser testing with real authentication and report data.
