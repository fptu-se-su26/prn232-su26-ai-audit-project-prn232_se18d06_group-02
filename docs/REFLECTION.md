# Reflection -- Feature -- Messaging and Chat

Branch: `feature/messaging-chat`
Date: 2026-05-27
Primary owner: Dam Nguyen Khang (DE180417)

---

## What went well

SignalR's user-based group pattern (Clients.User) was new to the team. AI explained that user groups work automatically with the default UserIdProvider -- no manual group management needed. The read receipt implementation required careful thought to avoid N+1 when displaying inbox unread counts.

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
