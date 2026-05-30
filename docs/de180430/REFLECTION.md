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

## Reflection - Order Tracking and Review

For this task, I implemented the order tracking page with a visual timeline and the product review page with a star rating widget. AI generated the initial CSS for the timeline and the star rating component, which I then reviewed and adjusted.

The order tracking timeline required careful CSS positioning for the connecting line between status dots. Using relative/absolute positioning kept the line aligned with each dot. The star rating widget used simple Unicode characters with React hover state for the preview effect.

### What I learned
- The order tracking timeline required careful CSS positioning for the connecting line between status dots
- The star rating widget was built with simple Unicode characters and React hover state
- Consistent Tailwind patterns across pages make the overall UI feel cohesive
