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
