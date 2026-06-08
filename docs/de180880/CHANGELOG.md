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
