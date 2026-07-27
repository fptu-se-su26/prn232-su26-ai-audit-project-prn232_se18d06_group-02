# GearZone OData API

GearZone exposes a read-only OData endpoint for the public product catalog:

```http
GET /odata/CatalogProducts
```

Only active, non-deleted products are returned. The endpoint supports `$select`,
`$filter`, `$orderby`, `$top`, `$skip`, and `$count`. Server-driven paging uses
20 items per page and `$top` is limited to 100.

Examples:

```http
GET /odata/CatalogProducts?$filter=Price le 1000000 and InStock eq true
GET /odata/CatalogProducts?$orderby=SoldCount desc&$top=10
GET /odata/CatalogProducts?$select=Name,Slug,Price,BrandName
GET /odata/CatalogProducts?$count=true&$top=20
GET /odata/$metadata
```
