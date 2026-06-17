# CHANGELOG.md

## [2026-06-07]
Author: DE180417

### Added
- Built the new React Admin Category Management page in `GearZone-FE/src/pages/AdminCategoriesPage.tsx`
- Added category hierarchy listing with root/sub-category rows, expand and collapse behavior, status filtering, search, soft-delete confirmation, and summary counts
- Added category create and edit workflows with parent category selection, active/inactive visibility control, automatic slug generation, and category path/meta preview
- Added category attribute management with attribute rows, filterable toggles, input type selection, option chips, and drag-to-reorder behavior for attributes and options
- Built the new React Admin Brand Management page in `GearZone-FE/src/pages/AdminBrandsPage.tsx`
- Added brand statistics, search, approval status filtering, paginated brand table, brand logo display, create/edit modal workflows, approve action, and delete confirmation
- Added brand logo support for both uploaded image files and external image URLs
- Built the new React Admin Voucher Management page in `GearZone-FE/src/pages/AdminVouchersPage.tsx`
- Added voucher KPI widgets, status tabs, search, scope/type/sort filters, advanced filters, ticket-style voucher rows, pagination, duplicate support, and status toggle confirmation
- Added voucher create and edit workflows with code generation, voucher type selection, category restriction, discount logic, usage lifecycle controls, date validation, visibility toggle, and real-time voucher preview
- Registered protected React routes for `/admin/categories`, `/admin/categories/create`, `/admin/categories/:id/edit`, `/admin/brands`, `/admin/vouchers`, `/admin/vouchers/create`, and `/admin/vouchers/edit/:id`
- Added typed Category, Brand, and Voucher API support in `GearZone-FE/src/api/admin.ts`

### Changed
- Expanded the shared admin API client so catalog and marketing modules use typed request and response contracts
- Updated `GearZone.Web/Controllers/Api/Admin/CategoriesController.cs` with category query support, create response data, and category attribute persistence support
- Updated `GearZone.Web/Controllers/Api/Admin/BrandsController.cs` so brand create and update actions accept multipart form data for logo uploads
- Updated `GearZone.Web/Controllers/Api/Admin/VouchersController.cs` so voucher list responses include summary KPI data for the React voucher dashboard
- Kept Category, Brand, and Voucher pages inside the existing admin shell and route protection model

### Fixed
- Fixed frontend TypeScript issues found while building the new catalog and marketing admin modules
- Avoided backend build output locking during verification by compiling the web project to a temporary output folder
- Added frontend validation for voucher discount rules and voucher date ranges before save requests

### Verification
- Ran `npm run build` in `GearZone-FE`
- Ran `dotnet build GearZone.Web\GearZone.Web.csproj -o %TEMP%\gearzone-web-build-check /p:UseAppHost=false`
- Confirmed the frontend development server was available at `http://localhost:5173`

### AI-assisted
- Used Codex to help design, implement, review, and verify the new React admin modules for category management, brand management, and voucher management
- Final route behavior, API contracts, form validation, mutation actions, and build verification were reviewed manually by the author

## [2026-06-06]
Author: DE180417

### Added
- Built the new React Admin Store Management page in `GearZone-FE/src/pages/AdminStoresPage.tsx`
- Added typed Store Management API support in `GearZone-FE/src/api/admin.ts` for active store listing, store stats, store detail, and store status updates
- Added backend Store Management API coverage in `GearZone.Web/Controllers/Api/Admin/StoreManagementController.cs`, including active-store filtering and `GET /api/admin/stores/stats`
- Registered protected React routes for `/admin/stores` and `/admin/stores/:id`
- Built the new React Admin User Management page in `GearZone-FE/src/pages/AdminUsersPage.tsx`
- Added typed User Management API support for user listing, role filtering, create, update, soft delete, and restore actions
- Registered the protected React route `/admin/users`
- Built the new React Admin Order Management list page in `GearZone-FE/src/pages/AdminOrdersPage.tsx`
- Built the new React Admin Order Detail page in `GearZone-FE/src/pages/AdminOrderDetailPage.tsx`
- Added typed Order Management API support for order list, payment/status filtering, sorting, pagination, and order detail loading
- Registered protected React routes for `/admin/orders`, `/admin/orders/detail`, and `/admin/orders/:id`
- Built the new React Admin Product Management list page in `GearZone-FE/src/pages/AdminProductsPage.tsx`
- Built the new React Admin Product Detail page in `GearZone-FE/src/pages/AdminProductDetailPage.tsx`
- Added typed Product Management API support for list, metadata, detail, approve, reject, suspend, delete, and bulk status actions
- Registered protected React routes for `/admin/products` and `/admin/products/:id`
- Added Product Management summary cards, search, status and brand quick filters, advanced filters, table sorting, pagination, row actions, and bulk actions
- Added Product Detail gallery, product information, technical specifications, variants table, commercial insights, description, store profile summary, and sticky admin action bar

