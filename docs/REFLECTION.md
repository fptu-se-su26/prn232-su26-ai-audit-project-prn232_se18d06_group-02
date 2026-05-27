# Reflection 

Branch: `core/application-abstractions`
Date: 2026-05-27
Primary owner: Nguyen Sinh Nhat (DE180430)

---

## What went well

Defining all interfaces before implementations enforced the Dependency Inversion Principle throughout the project. AI helped identify missing interfaces (e.g. IOrderTrackingNotifier for SignalR) that were initially overlooked. Some suggested interfaces were too fine-grained and were merged.
External service integrations were the most time-consuming part of the infrastructure. AI provided working code samples for each API but webhook signature verification required careful manual testing. The Disabled* stub pattern (suggested by AI) was very useful for local development without real API credentials.

AI accelerated the design and scaffolding phase significantly. The team spent more time on
business logic validation and testing rather than boilerplate.

## What was challenging

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
