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

- Date: 2026-06-13
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Define the TypeScript types for a customer–seller real-time chat feature

### Prompt
I'm building a real-time chat between a customer and a seller in GearZone-FE. Define the TypeScript
types I'll need: a conversation list item, a single message, a full conversation thread, an optional
product context attached to a conversation, a widget bootstrap payload, and the SignalR event payloads.
Reuse the existing PagedResult type for pagination.

### Expected Output
- Interfaces for conversation list item, message, thread, product context, bootstrap, scope option
- Send-message DTO and query param types
- Constants for page sizes and the message length limit

### Evaluation
Used as the data contract for the whole feature. I kept the types in `src/types/chat.ts` and made every
optional/nullable field explicit so the UI knows what to guard against.

---

## Prompt #04

- Date: 2026-06-13
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Create the chat API client module

### Prompt
Create an API module for chat using our existing apiClient/unwrap pattern: bootstrap the widget, list
the inbox, fetch a thread, fetch a single conversation update, get the unread count, get scope options,
ensure a conversation exists for a shop, send a message, and mark a conversation read. Build query
strings the same way the catalog module does.

### Expected Output
- `src/api/chat.ts` with one function per endpoint
- Consistent query-string building and unwrap() usage

### Evaluation
Adopted as-is. I made small helpers return the inner value (e.g. unread count, conversationId) so calling
code stays clean.

---

## Prompt #05

- Date: 2026-06-13
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Set up the SignalR client and a connection hook

### Prompt
Set up a SignalR client over /hubs/chat with automatic reconnect, plus join/leave/send/mark-read
methods and typed handlers for message-received, conversation-updated, unread-counts-updated, and
conversation-read events. Wrap it in a useChatHub hook that owns the connection lifecycle for an
authenticated customer and exposes a subscribe function and the invoke methods.

### Expected Output
- A typed connection factory and bind/unbind helpers in `src/lib/chatHub.ts`
- A `useChatHub` hook that only publishes the connection once it has started

### Evaluation
I kept the connection in state (not a ref) so consumers re-subscribe cleanly when it becomes ready and on
reconnect. Decided to send messages over REST and rely on the hub for receiving, deduping by message id.

---

## Prompt #06

- Date: 2026-06-13
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Build shared UI primitives and chat formatting helpers

### Prompt
Build the small reusable pieces this feature needs: an image-or-initials Avatar, an unread count badge
(max "99+"), and empty/error/loading states. Also add formatting helpers: message time as HH:mm, a date
separator dd/MM/yyyy, a list timestamp (time if today else dd/MM), grouping messages by day, and an
initials helper.

### Expected Output
- `src/components/ui/{Avatar,UnreadBadge,EmptyState,ErrorState,LoadingOverlay}.tsx`
- `src/lib/chatFormat.ts` and `src/lib/text.ts`

### Evaluation
Pulled these into a shared `components/ui` folder so other pages can reuse them. Kept the formatters as
pure functions so they're easy to unit test.

---

## Prompt #07

- Date: 2026-06-13
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Build the conversation list with search, filter, and infinite scroll

### Prompt
Build the conversation list: a header with a debounced search box, an All/Unread filter, and a shop
scope dropdown, plus a scrollable list with infinite scroll. Extract a useChatConversations hook that
loads pages, applies the filters, and silently refreshes when a conversation-updated event arrives.

### Expected Output
- `useChatConversations` hook
- ConversationList, ConversationListItem, ConversationListHeader, search/filter/scope components

### Evaluation
I split each control into its own component for reuse and kept the list state in the hook. Search uses the
shared debounce hook (250ms).

---

## Prompt #08

- Date: 2026-06-13
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Build the message thread with live updates and auto-scroll

### Prompt
Build the message thread: a useChatThread hook that loads a conversation, pages older messages when you
scroll to the top, appends new messages from the hub (dedup by id), and reflects read state. Render
messages grouped by day with a date separator, and a message bubble that aligns my own messages right
with a "Seen" marker when read. Auto-scroll to the latest message on new messages and when a conversation
opens, but not when loading older history.

### Expected Output
- useChatThread + useAutoScroll hooks
- MessageList, MessageGroup, MessageDateSeparator, MessageBubble, ChatThreadHeader, ProductContextCard

### Evaluation
Keyed auto-scroll on the latest message id so loading older messages doesn't jump the view. Used the
buyer id from the thread to decide which messages are "mine".

---

## Prompt #09

- Date: 2026-06-13
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Build the message composer with drafts and a length limit

### Prompt
Build a composer with a textarea and send button. Persist a per-conversation draft in sessionStorage,
send on Enter (Shift+Enter for newline), disable send when empty or while sending, and enforce a
2000-character limit with a warning.

### Expected Output
- ChatComposer component and a useMessageDraft hook

### Evaluation
Adopted. Sending posts to the REST endpoint and appends the returned message; the hub echo is deduped so
nothing doubles up.

---

## Prompt #10

- Date: 2026-06-13
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Build the responsive floating chat widget

### Prompt
Build a floating chat widget: a bubble with an unread badge, and a drawer that is a floating panel on
desktop (bottom-right) and a full-screen drawer with a backdrop on mobile. Persist the open/closed state
in sessionStorage. Reuse the same inbox layout (list + thread) inside the drawer.

### Expected Output
- useChatWidget hook; ChatLauncher, ChatWidget, ChatWidgetDrawer, ChatWidgetOverlay, ChatInboxLayout

### Evaluation
Reused ChatInboxLayout so the widget and the full page share exactly one implementation. The widget hides
itself on the full chat page.

---

## Prompt #11

- Date: 2026-06-13
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Add the full chat page and wire the feature into the app

### Prompt
Add a full-screen chat page at /messages that reuses the inbox layout, and wire the feature into the app:
a ChatProvider that exposes the hub, the widget state, the app-wide unread count, and an
openChatWithStore(slug) action. Mount the widget in the customer layout for authenticated customers, and
make the product page "Chat Now" button open a conversation with that shop.

### Expected Output
- ChatPage + /messages route; ChatProvider/useChatContext; useChatUnread
- Widget mounted in SiteLayout; product-detail Chat button wired

### Evaluation
Put the connection, unread count, and "open chat with store" action in one context so every entry point
shares a single source of truth. The Chat button redirects to login when the visitor isn't a customer.

---

## Prompt #12

- Date: 2026-06-13
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Add tests for the chat formatting helpers and the message bubble

### Prompt
Set up Vitest and write tests: unit tests for the time/date/grouping/initials helpers, and a component
test for the message bubble covering own vs incoming rendering and the "Seen" marker.

### Expected Output
- Vitest config + setup; chatFormat, text, and MessageBubble tests; a `test` script

### Evaluation
Kept the formatters pure so they were straightforward to test. Configured automatic JSX so the component
test renders without importing React. All tests pass alongside lint and build.
