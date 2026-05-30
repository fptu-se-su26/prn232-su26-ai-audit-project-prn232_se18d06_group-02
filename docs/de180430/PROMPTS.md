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

## Prompt #03

- Date: 2026-05-30
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Build order tracking and product review UI

### Prompt
Build an order tracking page with a visual timeline showing status steps (Pending, Processing, Shipping, Delivered) and a product review page with a star rating widget and comment form. Use Tailwind CSS and integrate with the existing orders and reviews API.

### Expected Output
- OrderTrackPage with vertical timeline UI and status-based color coding
- WriteReviewPage with interactive star rating (hover effect) and comment textarea

### Evaluation
AI generated both pages with consistent Tailwind styling. I verified the API integration and route protection manually.
