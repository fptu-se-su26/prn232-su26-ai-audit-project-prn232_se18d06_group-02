# CHANGELOG.md

## [2026-05-28]
Author: Nguyen Sinh Nhat (DE180430)

### Added
- Built a React-based login and registration page in `GearZone-FE`
- Added reusable auth UI components for layout, form sections, input fields, hero panels, and the social sign-in button
- Added frontend auth helpers for login, registration, logout, current-user retrieval, email verification, and verification resend

### Changed
- Configured frontend routing so the authentication flow works correctly after sign-in
- Adjusted auth page behavior for login, registration, and Google sign-in handling
- Updated the Vite path alias configuration for cleaner imports in the frontend codebase

### Fixed
- Fixed frontend import resolution issues related to `@/` path aliases
- Fixed post-login routing so the page no longer loops back incorrectly
- Fixed smaller interaction issues in the auth screen flow

### AI-assisted
- Used Claude Code in a limited way to review the implemented frontend and highlight a few issues for manual verification
- Final code changes and validation were performed manually by the author

---

## [2026-05-30] — Order Tracking and Review
Author: Nguyen Sinh Nhat (DE180430)

### Added
- Created Orders API module with payment status and tracking endpoints
- Created Reviews API module with get editor and submit operations
- Built OrderTrackPage with order summary card, status badge, vertical tracking timeline
- Built WriteReviewPage with interactive star rating (1-5), comment textarea with character counter
- Registered routes in App.tsx: /orders/track/:subOrderId, /write-review/:orderItemId

### AI-assisted
- Used Claude Code to generate the order tracking timeline CSS and star rating widget
- API integration and route protection were verified manually
