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
