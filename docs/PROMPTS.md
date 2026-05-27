# AI Prompts Log -- Core -- External Service Integrations

Branch: `core/infrastructure-external-services`
Scope: GearZone.Infrastructure/External: Cloudinary file storage, PayOS payment/payout, SMTP email, Goong map API

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Infrastructure/External: Cloudinary file storage, PayOS payment/payout, SMTP email, Goong map API

**Prompt:**
> How to integrate PayOS payment gateway in ASP.NET Core with webhook signature verification?

**AI Output Summary:**
PayOS SDK: create payment link, handle webhook callback, verify HMAC-SHA256 signature, update order status.

**Used in files:** GearZone.Infrastructure/External/*.cs, Settings/*.cs

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Infrastructure/External: Cloudinary file storage, PayOS payment/payout, SMTP email, Goong map API

**Prompt:**
> Implement Cloudinary image upload in C# with automatic public_id generation and error handling.

**AI Output Summary:**
Cloudinary .NET SDK: Cloudinary.UploadAsync with RawUploadParams; return SecureUrl as stored image URL.

**Used in files:** GearZone.Infrastructure/External/*.cs, Settings/*.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** GearZone.Infrastructure/External: Cloudinary file storage, PayOS payment/payout, SMTP email, Goong map API

**Prompt:**
> How do I calculate shipping distance using the Goong Directions API in .NET?

**AI Output Summary:**
Goong REST API: GET /direction with origin/destination lat-lng; parse routes[0].legs[0].distance.value in meters.

**Used in files:** GearZone.Infrastructure/External/*.cs, Settings/*.cs

---
