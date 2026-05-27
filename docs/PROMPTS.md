# AI Prompts Log -- Feature -- User Profile and Account

Branch: `feature/user-profile`
Scope: Buyer profile: view/edit personal info, avatar upload, saved delivery addresses, wallet balance view, order history

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Buyer profile: view/edit personal info, avatar upload, saved delivery addresses, wallet balance view, order history

**Prompt:**
> Design a user profile system for an e-commerce buyer that includes delivery address management with a default address selection.

**AI Output Summary:**
UserAddress table with IsDefault boolean; ensure only one IsDefault per user via EF transaction (unset all, then set selected).

**Used in files:** Features/User/*, Controllers/Api/UsersController.cs, Pages/Public/User/*, Repositories/UserRepository.cs, Repositories/UserAddressRepository.cs

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Buyer profile: view/edit personal info, avatar upload, saved delivery addresses, wallet balance view, order history

**Prompt:**
> How do I validate and upload a user avatar image in ASP.NET Core Razor Pages with size and type restrictions?

**AI Output Summary:**
Accept image/jpeg, image/png, image/webp; max 5MB; resize to 256x256 before Cloudinary upload to save storage.

**Used in files:** Features/User/*, Controllers/Api/UsersController.cs, Pages/Public/User/*, Repositories/UserRepository.cs, Repositories/UserAddressRepository.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Buyer profile: view/edit personal info, avatar upload, saved delivery addresses, wallet balance view, order history

**Prompt:**
> What information should a buyer's wallet transaction history show?

**AI Output Summary:**
Show: transaction type, amount (+/-), balance after, reference (order/payout ID), timestamp.

**Used in files:** Features/User/*, Controllers/Api/UsersController.cs, Pages/Public/User/*, Repositories/UserRepository.cs, Repositories/UserAddressRepository.cs

---
