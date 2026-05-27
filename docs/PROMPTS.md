# AI Prompts Log -- Feature -- Product Reviews

Branch: `feature/product-reviews`
Scope: Buyer product reviews after order delivery: rating, comment, seller reply, admin moderation

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Buyer product reviews after order delivery: rating, comment, seller reply, admin moderation

**Prompt:**
> How do I gate product reviews behind order completion -- only buyers who purchased can review?

**AI Output Summary:**
Check SubOrder.Status=Completed AND SubOrder contains the reviewed product for the requesting user before allowing review creation.

**Used in files:** Features/Reviews/*, Controllers/Api/ReviewsController.cs, Pages/StoreOwner/Reviews/*, Repositories/ProductReviewRepository.cs

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Buyer product reviews after order delivery: rating, comment, seller reply, admin moderation

**Prompt:**
> Design a review system with seller reply functionality and admin moderation for an e-commerce platform.

**AI Output Summary:**
ProductReview.SellerReplyText + SellerRepliedAt; seller can only reply to reviews for their own store products.

**Used in files:** Features/Reviews/*, Controllers/Api/ReviewsController.cs, Pages/StoreOwner/Reviews/*, Repositories/ProductReviewRepository.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Buyer product reviews after order delivery: rating, comment, seller reply, admin moderation

**Prompt:**
> How do I efficiently compute average product rating while keeping it accurate as reviews are added/deleted?

**AI Output Summary:**
Compute AVG(Rating) in SQL via repository query; do not cache -- consistency is more important than marginal performance at this scale.

**Used in files:** Features/Reviews/*, Controllers/Api/ReviewsController.cs, Pages/StoreOwner/Reviews/*, Repositories/ProductReviewRepository.cs

---
