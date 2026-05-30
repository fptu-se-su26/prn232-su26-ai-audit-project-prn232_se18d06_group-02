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

## Reflection - User Profile Page

For this task, I implemented the user profile page with a tabbed interface for order history and address management. AI generated the initial profile page layout and address form structure, which I then reviewed and adjusted.

The profile page required managing two separate data views (orders and addresses) within a single page. Using URL parameters for tab state allows deep linking to specific sections.

### What I learned
- The profile page required managing two separate data views within a single page
- Using URL parameters for tab state allows deep linking to specific sections
- The address form used a grid layout for responsive design