### Changed
- Expanded `GearZone-FE/src/api/admin.ts` so admin dashboard, stores, users, orders, and products share a typed API access layer
- Updated admin routing in `GearZone-FE/src/App.tsx` so each management area has protected React routes
- Kept admin navigation aligned with the existing sidebar labels for Stores, Users, Orders, and Product
- Kept export controls as visual placeholders where no export API was included in the feature scope
- Used route-level detail pages for orders and products so admin users can move from list rows into focused review screens
- Kept product category attribute filtering out of the React screen until a dedicated API endpoint is available

### Fixed
- Recreated the local project database by applying the available Entity Framework database updates with `dotnet ef database update`
- Fixed frontend TypeScript issues found while building the newly added admin management pages
- Avoided stale table selections by clearing selected product rows after product list reloads and bulk actions
- Kept state-changing product actions behind confirmation or reason modals before calling admin APIs

### Verification
- Ran `dotnet ef database update --project GearZone.Infrastructure\GearZone.Infrastructure.csproj --startup-project GearZone.Web\GearZone.Web.csproj`
- Ran `dotnet build GearZone.sln`
- Ran `npm run build` in `GearZone-FE`
- Confirmed the backend development server was available at `http://localhost:5107`
- Confirmed the frontend development server was available at `http://localhost:5173`

### AI-assisted
- Used Codex to help design, implement, review, and verify the new React admin management pages for stores, users, orders, and products
- Final route behavior, API contracts, modal actions, filtering behavior, and build verification were reviewed manually by the author

## [2026-05-29]
Author: DE180417

### Added
- Built a new React admin layout in `GearZone-FE/src/components/admin/AdminLayout.tsx`
- Built the new Admin Platform Overview page in `GearZone-FE/src/pages/AdminDashboardPage.tsx`
- Added typed admin dashboard API support in `GearZone-FE/src/api/admin.ts`
- Added KPI cards, revenue overview chart, revenue distribution summary, order status breakdown, top stores table, user growth chart, and dispute summary sections
- Built the new Store Applications list page in `GearZone-FE/src/pages/AdminStoreApplicationsPage.tsx`
- Built the new Store Application detail page in `GearZone-FE/src/pages/AdminStoreApplicationDetailPage.tsx`
- Added Store Applications typed API support for list, stats, detail, approve, reject, and request-info actions
- Added backend stats support through `GET /api/admin/store-applications/stats`
- Registered protected React routes for `/admin/dashboard`, `/admin/store-applications`, and `/admin/store-applications/:id`
- Added Store Applications summary cards for total, pending, approved, and rejected applications
- Added backend-compatible Store Applications filters for search term, status, date range, page number, and page size
- Added Store Applications table columns for company, tax code, business type, representative, phone, submission date, status, and detail action
- Added Store Application detail sections for basic information, owner and representative information, banking setup, metadata, application history, and document preview
- Added pending-application action modals for approve, reject, and request information
- Added rejection reason validation with a 500-character limit and quick reason chips
- Added request-info validation and post-action reload behavior

### Changed
- Updated admin routing so the dashboard and store-application screens open as React admin pages
- Updated the admin sidebar so Store Applications navigates to the new React screen
- Kept admin route protection aligned with the existing role-based access model
- Updated current-user role retrieval so logged-in admin users can pass the frontend role guard correctly
- Kept export PDF, Excel, and CSV buttons as visual placeholders because no export API was part of this feature scope
- Mapped StoreStatus numeric enum values to readable frontend labels and status badge styles
- Reloaded Store Application detail data after approve, reject, or request-info actions so the UI reflects the latest review state
- Kept review action controls visible only when the application status is pending

### Fixed
- Fixed the issue where logged-in admin users were redirected to the home page because the current-user API returned `role: null`
- Fixed React/TypeScript validation issues found during lint and production build checks
- Avoided backend build output locking by verifying the web project with a temporary output folder when the running web process locked DLL files
- Prevented invalid reject and request-info submissions by validating required modal fields before sending API requests
- Prevented Store Applications pagination from navigating beyond the first or last available page

### Verification
- Ran `npm.cmd run lint`
- Ran `npm.cmd run build`
- Ran `dotnet build GearZone.Application\GearZone.Application.csproj --no-restore`
- Ran `dotnet build GearZone.Web\GearZone.Web.csproj --no-restore -o .verify-build\GearZone.Web`

### AI-assisted
- Used Codex to help plan, implement, review, and verify the new React admin dashboard and store-application management features
- Final feature scope, affected files, route behavior, and verification commands were reviewed manually by the author
