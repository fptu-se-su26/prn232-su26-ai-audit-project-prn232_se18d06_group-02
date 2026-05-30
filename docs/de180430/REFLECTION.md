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
