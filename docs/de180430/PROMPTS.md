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

## Prompt #04

- Date: 2026-05-30
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Build user profile page with orders and addresses

### Prompt
Build a user profile page with a tabbed interface showing order history (with status badges and tracking links) and address management (view, add, delete). Use Tailwind CSS and integrate with the existing users API for orders and addresses.

### Expected Output
- ProfilePage with user avatar header and tab navigation
- Orders tab: order list with status badges, sub-order tracking links
- Addresses tab: address list, add new address form, delete button

### Evaluation
AI generated the profile page with tabbed layout. I verified the URL parameter handling for tab state and the address CRUD operations manually.
