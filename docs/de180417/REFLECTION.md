# REFLECTION.md

## Reflection - React Admin Catalog and Marketing Modules

In this phase, I expanded the React admin workspace with three new operational modules: Category Management, Brand Management, and Voucher Management. These modules cover catalog structure, brand metadata, and platform discount campaigns. The goal was to make each area usable from the React admin shell with protected routes, typed API access, clear list workflows, and focused create/edit experiences.

The Category Management module required the most structured data handling. Categories are not just flat records; admins need to understand parent and child relationships, expand roots, inspect product counts, and manage status. The create and edit workflows also needed attribute configuration, including attribute input types, filterability, and ordered option values. Building that editor in React made the state model explicit: category fields, parent selection, visibility, and attributes all update independently before being saved through typed API calls.

The Brand Management module focused on quick catalog administration. The page includes summary statistics, approval filtering, search, pagination, logo display, create/edit modals, approve actions, and delete confirmation. One important backend support change was accepting multipart form data for brand create and update requests so admins can use either uploaded logo files or external logo URLs. Keeping both options in one form made the workflow flexible without adding a separate asset-management screen.

The Voucher Management module was the most interaction-heavy. It combines campaign monitoring with campaign creation. The list page uses KPI cards, status tabs, search, filters, sorting, ticket-style rows, pagination, duplicate actions, and status toggles. The create/edit workflow includes code generation, voucher type selection, category restriction, discount logic, usage limits, lifecycle dates, visibility, validation, and real-time preview. This made it important to keep validation close to the form so invalid discount percentages, fixed discount rules, or invalid date ranges are caught before API calls.

The main lesson from this phase was that admin modules need both visual clarity and strong data contracts. Category attributes, brand logo sources, and voucher enum values can become fragile if the frontend guesses too much. Adding narrow backend API support helped the React pages stay honest: category attributes can be persisted, brand logos can be submitted as form data, and voucher KPI data can be loaded from the same list workflow. Typed API definitions in `admin.ts` made those contracts easier to review.

Verification was done with production frontend builds and isolated backend compilation. The normal backend output can be locked by a running development server, so compiling the web project to a temporary output folder was the safer verification path. This confirmed the controller changes compiled without requiring the running server process to be stopped during documentation and review.

### What I learned
- Hierarchical catalog data needs UI affordances for scanning, expansion, and summary counts
- Attribute editors are easier to maintain when attribute rows and option rows have explicit local state
- Brand logo workflows should support both file upload and URL entry when admins may not have a local image asset
- Voucher campaign screens benefit from a real-time preview because discount logic is easier to verify visually
- Enum values from the backend should be mapped through typed frontend constants instead of string guesses
- Small backend API additions are worthwhile when they complete a React workflow cleanly
- Frontend validation should catch business rules before sending mutation requests
- Build verification with a temporary backend output folder avoids false failures from locked development DLLs

## Reflection - Expanded React Admin Management Modules

In this phase, I continued building GearZone's admin area as a new React-based management workspace. The added work covered Store Management, User Management, Order Management, and Product Management. Each module was designed as part of the same admin experience: protected routes, a shared layout, typed API access, clear data-loading states, table workflows, and detail screens where the admin needs more context before making a decision.

The Store Management screen focuses on active store operations. It gives admins a way to search and review approved stores, check summary statistics, filter the list, open store profiles, and change store status through backend-supported actions. A small backend API adjustment was needed so the frontend could request active stores and store statistics directly.

The User Management screen focuses on account administration. The important part was keeping the workflow practical: stats cards for quick overview, search and role filters for discovery, active/inactive filtering, pagination, and modal-based create/edit actions. Soft delete and restore behavior were kept separate so account state changes remain explicit.

The Order Management feature was split into a list page and a detail page. The list page supports search, payment status filtering, date and total filters, sorting, pagination, and row navigation. The detail page focuses on inspection: customer information, store-grouped sub-orders, product line items, financial values, payment information, shipping details, and order history. I kept this module read-focused because no new order mutation endpoint was included in the feature scope.

The Product Management feature required the most action handling. The list page includes statistics, search, quick filters, advanced filters, table sorting, pagination, single-row actions, and bulk actions. The detail page includes gallery images, category/status badges, brand and SKU data, store linking, technical specifications, variants, commercial insights, description, store profile information, and a sticky admin action bar. Product approve, reject, suspend, delete, and bulk status actions use confirmation or reason modals before calling the API.

The biggest lesson from this phase was that admin modules become easier to reason about when each screen has a clear job. List screens should help users find and compare records quickly. Detail screens should show enough context to support a decision. Modals should only ask for information that is required by the action. Typed API clients helped keep these screens consistent because query parameters, payloads, and response shapes were visible in one shared module.

Verification was done with local build commands. The database was restored by applying the available Entity Framework database updates, the backend solution was compiled after the store API change, and the React frontend was checked with `npm run build`. The development servers were available at `http://localhost:5107` for the backend and `http://localhost:5173` for the frontend.

