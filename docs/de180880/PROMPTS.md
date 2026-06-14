# PROMPTS.md

## Prompt #01

- Date: 2026-05-29
- AI Tool: Codex
- Author: Dang Cong Quoc Khanh (DE180880)
- Purpose: Build the new React customer product browsing interface for GearZone FE

### Prompt
I am building a new GearZone frontend experience in React. Please help implement the customer product browsing page in `GearZone-FE`, including product cards, filters, sorting, price range filtering, infinite scroll, category navigation, a shared site header, and product search suggestions. Keep the implementation consistent with the GearZone API structure and make the page suitable for users browsing products in an ecommerce system.

### Expected Output
- A React product browsing page connected to catalog/product APIs
- Filter sidebar for brand, price, attributes, and stock status
- Product grid and list view support
- Infinite scrolling or load-more behavior for paginated products
- Shared header with category navigation and dropdown subcategories
- Header search with live product suggestions
- Successful frontend build verification

### Evaluation
The prompt was useful because it described the feature as a new customer-facing ecommerce workflow instead of a small UI-only task. The generated result still required manual review, browser testing, and several adjustments to category dropdown behavior, price filtering, search suggestions, and infinite scrolling.

## Prompt #02

- Date: 2026-06-08
- AI Tool: Codex
- Author: Dang Cong Quoc Khanh (DE180880)
- Purpose: Build a new React customer product detail flow and shopping actions in `GearZone-FE`

### Prompt
Please build the customer product detail page as a newly implemented React frontend feature in `GearZone-FE`, using the current GearZone APIs and business rules. Connect the page to the real product detail API, make the UI consistent with the current GearZone style, and implement working `Add to Cart`, `Buy Now`, cart count badge updates, product detail tabs, and product card add-to-cart behavior from the browsing page.

### Expected Output
- A new React product detail page connected to `/api/products/{slug}`
- Product detail UI with gallery, price, variants, specifications, reviews, related products, and store summary
- Working `Add to Cart` and `Buy Now` interactions using the existing cart and checkout APIs
- Shared header cart count updates after cart actions
- Product card `Add to Cart` support on the browsing page
- Brand filter and detail tab behavior working correctly
- Successful frontend build verification

### Evaluation
This prompt was useful because it clearly framed the work as building a new React shopping flow on top of the current project APIs instead of describing it like a rework of an older UI. The AI output accelerated page structure, API integration, and action handling, but manual review was still necessary to align the orange theme, cart badge behavior, brand filtering, login return flow, and section-anchor scrolling with the real app behavior.

## Prompt #03

- Date: 2026-06-14
- AI Tool: Codex
- Author: Dang Cong Quoc Khanh (DE180880)
- Purpose: Build the new React cart experience and improve customer shopping interactions in `GearZone-FE`

### Prompt
Please help build the customer cart experience in `GearZone-FE` as a new React frontend feature using the current project APIs. I need a proper cart page with store-grouped items, quantity controls, item selection, order summary, smooth quantity updates, a custom delete confirmation dialog, and reliable price-slider interaction on the browsing page. Keep the documentation and implementation framed as newly built frontend work in the current application, not as a refactor story from an older UI.

### Expected Output
- A React cart page at `/cart` with grouped store sections and product items
- Quantity increase/decrease controls that feel smooth and do not reload the full cart view
- Item and store selection behavior that updates the cart summary immediately
- A custom in-app confirmation dialog for removing cart items
- Cart navigation from customer shopping flows routed into the React cart page
- A more reliable custom price slider interaction for min/max filtering on the browsing page
- Successful frontend build verification

### Evaluation
This prompt was useful because it combined UX, state management, and interaction behavior into one customer shopping flow instead of treating them as isolated fixes. The AI output helped accelerate the cart page structure and interaction logic, but manual review and browser testing were still necessary to confirm that quantity updates felt smooth, the delete dialog matched the UI, and both price-slider handles worked correctly.
