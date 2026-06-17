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
- Date: 2026-06-13
- Author: Nguyen Sinh Nhat (DE180430)
- AI Tool: Claude Code
- Purpose: Build the data and real-time layer for the customer–seller chat feature
- Prompt Reference: PROMPTS.md#prompt-03 .. #prompt-05
- AI Output Summary: Generated the chat TypeScript types (`src/types/chat.ts`), the REST API module (`src/api/chat.ts`) following the existing apiClient/unwrap pattern, a typed SignalR client (`src/lib/chatHub.ts`), and a `useChatHub` hook managing the connection lifecycle with reconnect.
- Human Decision: I chose to send messages over the REST endpoint and rely on the hub only for receiving (deduping by message id) to avoid double-sends. I kept the connection in React state so subscribers re-bind cleanly once it is ready and after a reconnect. Optional/nullable fields were made explicit in the types.
- Applied To: `GearZone-FE/src/types/chat.ts`, `GearZone-FE/src/api/chat.ts`, `GearZone-FE/src/lib/chatHub.ts`, `GearZone-FE/src/hooks/useChatHub.ts`, `GearZone-FE/package.json` (added `@microsoft/signalr`)
- Verification: `npm run lint` and `npx tsc -b` pass for these files; confirmed the endpoint paths and payload shapes line up with the backend chat contract.

## Log #04
- Date: 2026-06-13
- Author: Nguyen Sinh Nhat (DE180430)
- AI Tool: Claude Code
- Purpose: Build the shared primitives and the conversation-list, thread, and composer UI
- Prompt Reference: PROMPTS.md#prompt-06 .. #prompt-09
- AI Output Summary: Generated shared `components/ui` primitives (Avatar, UnreadBadge, EmptyState, ErrorState, LoadingOverlay), pure formatting helpers (`chatFormat`, `text`), the conversation list (with `useChatConversations`, search/filter/scope, infinite scroll), the message thread (with `useChatThread`, `useAutoScroll`, message bubble and date grouping), and the composer (with `useMessageDraft` and a 2000-char limit).
- Human Decision: Kept every component in its own file and pushed shared logic into hooks/utilities to avoid duplication; promoted the UI primitives and the debounce hook into shared locations so the next feature can reuse them. Keyed auto-scroll on the latest message id so loading older history doesn't jump the view.
- Applied To: `GearZone-FE/src/components/ui/*`, `GearZone-FE/src/lib/chatFormat.ts`, `GearZone-FE/src/lib/text.ts`, `GearZone-FE/src/lib/sessionStore.ts`, `GearZone-FE/src/hooks/{useDebouncedValue,useChatConversations,useChatThread,useAutoScroll,useMessageDraft}.ts`, `GearZone-FE/src/components/chat/*`
- Verification: `npm run lint` clean; `npx tsc -b` passes. Followed the project convention of a file-level `react-hooks/set-state-in-effect` disable on data-fetching hooks (the same pattern the existing pages use).

## Log #05
- Date: 2026-06-13
- Author: Nguyen Sinh Nhat (DE180430)
- AI Tool: Claude Code
- Purpose: Build the floating widget, the full chat page, and wire everything into the app
- Prompt Reference: PROMPTS.md#prompt-10 .. #prompt-11
- AI Output Summary: Generated `useChatWidget` (open/close + sessionStorage), the widget shell (ChatLauncher/ChatWidget/Drawer/Overlay) and the shared `ChatInboxLayout`, a `ChatProvider`/`useChatContext` exposing the hub, widget state, app-wide unread count (`useChatUnread`) and an `openChatWithStore` action, plus the `/messages` page.
- Human Decision: Routed the unread count through one context so the bubble badge has a single source of truth, and reused `ChatInboxLayout` in both the widget and the page so there is one implementation. Mounted the widget in the customer layout and wired the product page "Chat Now" button to open a conversation (redirecting non-customers to login). While integrating, I also had to repair the app shell routing/imports (the router file was missing imports and had duplicated routes) so the app compiles, and apply the project's standard lint disable to a few existing data-fetching files.
- Applied To: `GearZone-FE/src/hooks/{useChatWidget,useChatUnread}.ts`, `GearZone-FE/src/contexts/{chat-context.ts,useChatContext.ts,ChatProvider.tsx}`, `GearZone-FE/src/components/chat/{ChatInboxLayout,ChatLauncher,ChatWidget,ChatWidgetDrawer,ChatWidgetOverlay}.tsx`, `GearZone-FE/src/pages/ChatPage.tsx`, `GearZone-FE/src/{App.tsx,main.tsx}`, `GearZone-FE/src/components/layout/SiteLayout.tsx`, `GearZone-FE/src/pages/ProductDetailPage.tsx`
- Verification: `npm run lint` clean, `npm run build` (tsc + vite) succeeds, app compiles end to end.