### What I learned
- Admin list pages need fast filtering, stable pagination, and clear empty states
- Admin detail pages should be decision-focused and expose action controls only when the current status allows them
- Shared typed API modules reduce repeated request mapping code across admin pages
- Bulk actions need careful selection clearing after a list reload to avoid acting on stale rows
- Reason modals make reject, suspend, and delete actions safer and easier to audit
- Backend endpoint gaps should be filled narrowly, only for data the React screen actually needs
- Some UI controls can remain placeholders when no matching API exists, but they should be visibly non-destructive
- Running the production frontend build is the fastest way to catch unused helpers and TypeScript mismatches in new screens

## Reflection - Admin Dashboard and Store Applications

For this task, I worked on new admin-facing functionality for GearZone in the React frontend. The main focus was building a complete Admin Platform Overview dashboard and Store Applications management screens. The dashboard gives admin users a centralized view of platform metrics, charts, store performance, user growth, and dispute summaries. The Store Applications feature supports the review workflow through a list page, detail page, status display, document preview, and approve/reject/request-info actions.

The most useful part of using AI was getting structured help across multiple connected files. The work touched API typing, protected routing, reusable layout, page-level state management, data formatting, status handling, and backend endpoint coverage. Having a clear implementation checklist helped keep the feature organized and prevented the admin pages from becoming isolated screens.

I still needed to review the output manually. In particular, I checked that the frontend routes used the existing authentication guard, that StoreStatus values matched the backend enum values, that the approve/reject/request-info requests used the expected API payloads, and that export buttons remained visual placeholders because export endpoints were not included in the feature scope.

For the Store Applications feature, the most important design decision was separating the list workflow from the review workflow. The list page focuses on finding applications quickly through summary cards, search, filters, and pagination. The detail page focuses on decision making: admins can review business information, owner identity data, banking information, document images, and history before taking action.

The review actions also needed extra care. Approving can be a direct confirmation, but rejection requires a clear reason and request-info requires a clear note. Adding quick reason chips, a character limit, and required-field validation made the admin workflow safer and easier to test. Refreshing the detail data after each action was important because the available buttons depend on the latest application status.

One important debugging point was the admin redirect issue. The browser showed that the user was authenticated, but the current-user response returned `role: null`. Because the React route guard depends on the role value, authenticated admin users were redirected to the home page. The fix was to load the user's Identity role in the backend current-user service and return it to the frontend.

Verification was also important. The frontend was checked with `npm.cmd run lint` and `npm.cmd run build`. The backend web project was checked with a temporary output folder because the running `GearZone.Web` process locked DLL files in the normal build output. This confirmed that the new Store Applications stats endpoint compiled successfully.

### What I learned
- Admin pages should share a consistent layout instead of duplicating sidebar and header structure
- Typed API clients make dashboard and admin management screens easier to validate
- Role-based routing depends on accurate current-user data, not only a valid login session
- Numeric enum values from the backend need explicit frontend labels and styling helpers
- Action modals should include validation before sending approve/reject/request-info requests
- Store application review screens need both a searchable list flow and a decision-focused detail flow
- Detail pages should refresh after state-changing actions so role/status-based controls remain correct
- Backend-compatible query names reduce the risk of filter and pagination mismatches
- Build verification can require a temporary output folder when a running ASP.NET process locks normal build artifacts
- AI is most useful when the prompt describes the intended new feature, route behavior, API contract, and verification steps clearly

## Reflection - Admin Reports and Business Intelligence

This feature connected business formulas, database aggregation, API design, file generation, AI privacy, and Razor UI into one administrative workflow. The most important design decision was separating deterministic reporting from optional AI analysis: report JSON and exports continue to work even if AI is disabled, misconfigured, rate-limited, or unavailable.

The period resolver also showed why reporting dates cannot be handled as plain UTC calendar dates. Admins select dates in Vietnam time, while database queries need UTC half-open intervals. Centralizing that conversion, the previous period, granularity selection, and zero-filled buckets reduces inconsistent numbers between tabs and exports.

AI required an additional trust boundary. Only aggregate metrics, trends, status breakdowns, and seller performance summaries are included. Provider output is structured, capped, and filtered so every retained item cites an allowed metric key. This does not eliminate the need for human review, but it prevents unsupported AI commentary from being displayed as evidence-based analysis.

### What I learned
- Revenue and operational rates require different status filters and denominators.
- Parent orders must be counted distinctly when an order contains suborders from multiple stores.
- Report exports should be deterministic and independent from AI cache state.
- Provider keys and business payloads should never appear in logs or committed configuration.
- SQLite integration tests are useful for catching provider-specific EF Core translation issues before deployment.
- A production rollout should still inspect SQL query plans, validate the configured model names, and perform authenticated browser smoke tests with realistic data volumes.
