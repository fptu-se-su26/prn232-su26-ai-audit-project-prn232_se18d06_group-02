# CHANGELOG.md

## [2026-06-15] - Customer Profile Account Center
Author: DE180494

### Added
- Built a new React customer profile account center in `GearZone-FE`.
- Added profile tabs for account information, order history, buyer messages, shipping addresses, review history, and password updates.
- Added order search and status filtering with summary counters, order item display, tracking links, and review action links.
- Added address management with create, edit, delete, set-default, address type selection, coordinates, and Goong autocomplete integration.
- Added review history with product links, ratings, seller replies, delivered date, review deadline, and edit-review navigation.
- Added React seller registration page at `/seller/register` with store information, identity verification, banking details, and final submission.
- Added seller registration API helper in `GearZone-FE/src/api/seller.ts`.
- Added frontend routes for `/profile`, `/orders/track/:subOrderId`, `/reviews/write/:orderItemId`, and `/seller/register`.

### Changed
- Updated the authenticated user menu in `SiteLayout` to include a direct `My Profile` entry.
- Extended `usersApi` with profile update, password change, and corrected user-order query parameter support.
- Updated seller registration step 2 API binding to accept multipart form data for identity-card image upload.
- Improved customer profile handling for backend enum values that may be returned as either strings or numeric enum values.

### Fixed
- Fixed incomplete React profile behavior by connecting the page to the current user, order, review, address, chat, store-status, and map APIs.
- Fixed user-order response handling by using the current `{ summary, orders }` API shape.
- Fixed address DTO alignment by using `phoneNumber`, `addressType`, latitude, and longitude fields consistently.
- Fixed missing profile-related routes that prevented account center, order tracking, review writing, and seller registration workflows from being reached from the React app.
- Added missing backend profile/password mutation endpoints in `UsersController`.

### AI-assisted
- Used Codex to help design and implement the new customer profile account center, API integration, route wiring, and seller registration flow.
- Reviewed the generated changes manually and kept the implementation aligned with the current GearZone API + React architecture.
- Verified the implementation with backend and frontend build commands.
