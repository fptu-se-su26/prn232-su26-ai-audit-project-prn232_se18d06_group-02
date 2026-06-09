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
- Purpose: Design the component state shape and event handler structure for the seller messages page

### Prompt
I am building a seller messages page in React with TypeScript. The page has a sidebar listing conversations and a main panel showing the selected conversation thread. Describe the TypeScript interfaces needed for ConversationSummary and Message, the useState hooks required, and the event handlers for selecting a conversation, sending a message, and polling for updates every 5 seconds.

### Expected Output
- TypeScript interfaces for ConversationSummary, Message, and ThreadData
- List of useState hooks with types
- Outline of event handlers: loadInbox, loadThread, handleSend, polling useEffect

### Evaluation
The output gave me a clear structure to start from. I adjusted the interface fields based on the actual backend response shape I expected, and I simplified some handler names. The polling useEffect pattern was useful as a reference.

---

## Prompt #04

- Date: 2026-06-09
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Generate Tailwind CSS class structure for the two-panel chat layout

### Prompt
Generate Tailwind CSS classes for a two-panel seller chat interface. Left sidebar is 288px wide with a dark slate background (`bg-slate-900`), a list of conversation items with an active state using an amber left border and amber/10 background. Right panel is `bg-slate-950`. Message bubbles: seller messages are right-aligned with `bg-gradient-to-br from-amber-500 to-orange-600`, buyer messages are left-aligned with `bg-white/10 border border-white/10`. Use `rounded-2xl` with asymmetric corner on the chat tail side.

### Expected Output
- Sidebar container classes
- Conversation list item classes including active state
- Message bubble classes for both sender sides
- Input area container classes

### Evaluation
The generated classes were mostly usable. I tweaked a few values — for example the input focus ring was changed from `ring` to `border` to stay consistent with other inputs in the codebase. The overall dark theme structure was correct and matched the existing seller shell palette.

---

## Prompt #05

- Date: 2026-06-09
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Review the real-time polling implementation for memory leaks or race conditions

### Prompt
Review this React useEffect polling pattern: a setInterval is started when activeId changes, polling an API every 5 seconds. The interval ID is stored in a useRef. The useEffect cleanup function clears the interval. Is there a risk of stale closures, memory leaks, or multiple intervals piling up if activeId changes quickly? What would you change?

### Expected Output
- Assessment of whether the pattern is safe
- Any stale closure or race condition risks
- Suggested improvements if any

### Evaluation
AI confirmed the pattern is correct for this use case — the ref-based interval ID and the cleanup return prevent interval pile-up. It noted that the conversationUpdate callback uses a setState updater function which avoids stale closure issues. No changes were needed based on this review.
