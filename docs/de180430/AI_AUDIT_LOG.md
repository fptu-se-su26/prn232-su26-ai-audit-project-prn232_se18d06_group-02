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

## Log #03
- Date: 2026-05-30
- Author: Nguyen Sinh Nhat (DE180430)
- AI Tool: Claude Code
- Purpose: Implement order tracking timeline and product review submission pages
- Prompt Reference: PROMPTS.md#prompt-03
- AI Output Summary: Generated OrderTrackPage with visual timeline UI and status-based color coding, WriteReviewPage with interactive star rating widget (hover effect) and comment form
- Human Decision: I reviewed the generated pages manually, adjusted the timeline styling and star rating behavior to match the intended design
- Applied To: `GearZone-FE/src/api/orders.ts`, `GearZone-FE/src/api/reviews.ts`, `GearZone-FE/src/pages/OrderTrackPage.tsx`, `GearZone-FE/src/pages/WriteReviewPage.tsx`, `GearZone-FE/src/App.tsx`
- Verification: Confirmed changes manually and validated them with `pnpm build` in `GearZone-FE`

## Usage Note
AI assisted with generating the order tracking timeline CSS and star rating widget. The API integration and route protection were verified manually.
