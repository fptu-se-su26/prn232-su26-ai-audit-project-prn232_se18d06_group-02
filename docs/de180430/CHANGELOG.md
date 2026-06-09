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

## [2026-05-30] — Shopping Cart and Checkout
Author: Nguyen Sinh Nhat (DE180430)

### Added
- Built ProductCard reusable component with product image, price, brand, store, rating
- Created Cart API module (get, add, update quantity, remove)
- Built CartPage with item list, quantity controls, remove, order summary sidebar
- Created Checkout API module (getData, placeOrder, applyVoucher, cancelPayment)
- Built CheckoutPage with delivery address selection, payment method (COD/PayOS), voucher code
- Built PayOSCheckoutPage with QR code display and external payment link
- Built OrderSuccessPage with order confirmation and item list
- Registered routes in App.tsx: /cart, /checkout, /checkout/payos, /checkout/success/:orderId

### Changed
- App.tsx: added imports and routes for cart and checkout pages

### AI-assisted
- Used Claude Code to generate the UI components and Tailwind CSS structure
- API integration decisions, route protection, and payment flow logic were designed and verified manually

---

## [2026-06-09] — Store Profile Page
Author: Nguyen Sinh Nhat (DE180430)

### Added
- Added `StoreProfile` interface to `GearZone-FE/src/types/catalog.ts`
- Added `getStoreProfile()`, `getStoreProducts()`, and `followStore()` to `GearZone-FE/src/api/catalog.ts` following the existing `unwrap(await apiClient.get(...))` pattern
- Built `GearZone-FE/src/pages/StoreProfilePage.tsx` — public page at `/store/:slug` with banner (image or gradient fallback), store logo overlay, verified badge, follow/unfollow toggle, stats row (followers, products, total sold, rating), and a responsive 2–5 column products grid using `ProductCard`

### Changed
- `GearZone-FE/src/App.tsx`: added missing imports for `SiteLayout`, `ProductBrowsePage`, `ProductDetailPage`; removed four duplicate route definitions; registered `/store/:slug` as a public route inside `SiteLayout`

### AI-assisted
- Used Claude Code to generate the Tailwind CSS layout for banner, stats, and products grid sections
- Used Claude Code to draft the three catalog API function signatures
- Page section design, follow toggle logic, and final verification were done manually
