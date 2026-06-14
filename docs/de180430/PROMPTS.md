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

---

## Prompt #13

- Date: 2026-06-14
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Define the store-profile types and a stores API module for the seller view

### Prompt
I'm building a public store profile page. Define the TypeScript types for a store profile (name, slug,
logo, description, province, product count, total sold, rating, review count, follower count, isFollowing,
createdAt, review summary) and a follow-toggle result, plus a store product filter derived from the
existing product filter. Then create a stores API module using our apiClient/unwrap pattern: get a store
profile by slug, toggle follow, and get a store's products — building the query string the same way the
catalog module does.

### Expected Output
- `src/types/store.ts` with StoreProfile, FollowToggleResult, StoreProductFilter
- `src/api/stores.ts` with getStoreProfile, toggleStoreFollow, getStoreProducts

### Evaluation
Adopted. I pointed the products call at the dedicated `/stores/{slug}/products` endpoint and the follow
call at `/stores/{slug}/follow` (both by slug) after confirming the real backend routes, so no change to
the shared product filter was needed. Exported the param builder so it can be unit tested.

---

## Prompt #14

- Date: 2026-06-14
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Build the data hooks for the store profile, URL-driven filters, products, and follow

### Prompt
Create the hooks this page needs: a useStoreProfile hook that loads a store by slug with loading/error/
not-found states; a useStoreFilters hook that keeps sort, category, price range, and page number in the
URL search params (default sort popular, resetting the page when a filter changes); a useStoreProducts
hook that refetches when the filter changes; and a useStoreFollow hook with an optimistic toggle that
redirects anonymous users to login and reconciles the follower count from the response.

### Expected Output
- `src/hooks/{useStoreProfile,useStoreFilters,useStoreProducts,useStoreFollow}.ts`

### Evaluation
Adopted. Listing state lives entirely in the URL so the view is shareable and back/forward works. The
follow hook applies the optimistic change immediately, reconciles with the API result, and reverts on
failure; the profile sync is done during render (keyed on store id) to satisfy the strict hooks lint.

---

## Prompt #15

- Date: 2026-06-14
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Build the store header with identity, follow/chat actions, and a stats grid

### Prompt
Build the store header: a dark gradient banner with the logo (reusing our Avatar), the name, a verified
badge, location, a 2-line clamped description, a follow button with count, a chat button, and a stats
grid (products, total sold, rating with reviews, and "joined" as a relative time) — 4 columns on desktop
and 2 on mobile, with thousands separators. Add a small format helper for counts and relative time.

### Expected Output
- `src/components/store/{StoreHeader,StoreIdentity,StoreFollowButton,StoreChatButton,StoreStats,StoreStatItem}.tsx`
- `src/lib/format.ts`

### Evaluation
Adopted; I split each stat into a reusable StoreStatItem and kept the count/relative-time formatters as
pure functions. The chat button reuses the existing chat context's openChatWithStore action.

---

## Prompt #16

- Date: 2026-06-14
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Build the sticky sort tabs and the sidebar filters

### Prompt
Build the sticky sort tabs (popular, newest, best selling, price low→high, price high→low) with the total
product count, and the sidebar filters: a category hierarchy with an "All Products" option, a price-range
panel with apply that ignores negative/inverted ranges, and a conditional "clear all filters" link.

### Expected Output
- `src/components/store/{StoreSortTabs,StoreSidebar,StoreCategoryFilter,CategoryFilterItem,StorePriceFilter,ClearFiltersLink}.tsx`
- Shared sort options constant in `src/lib/storeSort.ts`

### Evaluation
Adopted. Moved the sort-options constant into its own module so the tab component file only exports a
component (Fast-Refresh rule). The price filter validates bounds before emitting and syncs its inputs when
the applied range changes externally.

---

## Prompt #17

- Date: 2026-06-14
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Build the product grid, empty state, and a reusable pagination control

### Prompt
Build the product grid using our existing ProductCard (2→3→4 columns), a "No Products Found" empty state
that reuses our EmptyState with a reset link, and a reusable pagination control showing the first/last/
neighbor pages with ellipsis and prev/next, with the page-list logic factored out so it can be tested.

### Expected Output
- `src/components/store/{StoreProductGrid,StoreProductsEmptyState}.tsx`
- `src/components/ui/Pagination.tsx` and `src/lib/pagination.ts`

### Evaluation
Adopted. The grid maps the catalog product into the ProductCard's data shape. Extracted `buildPageList`
into `lib/pagination.ts` so the component file stays component-only and the logic is unit tested.

---

## Prompt #18

- Date: 2026-06-14
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Assemble the store profile page, add the route, and handle states + responsive layout

### Prompt
Assemble the store profile page at /store/:slug, wiring the header, sort tabs, sidebar, grid, and
pagination to the URL-driven filter state. Add a public route, the loading/empty/error and store-not-found
states, and make the layout responsive: hide the sidebar on mobile behind a collapsible "Filters" toggle,
keep the sort tabs sticky, and use the 4→2 stats grid and 2→3→4 product grid.

### Expected Output
- `src/pages/StoreProfilePage.tsx`, `src/components/store/StoreNotFound.tsx`
- A public `/store/:slug` route in `App.tsx`

### Evaluation
Adopted. Each data source has its own loading/error handling so a slow product fetch never blanks the
header. Only the product list refetches on filter changes; profile and categories stay put. The route is
public; follow/chat actions redirect anonymous users to login.

---

## Prompt #19

- Date: 2026-06-14
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Confirm the chat button and the product-detail entry points reach the store page

### Prompt
Wire the chat button to open chat scoped to this store, and confirm the entry points (product detail
"View Shop" and the store logo/name) link to /store/{slug}.

### Expected Output
- Chat button calling openChatWithStore(slug)
- Verified product-detail links resolve to the new route

### Evaluation
The chat button reuses the existing chat context. The product detail page already linked to
`/store/{slug}` for the logo, name, and "View Shop" button, so those entry points resolve to the new page
with no change; the home page currently shows role shells with no store cards to wire.

---

## Prompt #20

- Date: 2026-06-14
- AI Tool: Claude Code
- Author: Nguyen Sinh Nhat (DE180430)
- Purpose: Add tests for the seller-view hooks, the param builder, pagination, and key components

### Prompt
Write tests: the store product param builder (emits/omits correctly), the pagination page-list (ellipsis
and bounds), the useStoreFilters URL round-trip (default sort, page reset, clear), the useStoreFollow
optimistic toggle (reconcile, revert on failure, anonymous redirect), and the price filter and sort tabs
components.

### Expected Output
- Unit tests for `toStoreProductParams` and `buildPageList`
- Hook tests for `useStoreFilters` and `useStoreFollow`
- Component tests for `StorePriceFilter` and `StoreSortTabs`

### Evaluation
Adopted. Kept the testable logic pure (param builder, page-list) and mocked the API/auth/navigation for
the follow hook. All 21 new tests pass alongside the existing suite, lint, and build.
