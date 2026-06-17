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
