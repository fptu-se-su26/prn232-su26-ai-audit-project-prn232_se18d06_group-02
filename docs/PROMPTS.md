# AI Prompts Log -- Feature -- Seller Product Management

Branch: `feature/product-management`
Scope: Seller CRUD for products: create with variants/images/attributes, update, soft-delete, status management, inventory tracking

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Seller CRUD for products: create with variants/images/attributes, update, soft-delete, status management, inventory tracking

**Prompt:**
> Design a product creation flow where the seller defines product variants as a matrix of attribute combinations (e.g. Size x Color).

**AI Output Summary:**
Store attribute options; generate ProductVariant records from all combinations; allow seller to set price/stock per variant.

**Used in files:** Features/Seller/* (products), Controllers/Api/Seller/ProductsController.cs, Pages/StoreOwner/Products/*

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Seller CRUD for products: create with variants/images/attributes, update, soft-delete, status management, inventory tracking

**Prompt:**
> How should I implement soft delete for products in EF Core while keeping variant and image data intact?

**AI Output Summary:**
Set ProductStatus = Inactive on soft delete; global query filter WHERE ProductStatus != Deleted; keep related data intact.

**Used in files:** Features/Seller/* (products), Controllers/Api/Seller/ProductsController.cs, Pages/StoreOwner/Products/*

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Seller CRUD for products: create with variants/images/attributes, update, soft-delete, status management, inventory tracking

**Prompt:**
> What is the right way to track inventory changes -- update stock directly or use transaction records?

**AI Output Summary:**
InventoryTransaction records with TransactionType (StockIn, StockOut, Adjustment); derive current stock by summing transactions.

**Used in files:** Features/Seller/* (products), Controllers/Api/Seller/ProductsController.cs, Pages/StoreOwner/Products/*

---
