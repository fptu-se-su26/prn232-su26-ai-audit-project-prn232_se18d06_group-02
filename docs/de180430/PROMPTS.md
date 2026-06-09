# PROMPTS.md

## Prompt #01

- Date: 2026-05-28
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Review the React login and registration feature and suggest a few focused improvements for UI consistency, routing behavior, and auth API completeness

### Prompt
I implemented a React login and registration page in GearZone-FE and split the UI into smaller components. Please review the current frontend implementation and point out only the most important issues related to UI consistency, login and registration behavior, post-login routing, and missing auth helper methods. Keep the scope limited to this frontend feature and avoid unrelated backend suggestions.

### Expected Output
- A short list of important frontend issues
- Any routing or login/registration behavior problems
- Any missing auth helper methods relevant to this feature

### Evaluation
This prompt was specific enough to keep AI focused on the frontend auth feature instead of expanding into unrelated areas. The output was useful as a final review checklist, but the final implementation choices and verification were still done manually.

---

## Prompt #02

- Date: 2026-05-30
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Build shopping cart, checkout flow, PayOS online payment, and order confirmation pages

### Prompt
Build a shopping cart page with item quantity controls and order summary, a checkout page with address selection and payment method choice (COD and PayOS), a PayOS payment page with QR code display, and an order success confirmation page. Use Tailwind CSS and the existing API client pattern.

### Expected Output
- CartPage with item list, quantity +/- controls, remove, order summary
- CheckoutPage with address radio buttons, payment method selection, voucher input
- PayOSCheckoutPage with QR code image and cancel button
- OrderSuccessPage with order details and action links

### Evaluation
AI generated all four pages with consistent Tailwind styling. I verified the API integration points, route protection, and payment flow logic manually to ensure they aligned with the existing backend contracts.

---

## Prompt #03

- Date: 2026-06-09
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Build a public store profile page with banner, logo, stats, and a responsive products grid

### Prompt
Build a public store profile page in React with TypeScript. The page should have: a full-width banner section (image or blue gradient fallback) with an overlapping store logo, store name and optional verified badge, and a follow/unfollow toggle button. Below the banner show a stats row with follower count, product count, total sold, and rating using Material Symbols icons. Then a responsive products grid in 2–5 columns using the existing ProductCard component. Use Tailwind CSS matching a light/white theme consistent with the other public pages in the project.

### Expected Output
- Banner section with logo overlay and follow button
- Stats row with icon-label pairs
- Products grid using ProductCard
- Loading and empty states throughout

### Evaluation
The generated component covered all required sections. I reviewed the spacing and color choices to ensure they matched the existing public pages rather than the seller dashboard dark theme. I also adjusted the optimistic follow toggle update to correctly increment/decrement the follower count display without refetching.

---

## Prompt #04

- Date: 2026-06-09
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Add store profile and products API functions to the existing catalog module

### Prompt
Add three functions to the existing GearZone-FE catalog API module following the same pattern as the other functions: getStoreProfile(slug) calling GET /stores/{slug}, getStoreProducts(slug, params?) calling GET /stores/{slug}/products, and followStore(slug) calling POST /stores/{slug}/follow. The return types should use the existing unwrap() helper and match the appropriate TypeScript interfaces. Where should I define the StoreProfile interface?

### Expected Output
- Three async functions following the `unwrap(await apiClient.get(...))` pattern
- Recommendation on where to put StoreProfile interface
- TypeScript interface for StoreProfile

### Evaluation
AI correctly suggested placing StoreProfile in types/catalog.ts alongside the other catalog types. The function signatures matched the existing pattern exactly. I added optional fields (followerCount, totalSold, etc.) based on what the backend was expected to return.
