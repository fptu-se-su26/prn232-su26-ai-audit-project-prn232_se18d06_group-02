# AI Prompts Log -- Feature -- Product Catalog Browsing

Branch: `feature/catalog-browsing`
Scope: Public product listing, search, filtering by category/brand/price/rating, product detail page, store profile browsing

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Public product listing, search, filtering by category/brand/price/rating, product detail page, store profile browsing

**Prompt:**
> Design a composable product filtering system in EF Core that supports category, brand, price range, and custom attributes without N+1 queries.

**AI Output Summary:**
Specification pattern with IQueryable<Product> chaining; ProjectTo<ProductDto> with AutoMapper to avoid SELECT *.

**Used in files:** Features/Catalog/*, Controllers/Api/CatalogController.cs, Pages/Public/Catalog/*, Application/Common/ProductSpecifications/

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Public product listing, search, filtering by category/brand/price/rating, product detail page, store profile browsing

**Prompt:**
> How do I implement cursor-based pagination in ASP.NET Core for a product listing API?

**AI Output Summary:**
Keyset pagination using (LastProductId, LastCreatedAt) tuple; more performant than OFFSET for large datasets.

**Used in files:** Features/Catalog/*, Controllers/Api/CatalogController.cs, Pages/Public/Catalog/*, Application/Common/ProductSpecifications/

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Public product listing, search, filtering by category/brand/price/rating, product detail page, store profile browsing

**Prompt:**
> What is the best way to render a product variant selector from a database-driven attribute system?

**AI Output Summary:**
Attribute matrix: load all CategoryAttribute + ProductAttributeValue; render dropdowns client-side from the data.

**Used in files:** Features/Catalog/*, Controllers/Api/CatalogController.cs, Pages/Public/Catalog/*, Application/Common/ProductSpecifications/

---
