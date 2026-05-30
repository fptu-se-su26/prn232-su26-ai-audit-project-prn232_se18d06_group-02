# Reflection

Branch: `core/infrastructure-repositories`
Date: 2026-05-27
Primary owner: Dang Cong Quoc Khanh (DE180880)

Branch: `core/logging-infrastructure`
Date: 2026-05-27
Primary owner: Ho Huy Hoang (DE180416)

# Reflection 

Branch: `core/infrastructure-ef-config`
Date: 2026-05-27
Primary owner: Phan Tran Cong Vu (DE180494)

Branch: `core/domain-entities`
Date: 2026-05-27
Primary owner: Dam Nguyen Khang (DE180417)

Branch: `core/application-abstractions`
Date: 2026-05-27
Primary owner: Nguyen Sinh Nhat (DE180430)

---

## What went well

The generic repository eliminated about 80% of CRUD boilerplate. AI correctly identified that exposing IQueryable from repositories is pragmatic at this project scale. For larger systems, the Specification pattern would be preferable.
The IAppLogger<T> abstraction keeps Application services testable without Serilog. The middleware approach for HTTP logging is cleaner than per-controller logging. Audit trail structured logging is directly importable into Seq/Elasticsearch for compliance reporting.
EF Core configuration is verbose but critical. AI saved significant time on relationship configuration syntax. However, several AI-suggested cascade delete rules had to be changed after causing FK constraint violations during integration testing. Always verify cascade behavior manually.
AI was most useful for validating aggregate boundary decisions. The Order/SubOrder split was debated; AI provided the pattern used by large e-commerce platforms. The final model was simplified from AI suggestions -- several over-engineered value objects were removed.
Defining all interfaces before implementations enforced the Dependency Inversion Principle throughout the project. AI helped identify missing interfaces (e.g. IOrderTrackingNotifier for SignalR) that were initially overlooked. Some suggested interfaces were too fine-grained and were merged.
External service integrations were the most time-consuming part of the infrastructure. AI provided working code samples for each API but webhook signature verification required careful manual testing. The Disabled* stub pattern (suggested by AI) was very useful for local development without real API credentials.

AI accelerated the design and scaffolding phase significantly. The team spent more time on
business logic validation and testing rather than boilerplate.

## What was challenging

- Adapting AI suggestions to the existing codebase conventions required careful review
- Some AI-generated code used outdated API patterns that needed updating to .NET 8
- Edge cases (concurrency, error handling, validation) always required manual additions
- Integrating AI-generated components with the rest of the system needed extra attention


- Adapting AI suggestions to the existing codebase conventions required careful review
- Some AI-generated code used outdated API patterns that needed updating to .NET 8
- Edge cases (concurrency, error handling, validation) always required manual additions
- Integrating AI-generated components with the rest of the system needed extra attention

## AI assistance level for this branch

AI provided meaningful assistance for architecture design and initial implementation.
All AI-generated code was reviewed, tested, and often modified before use.
The team retains full understanding and ownership of every file in this branch.

## Key learnings

1. AI is best used for scaffolding and pattern identification; domain-specific business rules require human judgement.
2. Always run AI-generated code -- syntax correctness does not imply logical correctness.
3. The more specific and contextual the prompt, the more accurate the AI output.
4. AI suggestions for external API integrations should always be cross-referenced against official documentation.

## For future iterations

- Add unit tests for the service layer on this branch
- Consider Specification pattern for more complex queries
- Review AI suggestions against OWASP Top 10 for security-sensitive code paths

---

## Current Session Reflection -- 2026-05-28

This feature was implemented as a new frontend module inside the current project, so the right approach was to build a shared shell architecture instead of trying to force everything into a single generic dashboard.

Backend verification mattered here. Source inspection showed that role-seeded authentication and dedicated Admin and Seller APIs already exist, so the frontend shell could be wired safely around those routes. The Staff role is present in identity seeding, but there is no dedicated Staff controller group yet, so that part remains a frontend scaffold only.

The login redirect change was necessary because the backend callback still routes Store Owner users to `/seller/dashboard` and Admin users to `/admin/dashboard`. Adding compatibility aliases kept the existing backend behavior working without forcing a backend change.

The main lesson is that shared shell work should be implemented as a routing and layout problem first, then connected to backend capabilities only where the API contract is already real. That kept the change minimal, testable, and easy to verify with a successful production build.

---

## Order Tracking + Review UI Clone Reflection -- 2026-05-30

The order tracking page's visual timeline (connected dots with status colors) was the most complex UI element in this PR. The star rating widget in the review page was straightforward — simple hover state management with React useState.

Both pages follow the same pattern as PR 1: API module → Page component → Route registration. This consistent pattern made the clone process fast and predictable.

One adjustment: the OrderSuccessPage in PR 1 extracted orderId from the URL path instead of location.state, which is more user-friendly (shareable URLs). This pattern was carried forward to the tracking and review pages as well.
