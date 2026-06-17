# REFLECTION.md

## Reflection - Customer Profile Account Center

For this task, I built a new customer profile account center for the GearZone React frontend. The feature brings together several customer self-service workflows: account information, order history, buyer messages, saved addresses, review history, password updates, seller application status, and seller registration.

The main challenge was making the profile page work as a complete user flow rather than a static screen. The frontend needed to coordinate multiple API areas, including user data, order summaries, review history, address CRUD, chat inbox behavior, map autocomplete, and seller registration. Each part had its own response shape and payload requirements, so the React page needed careful state management and accurate TypeScript models.

AI assistance was useful for moving quickly across the frontend and backend boundaries. It helped draft the account center structure, connect API calls, add route registration, and identify small backend gaps such as profile update and password change endpoints. I still needed to verify the implementation through builds because small contract details, such as `{ summary, orders }` response data, `phoneNumber` naming, multipart identity-card upload, and enum values, can break the user flow even when the UI looks correct.

This feature also showed that profile pages can become central navigation points for many related workflows. A customer expects to move from profile to tracking, reviewing products, editing addresses, opening messages, or starting seller registration without hitting dead routes or placeholder UI.

### What I learned
- Customer profile pages should be treated as complete workflow hubs, not simple account-detail screens.
- React frontend code must match backend DTO field names and response shapes exactly.
- Route registration is part of feature completeness; a screen is not usable if users cannot navigate to its related actions.
- File upload flows need special API binding and frontend request handling.
- AI can accelerate multi-area implementation, but final correctness still depends on local build checks and careful manual review.
