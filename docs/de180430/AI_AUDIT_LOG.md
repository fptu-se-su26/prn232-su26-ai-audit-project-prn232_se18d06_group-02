# AI_AUDIT_LOG.md

## Log #01
- Date: 2026-05-28
- Author: Nguyen Sinh Nhat (DE180430)
- AI Tool: Claude Code
- Purpose: Final review support for the React login and registration frontend in `GearZone-FE`
- Prompt Reference: PROMPTS.md#prompt-01
- AI Output Summary: Identified a few focused issues related to post-login routing, Google sign-in behavior, auth helper completeness, and UI consistency
- Human Decision: I reviewed each suggestion manually, kept only the items that matched my intended frontend scope, and ignored suggestions that were outside the feature boundary
- Applied To: `GearZone-FE/src/pages/LoginPage.tsx`, `GearZone-FE/src/components/auth/AuthPageShell.tsx`, `GearZone-FE/src/api/auth.ts`, `GearZone-FE/src/App.tsx`
- Verification: Confirmed changes manually and validated them with `npm lint` and `npm build`

## Usage Note
For this feature, AI was used only as a lightweight review assistant near the end of the implementation. The page structure, component split, routing setup, and auth flow decisions were implemented manually and then verified with local tooling.

---

## Log #02
- Date: 2026-05-30
- Author: Nguyen Sinh Nhat (DE180430)
- AI Tool: Claude Code
- Purpose: Implement shopping cart, checkout flow, PayOS payment, and order confirmation pages
- Prompt Reference: PROMPTS.md#prompt-02
- AI Output Summary: Generated CartPage with quantity controls and remove functionality, CheckoutPage with address selection and payment method choice, PayOSCheckoutPage with QR code display, and OrderSuccessPage for order confirmation. AI assisted with Tailwind CSS component structure and React state management patterns.
- Human Decision: I reviewed all generated pages manually, kept the layout and styling that matched my intended design, and adjusted API integration points to align with existing backend contracts
- Applied To: `GearZone-FE/src/components/ProductCard.tsx`, `GearZone-FE/src/api/cart.ts`, `GearZone-FE/src/api/checkout.ts`, `GearZone-FE/src/pages/CartPage.tsx`, `GearZone-FE/src/pages/CheckoutPage.tsx`, `GearZone-FE/src/pages/PayOSCheckoutPage.tsx`, `GearZone-FE/src/pages/OrderSuccessPage.tsx`, `GearZone-FE/src/App.tsx`
- Verification: Confirmed changes manually and validated them with `pnpm build` in `GearZone-FE`

## Usage Note
AI assisted with generating the UI components and Tailwind CSS structure. The API integration decisions, route protection, and payment flow logic were designed and verified manually.

---

## Log #03
- Date: 2026-06-09
- Author: Nguyen Sinh Nhat (DE180430)
- AI Tool: Claude Code
- Purpose: Implement the seller messaging page — a two-panel inbox and thread interface for Store Owner users
- Prompt Reference: PROMPTS.md#prompt-03, PROMPTS.md#prompt-04, PROMPTS.md#prompt-05
- AI Output Summary: Prompt-03 produced the component state shape, interface definitions, and handler outline. Prompt-04 generated the Tailwind CSS class structure for the sidebar and message bubbles. Prompt-05 reviewed the polling implementation and flagged the cleanup pattern in the useEffect return.
- Human Decision: I designed the two-panel layout decision (sidebar vs. thread split) and the polling interval before prompting. I reviewed the generated Tailwind classes and adjusted color choices to match the existing seller theme. I verified the mark-as-read side-effect and the sending flow manually. The API module structure was written manually using the existing apiClient pattern.
- Applied To: `GearZone-FE/src/api/chat.ts`, `GearZone-FE/src/pages/seller/SellerMessagesPage.tsx`, `GearZone-FE/src/App.tsx`
- Verification: Confirmed page renders and routing works. Validated with `pnpm build` in `GearZone-FE` with zero errors.

## Usage Note
For this feature, I designed the overall layout structure and the API contract shape before using AI. AI was mainly used to generate the Tailwind CSS styling and to review the polling cleanup logic. The conversation sidebar, thread rendering, and send handler were understood and verified manually before committing.
