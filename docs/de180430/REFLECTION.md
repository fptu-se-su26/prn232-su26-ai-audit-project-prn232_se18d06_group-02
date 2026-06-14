# REFLECTION.md

## Reflection - Frontend Auth Feature

For this frontend task, I implemented the React login and registration feature in `GearZone-FE`, including the page structure, component split, auth API wiring, and route behavior. After finishing the main implementation, I used AI only in a limited way to review the result and point out a few issues that were worth checking again.

The most helpful use of AI in this task was as a review assistant rather than as the main source of code. It helped surface a few practical issues such as route handling after sign-in, small behavior mismatches in the auth flow, and missing helper methods in the frontend auth API module. I still had to evaluate each suggestion myself, decide whether it matched my intended feature scope, and test the final result locally.

This process reinforced that AI is useful for catching small mistakes or missing details, but it does not replace understanding the code or verifying the implementation manually. In this task, the final confidence came from reading the code carefully, reviewing the affected files, and confirming the result with `npm lint` and `npm build`.

### What I learned
- Breaking a larger page into smaller reusable React components makes it easier to maintain and debug
- Frontend auth flow needs both UI validation and route validation, not just successful API calls
- AI is more useful when the prompt is narrow and specific, especially for final review tasks
- Manual verification is still necessary even when AI suggestions look correct at first glance

---

## Reflection - Shopping Cart and Checkout

For this frontend task, I implemented the shopping cart, checkout flow, PayOS payment, and order confirmation pages in `GearZone-FE`. I used AI to generate the initial UI components and Tailwind CSS structure, then reviewed and verified all integration points manually.

The payment method branching (COD goes to order success, PayOS goes through QR page first) required careful design of the placeOrder handler. The voucher apply feature needed a separate API call with proper error and success message handling.

This experience reinforced that AI is useful for generating component structure quickly, but the API integration, state management, and route protection decisions need manual design and verification.

### What I learned
- Payment flow branching (COD vs online payment) needs clear handler design before implementation
- Voucher apply requires separate API calls with proper feedback messages for success and failure
- Using React useState for loading states during API calls gives users important visual feedback
- Consistent Tailwind component patterns across multiple pages make the overall UI feel cohesive

---

## Reflection - Real-time Chat (customer ↔ seller)

For this feature I built a real-time chat between customers and sellers in `GearZone-FE`: a floating
widget available across the site, a full-screen `/messages` page, a conversation list with search and
filters, and a live message thread. I used AI heavily but in small, focused steps — one prompt per
concern (types, API client, SignalR client, list, thread, composer, widget, integration, tests) rather
than one giant "build the whole thing" request. That kept each piece reviewable and let me verify with
lint, build, and tests before moving on.

The most important design decisions were mine to make and verify. I decided to send messages over the
REST endpoint and use SignalR only for receiving, deduping by message id, which avoided double-sends and
race conditions when the echo arrives. I kept the SignalR connection in React state so subscribers
re-bind cleanly once it is ready and after a reconnect. I routed the unread count through a single
context so the widget badge and the rest of the app always agree. And I reused one `ChatInboxLayout`
inside both the widget and the full page so there is exactly one implementation to maintain.

A few things needed real human judgement beyond accepting AI output. Auto-scroll was subtle: scrolling
to the bottom on every change would fight the "load older messages" behaviour, so I keyed it on the
latest message id instead of the message count. I also hit pre-existing problems in the project — the
router file didn't compile (missing imports, duplicated routes) and several existing files tripped a
strict hooks lint rule — so I repaired the router (needed for anything to build) and followed the team's
established lint-disable convention to get a green build and lint.

### How I verified the results
- Ran `npm run lint`, `npm run build` (tsc + vite), and `npm run test` after each step; all green.
- Wrote unit tests for the pure formatting/initials helpers and a component test for the message bubble.
- Confirmed the chat API calls and SignalR events match the backend chat contract before wiring the UI.

### What I learned
- Splitting a real-time feature into a thin data/connection layer plus small components and hooks makes
  it far easier to reason about and test.
- For real-time UIs, deciding the send/receive strategy (REST send + hub receive + dedupe) up front
  prevents a class of duplication and ordering bugs.
- A single source of truth for cross-cutting state (the unread count) avoids subtle desyncs between a
  floating widget and the rest of the app.
- AI is great at producing the scaffolding quickly, but the timing-sensitive behaviours (auto-scroll,
  reconnect, read sync) still need careful human verification.

### Difficulties
- Getting auto-scroll to behave for both new messages and "load older" history.
- Working around pre-existing build/lint breakage in the project before the feature could be verified.
- Configuring the test runner (automatic JSX, excluding test files from the production build) on a very
  new toolchain.

---

## Reflection - Seller View (customer-facing store profile)

For this feature I built the public store profile page in `GearZone-FE`: a header with the seller's
identity, follow and chat actions, and a stats grid, followed by a product listing with sort, category and
price filters, and pagination. I used AI in small, focused steps — one prompt per concern (types, API,
hooks, header, controls, grid/pagination, page assembly, tests) — so each piece stayed reviewable and I
could verify with lint, build, and tests before moving on.

The decisions that mattered were mine to make and verify. The most important was to keep the entire
listing state (sort, category, price, page) in the URL search params as a single source of truth, so the
view is shareable and the browser back/forward buttons work, and only the product list refetches while the
header and categories stay put. I also made the follow action optimistic — applying the change instantly,
then reconciling with the API response and reverting on failure — and made it redirect anonymous users to
login rather than silently doing nothing.

A few things needed human judgement beyond accepting AI output. Before wiring anything I checked the real
backend routes and found the follow and product endpoints are keyed by slug (not the id the early notes
assumed), with a dedicated `/stores/{slug}/products` route — so I targeted those directly and avoided
changing the shared product filter. A strict new hooks lint rule also pushed me to sync prop-derived state
during render (keyed on the store id) instead of in an effect, and to move the sort-options constant and
the pagination page-list logic out of the component files (both for the Fast-Refresh rule and to make the
logic unit-testable).

### How I verified the results
- Ran `npm run lint`, `npm run build` (tsc + vite), and `npm run test` after each step; all green.
- Wrote unit tests for the pure logic (param builder, pagination page-list), hook tests for the URL-driven
  filters and the optimistic follow, and component tests for the price filter and sort tabs.
- Confirmed the store API calls and DTO shapes match the backend before wiring the UI, and that the
  product-detail "View Shop" links resolve to the new route.

### What I learned
- Putting listing state in the URL keeps the page shareable and makes filter/pagination behaviour
  predictable, with refetching scoped to just the product list.
- Reusing the project's shared primitives, ProductCard, and the chat integration point kept the feature
  small and consistent instead of duplicating UI.
- Verifying the actual backend routes up front prevented wiring against assumed endpoints.

### Difficulties
- Reconciling the early route assumptions with the real (slug-based) backend endpoints.
- Satisfying the strict React 19 hooks/refresh lint rules without changing behaviour (render-time state
  sync, and moving constants/logic into `lib/`).
