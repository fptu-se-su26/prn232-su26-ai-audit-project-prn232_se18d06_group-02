# AI_AUDIT_LOG.md

## Log #01
- Date: 2026-05-29
- Author: Dang Cong Quoc Khanh (DE180880)
- AI Tool: Codex
- Purpose: Implementation support for the new customer product browsing experience in `GearZone-FE`
- Prompt Reference: PROMPTS.md#prompt-01
- AI Output Summary: Suggested and implemented a React-based catalog browsing flow with product listing, filtering, sorting, infinite scroll, shared site header, category dropdown navigation, and live product search suggestions.
- Human Decision: I reviewed the generated code, checked the UI behavior locally, and adjusted the implementation so it matched the intended GearZone customer shopping experience.
- Applied To: `GearZone-FE/src/pages/ProductBrowsePage.tsx`, `GearZone-FE/src/components/layout/SiteLayout.tsx`, `GearZone-FE/src/api/catalog.ts`, `GearZone-FE/src/types/catalog.ts`, `GearZone-FE/src/App.tsx`
- Verification: Verified the frontend build with `npm run build` and manually checked product browsing, category navigation, price filtering, and header search behavior in the browser.

## Usage Note
AI was used as a development assistant for building a new React frontend feature in the GearZone system. The work focused on creating a customer-facing catalog experience for the current project, including API integration, UI behavior, and route setup. Final decisions, manual review, and verification were performed by the author.

## Log #02
- Date: 2026-06-08
- Author: Dang Cong Quoc Khanh (DE180880)
- AI Tool: Codex
- Purpose: Implementation support for the new React customer product detail and cart interaction flow in `GearZone-FE`
- Prompt Reference: PROMPTS.md#prompt-02
- AI Output Summary: Suggested and implemented a new React product detail page, cart and buy-now actions, cart badge updates, browse-page add-to-cart behavior, brand filter correction, login return handling, and detail-tab scroll fixes using the current project APIs.
- Human Decision: I reviewed the generated code and kept the work framed as a newly built React frontend feature that consumes existing APIs, without describing it like a rewrite of an older UI layer.
- Applied To: `GearZone-FE/src/pages/ProductDetailPage.tsx`, `GearZone-FE/src/pages/ProductBrowsePage.tsx`, `GearZone-FE/src/components/layout/SiteLayout.tsx`, `GearZone-FE/src/pages/LoginPage.tsx`, `GearZone-FE/src/api/catalog.ts`, `GearZone-FE/src/types/catalog.ts`, `GearZone.Application/Features/Catalog/DTOs/ProductFilterDto.cs`
- Verification: Verified frontend behavior through browser checks on product detail, add-to-cart flow, buy-now flow, cart badge updates, browse-page add-to-cart behavior, and section-anchor scrolling. Confirmed frontend build with `npm run build`. Confirmed backend DTO change with `dotnet build GearZone.Application/GearZone.Application.csproj`.

## Usage Note Update
For DE180880, the recent AI-assisted work should be understood as building new React customer shopping features inside `GearZone-FE` while reusing the current GearZone backend APIs and business rules. The implementation was documented as new frontend feature work rather than as changes framed around an older UI version. Final selection of changes, review, verification, and responsibility remained with the author.

## Log #03
- Date: 2026-06-14
- Author: Dang Cong Quoc Khanh (DE180880)
- AI Tool: Codex
- Purpose: Implementation support for a new React cart experience and customer shopping interaction improvements in `GearZone-FE`
- Prompt Reference: PROMPTS.md#prompt-03
- AI Output Summary: Suggested and implemented a new React shopping cart experience with grouped store sections, item selection, optimistic quantity updates, cart summary, custom remove-item dialog, and cart-route wiring. Also refined the product browsing price slider so both ends can be adjusted more reliably with smoother pointer interaction.
- Human Decision: I reviewed the generated code and documented the work as newly built frontend functionality in the current React application, not as a rewrite or refactor narrative from an older UI.
- Applied To: `GearZone-FE/src/pages/CartPage.tsx`, `GearZone-FE/src/pages/ProductBrowsePage.tsx`, `GearZone-FE/src/index.css`, `GearZone-FE/src/pages/ProductDetailPage.tsx`, `GearZone-FE/src/components/layout/SiteLayout.tsx`
- Verification: Verified frontend build with `npm run build` and manually tested cart quantity changes, delete confirmation dialog, cart navigation, item selection, order summary updates, and price-slider dragging behavior in the browser.

## Usage Note Final
For DE180880, the AI-assisted work across these entries focused on building new React customer shopping features inside `GearZone-FE`, including browsing, product detail, cart actions, and cart-page interactions. Even when the implementation reused the project’s existing backend APIs and business rules, the frontend work was documented as new feature construction in the current application. Final review, testing, acceptance, and responsibility remained with the author.
