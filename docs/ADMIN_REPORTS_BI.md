# Admin Reports / Business Intelligence v1

The Super Admin report page is available at `/admin/reports`. `GearZone.Api` and `GearZone.Web` must both be running because Razor consumes the protected API through the shared authentication cookie.

## Endpoints

- `GET /api/admin/reports/overview`
- `GET /api/admin/reports/orders`
- `GET /api/admin/reports/sellers`
- `GET /api/admin/reports/{tab}/export?format=csv|xlsx|pdf`
- `GET /api/admin/reports/{tab}/insights`
- `POST /api/admin/reports/{tab}/insights?forceRefresh=false`

All endpoints require the `Super Admin` role. Report queries accept `range`, `from`, `to`, `granularity`, and the seller-specific search, status, sort, and paging parameters.

## AI configuration

Copy the relevant values from `.env.example` into a local `.env`, environment variables, or .NET user secrets. Do not commit provider keys.

```text
AI_INSIGHTS_ENABLED=true
AI_PROVIDER=OpenAI
AI_TIMEOUT_SECONDS=30
OPENAI_API_KEY=...
OPENAI_MODEL=gpt-5.6-luna
```

Set `AI_PROVIDER=Gemini` and the corresponding `GEMINI_*` variables to use Gemini. There is no automatic provider failover. Reports and exports remain available when AI is disabled or fails.

## Verification

```powershell
dotnet test GearZone.Tests/GearZone.Tests.csproj
dotnet build GearZone.sln
```

QuestPDF is configured with its Community license for this academic project. Re-evaluate that license before commercial deployment.
