# CHANGELOG.md

## [2026-05-29]
Author: Dang Cong Quoc Khanh (DE180880)

### Added
- Built the new customer product browsing page in `GearZone-FE`.
- Added catalog API helpers for categories, filters, product browsing, and product suggestions.
- Added TypeScript types for catalog products, filters, categories, paged results, product suggestions, and compare items.
- Added a shared site layout with header, category navigation, search bar, account actions, and footer.
- Added product grid and list display modes for the browsing page.
- Added filter sidebar for brand, price range, dynamic product attributes, and in-stock status.
- Added header search suggestions connected to the product suggestion API.
- Added category dropdown navigation for parent categories with subcategories.

### Changed
- Updated frontend routing so customer product browsing is available at `/products` and `/products/:slug`.
- Improved product browsing state management with URL query parameters for search, sort, view mode, and filters.
- Adjusted the price range filter so users can control minimum and maximum prices more clearly.
- Refined category dropdown behavior so subcategories appear from the shared header navigation.
- Improved loading behavior for product browsing and pagination states.

### Fixed
- Fixed product browsing build issues caused by unused state and outdated filter-search logic.
- Fixed category dropdown visibility issues in the header navigation.
- Fixed header search behavior so typed product names can show live suggestions.
- Fixed price range slider behavior so both minimum and maximum values can be adjusted.

### AI-assisted
- Used Codex to help implement the new React catalog browsing feature and refine several UI interactions.
- Reviewed and verified the generated changes manually.
- Confirmed the frontend build with `npm run build`.

## [2026-06-08]
Author: Dang Cong Quoc Khanh (DE180880)

### Added
- Built a new React customer product detail page in `GearZone-FE` for route `/product/:slug`.
- Added product detail API typing and data loading for product info, variants, specifications, reviews, and related products.
- Added working `Add to Cart` and `Buy Now` behaviors on the product detail page using the current GearZone cart API.
- Added working `Add to Cart` behavior on the React product browsing page product cards.
- Added live cart count badge behavior in the shared React site header.
- Added in-page success and error feedback for cart actions in both browsing and product detail flows.

### Changed
- Updated the product detail interface so its layout, spacing, sections, and color accents match the current GearZone shopping style more closely.
- Updated the login flow so users can return to the same product page after authentication when cart actions require sign-in.
- Updated the header cart icon to open the real cart page instead of navigating back to the product catalog.
- Updated anchor-tab behavior in the product detail page so `Description`, `Specifications`, and `Reviews` scroll to the correct section.
- Updated backend catalog query binding so the React `brand` filter works correctly with the product browsing API.

### Fixed
- Fixed product detail accent colors that did not match the main GearZone FE orange theme.
- Fixed customer brand filtering by binding query parameter `brand` correctly to backend filter DTO data.
- Fixed missing cart badge display in the React header after successful add-to-cart actions.
- Fixed missing add-to-cart execution on the React browsing page product cards.
- Fixed tab anchor scrolling on the product detail page so sticky layout elements no longer hide the target section.

### AI-assisted
- Used Codex to help implement the new React product detail feature, cart action flows, cart badge updates, brand filter correction, and detail-page interaction fixes.
- Reviewed generated changes manually and aligned the implementation with the current project structure and APIs.
- Confirmed frontend changes with repeated `npm run build` verification and validated behavior in the browser.

## [2026-06-14]
Author: Dang Cong Quoc Khanh (DE180880)

### Added
- Built a new React shopping cart experience in `GearZone-FE` for route `/cart`.
- Added cart display grouped by store, with item checkboxes, store-level selection, order summary, and checkout entry behavior.
- Added optimistic quantity updates so cart totals and counts respond immediately when users increase or decrease item quantity.
- Added a custom remove-item confirmation dialog in the cart page instead of relying on the browser default confirm popup.
- Added a custom draggable dual-thumb price slider interaction for the product browsing filter.

### Changed
- Updated customer cart navigation so header cart actions and shopping feedback links open the React cart route.
- Updated cart interaction behavior so quantity changes no longer trigger a full loading experience after every click.
- Updated the price filter interaction so the minimum and maximum thumbs use a more reliable pointer-based dragging flow.
- Updated the shopping cart summary to stay synchronized with item selection and quantity changes in real time.

### Fixed
- Fixed cart UX lag caused by refetch-style loading after each quantity increment or decrement.
- Fixed product browsing price slider behavior so both minimum and maximum price handles can be dragged independently.
- Fixed price slider hit-area and cursor behavior so dragging feels more predictable for users.
- Fixed cart delete confirmation UX by replacing the native browser popup with a consistent in-app dialog.

### AI-assisted
- Used Codex to help implement the new React cart page, smoother quantity-change behavior, custom cart removal dialog, and price-slider interaction fixes.
- Reviewed the generated output manually and kept the documentation framed as new frontend feature work in the current application.
- Confirmed frontend changes with `npm run build` and browser-based interaction testing.
