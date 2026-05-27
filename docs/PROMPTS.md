# AI Prompts Log -- Feature -- Store Management

Branch: `feature/store-management`
Scope: Seller store registration, admin approval, store profile, store settings, store follow/unfollow

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Seller store registration, admin approval, store profile, store settings, store follow/unfollow

**Prompt:**
> Design a store registration flow with admin approval where sellers upload identity card images for KYC verification.

**AI Output Summary:**
Store entity with StoreStatus enum (PendingReview, Active, Suspended, Rejected); identity card images stored on Cloudinary; admin sets status.

**Used in files:** Features/Seller/* (store), Controllers/Api/SellerRegistrationController.cs, Pages/StoreOwner/Settings/*, Pages/Admin/StoreApplications/*, Pages/Public/StoreProfile/*

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Seller store registration, admin approval, store profile, store settings, store follow/unfollow

**Prompt:**
> How do I implement a follow system between buyers and stores in EF Core?

**AI Output Summary:**
StoreFollow join entity with UserId and StoreId composite PK; StoreRepository.GetFollowerCountAsync uses COUNT query.

**Used in files:** Features/Seller/* (store), Controllers/Api/SellerRegistrationController.cs, Pages/StoreOwner/Settings/*, Pages/Admin/StoreApplications/*, Pages/Public/StoreProfile/*

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Seller store registration, admin approval, store profile, store settings, store follow/unfollow

**Prompt:**
> What store settings should a multi-vendor marketplace expose to sellers?

**AI Output Summary:**
Store settings: display name, bio, logo, banner, bank account (for payouts), address, geo-coordinates for shipping.

**Used in files:** Features/Seller/* (store), Controllers/Api/SellerRegistrationController.cs, Pages/StoreOwner/Settings/*, Pages/Admin/StoreApplications/*, Pages/Public/StoreProfile/*

---
