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

## Reflection - Seller Messaging Page

For this feature, I implemented a real-time messaging interface for sellers to read and reply to buyer conversations. The page has a sidebar listing all conversations and a thread panel showing the selected conversation with a send input. I designed the two-panel layout and the polling approach before writing any code.

I used AI in three focused ways: to draft the TypeScript interface shapes, to generate a Tailwind CSS skeleton for the sidebar and message bubble classes, and to review the useEffect polling implementation for potential issues. In each case I gave AI a specific, narrow prompt and then verified the output before applying it.

The most important part was still manual: understanding the interaction between loadThread, the polling interval, and the mark-as-read side effect. These three things need to stay in sync — if the interval fires while the thread is already loading, it should not cause double-rendering. I verified this by reading the code path carefully and confirming that the setState updater function in the polling callback avoids the stale closure issue.

One thing I had to fix manually was the App.tsx routing — the existing file had missing imports for SiteLayout, ProductBrowsePage, and ProductDetailPage, plus duplicate route definitions. These caused a TypeScript build error that I identified and fixed before the final commit.

### What I learned
- Real-time polling with setInterval works well for low-frequency updates but the cleanup pattern in useEffect is important — missing it causes interval pile-up when the selected conversation changes
- Tailwind's bg-white/10 and border-white/10 utilities are very useful for dark-theme glassmorphism without needing custom CSS
- Splitting the API module into buyer and seller namespaces keeps the chat contract clear even though both sides share send() and markRead()
- AI-generated TypeScript interfaces are a useful starting point but need adjustment once the actual backend response shape is known
