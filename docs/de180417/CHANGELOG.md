# CHANGELOG.md

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
