# CHANGELOG.md

## [2026-05-28]
Author: Nguyen Sinh Nhat (DE180430)

### Added
- Built a React-based login and registration page in `GearZone-FE`
- Added reusable auth UI components for layout, form sections, input fields, hero panels, and the social sign-in button
- Added frontend auth helpers for login, registration, logout, current-user retrieval, email verification, and verification resend

### Changed
- Configured frontend routing so the authentication flow works correctly after sign-in
- Adjusted auth page behavior for login, registration, and Google sign-in handling
- Updated the Vite path alias configuration for cleaner imports in the frontend codebase

### Fixed
- Fixed frontend import resolution issues related to `@/` path aliases
- Fixed post-login routing so the page no longer loops back incorrectly
- Fixed smaller interaction issues in the auth screen flow

### AI-assisted
- Used Claude Code in a limited way to review the implemented frontend and highlight a few issues for manual verification
- Final code changes and validation were performed manually by the author

---

## [2026-05-30] — Shopping Cart and Checkout
Author: Nguyen Sinh Nhat (DE180430)

### Added
- Built ProductCard reusable component with product image, price, brand, store, rating
- Created Cart API module (get, add, update quantity, remove)
- Built CartPage with item list, quantity controls, remove, order summary sidebar
- Created Checkout API module (getData, placeOrder, applyVoucher, cancelPayment)
- Built CheckoutPage with delivery address selection, payment method (COD/PayOS), voucher code
- Built PayOSCheckoutPage with QR code display and external payment link
- Built OrderSuccessPage with order confirmation and item list
- Registered routes in App.tsx: /cart, /checkout, /checkout/payos, /checkout/success/:orderId

### Changed
- App.tsx: added imports and routes for cart and checkout pages

### AI-assisted
- Used Claude Code to generate the UI components and Tailwind CSS structure
- API integration decisions, route protection, and payment flow logic were designed and verified manually

---

## [2026-06-13] — Real-time Chat (customer ↔ seller)
Author: Nguyen Sinh Nhat (DE180430)

### Added
- Chat data layer: TypeScript types (`types/chat.ts`) and REST API client (`api/chat.ts`)
- SignalR real-time client (`lib/chatHub.ts`) and `useChatHub` connection hook; added `@microsoft/signalr`
- Shared UI primitives (`components/ui/`): Avatar, UnreadBadge, EmptyState, ErrorState, LoadingOverlay
- Pure helpers: `lib/chatFormat.ts` (time/date/grouping), `lib/text.ts` (initials), `lib/sessionStore.ts`
- Hooks: `useDebouncedValue`, `useChatConversations`, `useChatThread`, `useAutoScroll`, `useMessageDraft`, `useChatWidget`, `useChatUnread`
- Conversation list (search, All/Unread filter, shop scope, infinite scroll) and components
- Message thread with date grouping, message bubble (own/incoming, "Seen"), product context card, and composer (draft persistence + 2000-char limit)
- Floating, responsive chat widget (desktop panel / mobile full-screen drawer) reusing a shared `ChatInboxLayout`
- Full chat page at `/messages`; `ChatProvider`/`useChatContext` with app-wide unread count and an `openChatWithStore` action
- Vitest setup with unit tests (formatters/initials) and a MessageBubble component test; `test` script

### Changed
- `main.tsx`: wrapped the app in `ChatProvider`
- `App.tsx`: added the `/messages` route
- `SiteLayout.tsx`: mounted the floating chat widget for authenticated customers (hidden on `/messages`)
- `ProductDetailPage.tsx`: wired the "Chat Now" button to open a conversation with the shop

### Fixed
- Repaired the app router (`App.tsx`): added missing page/layout imports, removed duplicated routes, and fixed the layout route nesting so the app compiles
- Cleared pre-existing lint errors in a few existing files using the project's standard rule disables and trivial safe fixes

### AI-assisted
- Built with Claude Code from a sequence of small, focused prompts (see PROMPTS.md #03–#12)
- Each step was reviewed and verified locally with `npm run lint`, `npm run build`, and `npm run test` before moving on