## Log #06
- Date: 2026-06-13
- Author: Nguyen Sinh Nhat (DE180430)
- AI Tool: Claude Code
- Purpose: Add automated tests for the chat feature
- Prompt Reference: PROMPTS.md#prompt-12
- AI Output Summary: Set up Vitest (jsdom + Testing Library) and wrote unit tests for the formatting/initials helpers and a component test for the message bubble (own vs incoming, and the "Seen" marker).
- Human Decision: Configured automatic JSX so component tests render without importing React, and excluded test files from the production tsc build. Focused unit coverage on the pure logic that is most error-prone.
- Applied To: `GearZone-FE/vitest.config.ts`, `GearZone-FE/src/test/setup.ts`, `GearZone-FE/src/lib/{text,chatFormat}.test.ts`, `GearZone-FE/src/components/chat/MessageBubble.test.tsx`, `GearZone-FE/package.json` (test deps + script), `GearZone-FE/tsconfig.app.json`
- Verification: `npm run test` → 13 tests pass; `npm run lint` and `npm run build` remain green.

## Usage Note
For the real-time chat feature, AI generated the bulk of the types, hooks, and components from a sequence
of small, focused prompts. Every step was reviewed and verified locally with lint, build, and tests
before moving on; the architecture decisions (REST-send + hub-receive, one shared inbox layout, a single
unread source of truth, file-per-component) were made and confirmed by the author.

---

## Log #07
- Date: 2026-06-14
- Author: Nguyen Sinh Nhat (DE180430)
- AI Tool: Claude Code
- Purpose: Build the data layer and hooks for the customer-facing store profile (seller view)
- Prompt Reference: PROMPTS.md#prompt-13 .. #prompt-14
- AI Output Summary: Generated the store types (`types/store.ts`), a stores API module (`api/stores.ts`)
  following the apiClient/unwrap pattern, and the four data hooks (`useStoreProfile`, `useStoreFilters`,
  `useStoreProducts`, `useStoreFollow`).
- Human Decision: Confirmed the real backend routes first and targeted the dedicated
  `/stores/{slug}/products` and `/stores/{slug}/follow` (by slug) endpoints, so the shared product filter
  needed no change. Kept all listing state in the URL search params as the single source of truth, made
  the follow toggle optimistic with reconcile-and-revert, and synced profile-derived state during render
  (keyed on store id) to satisfy the strict hooks lint.
- Applied To: `GearZone-FE/src/types/store.ts`, `GearZone-FE/src/api/stores.ts`,
  `GearZone-FE/src/hooks/{useStoreProfile,useStoreFilters,useStoreProducts,useStoreFollow}.ts`
- Verification: `npx tsc -b` and `npm run lint` clean; followed the project's file-level
  `react-hooks/set-state-in-effect` disable on the two data-fetching hooks (same as existing pages).

## Log #08
- Date: 2026-06-14
- Author: Nguyen Sinh Nhat (DE180430)
- AI Tool: Claude Code
- Purpose: Build the seller-view UI, assemble the page/route, and add tests
- Prompt Reference: PROMPTS.md#prompt-15 .. #prompt-20
- AI Output Summary: Generated the header (banner, identity with Avatar, follow/chat buttons, stats grid),
  the sticky sort tabs and sidebar filters (category hierarchy + price range + clear link), the product
  grid reusing `ProductCard`, a reusable `components/ui/Pagination`, the `StoreProfilePage` at
  `/store/:slug` with loading/empty/error/not-found states and a responsive layout, plus format helpers
  and unit/component/hook tests.
- Human Decision: One component per file with shared logic in hooks and shared UI in `components/ui/`;
  reused the existing `Avatar`, `EmptyState`, `ErrorState`, `LoadingOverlay`, `ProductCard`, and the chat
  context's `openChatWithStore`. Moved the sort-options constant and the pagination page-list logic into
  `lib/` so the component files stay component-only (Fast-Refresh rule) and the logic is unit testable.
  Added a mobile "Filters" collapsible so filtering stays reachable when the sidebar is hidden. Verified
  the product-detail "View Shop" links already resolve to the new route (no change needed).
- Applied To: `GearZone-FE/src/components/store/*`, `GearZone-FE/src/components/ui/Pagination.tsx`,
  `GearZone-FE/src/lib/{format,storeSort,pagination}.ts`, `GearZone-FE/src/pages/StoreProfilePage.tsx`,
  `GearZone-FE/src/App.tsx`, and the matching `*.test.ts(x)` files
- Verification: `npm run lint` clean, `npm run build` (tsc + vite) succeeds, `npm run test` → 34 tests
  pass (21 new for the seller view).

## Usage Note
The seller view was built from a sequence of small, focused prompts on top of the shared primitives,
catalog API, ProductCard, and chat integration point already in the project. Each step was reviewed and
verified locally with lint, build, and tests before moving on; the routing, endpoint, and reuse decisions
were made and confirmed by the author.
