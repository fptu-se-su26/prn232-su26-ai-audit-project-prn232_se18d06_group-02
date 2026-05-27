# AI Prompts Log -- Feature -- Shipping and Logistics

Branch: `feature/shipping-logistics`
Scope: Shipping cost calculation via Goong distance API, shipment tracking, estimated delivery, address management

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Shipping cost calculation via Goong distance API, shipment tracking, estimated delivery, address management

**Prompt:**
> How do I calculate shipping cost based on real road distance using the Goong Directions API in ASP.NET Core?

**AI Output Summary:**
Goong /direction endpoint: origin=store_coords&destination=buyer_coords&vehicle=car; parse routes[0].legs[0].distance.value; apply rate per km.

**Used in files:** Features/Shipping/*, Features/Map/*, Infrastructure/External/GoongService.cs, Infrastructure/Jobs/OrderAutoCompleteJob.cs

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Shipping cost calculation via Goong distance API, shipment tracking, estimated delivery, address management

**Prompt:**
> Design a shipment tracking system where sellers update the shipment status and buyers see real-time updates.

**AI Output Summary:**
Shipment entity tracks code, carrier, status, estimatedDelivery; seller updates via API; SignalR notifies buyer.

**Used in files:** Features/Shipping/*, Features/Map/*, Infrastructure/External/GoongService.cs, Infrastructure/Jobs/OrderAutoCompleteJob.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Shipping cost calculation via Goong distance API, shipment tracking, estimated delivery, address management

**Prompt:**
> How do I geocode a Vietnamese address to lat/long using the Goong Geocoding API?

**AI Output Summary:**
Goong /geocode endpoint: address string -> location.lat/lng; fallback to manual entry if geocode fails.

**Used in files:** Features/Shipping/*, Features/Map/*, Infrastructure/External/GoongService.cs, Infrastructure/Jobs/OrderAutoCompleteJob.cs

---
