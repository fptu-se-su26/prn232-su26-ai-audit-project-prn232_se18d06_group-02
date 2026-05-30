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

## [2026-05-30] — User Profile Page
Author: Nguyen Sinh Nhat (DE180430)

### Added
- Created Users API module with orders and addresses CRUD operations
- Built ProfilePage with tabbed interface (My Orders, Addresses)
- Order list with status badges, dates, and sub-order tracking links
- Address management: view list, add new, delete
- Registered route in App.tsx: /profile with ?tab URL parameter support

### AI-assisted
- Used Claude Code to generate the profile page layout and form structure
- URL parameter handling for tab state and address CRUD operations were designed and verified manually
