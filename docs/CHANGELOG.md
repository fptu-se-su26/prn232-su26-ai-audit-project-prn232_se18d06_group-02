# Changelog -- Feature -- Product Reviews

All notable changes on branch `feature/product-reviews` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Buyer product reviews after order delivery: rating, comment, seller reply, admin moderation

### Added
- ProductReview entity with Rating (1-5), Comment, optional SellerReply, ReviewStatus
- Review submission gated behind order delivery: buyer must have Completed SubOrder for the product
- Review listing on product detail page with pagination and average rating aggregation
- Seller reply to review via StoreOwner/Reviews page
- Admin review moderation: flag/hide inappropriate reviews
- IProductReviewService contract and implementation

### Changed
- Adapted existing code patterns to align with Clean Architecture conventions

### Fixed
- N/A (initial implementation on this branch)

### Notes
- All changes target `develop` as the merge destination
- No direct commits to `main`

---

## Previous Releases
See `main` branch CHANGELOG for project-level release history.
