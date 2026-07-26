using System.Globalization;
using ClosedXML.Excel;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;

namespace GearZone.Infrastructure.External;

/// <summary>
/// Bulk product import from an .xlsx file (ClosedXML). One spreadsheet row = one variant;
/// rows sharing a ProductName are grouped into a single product. Validation is read-only
/// (preview); creation reuses <see cref="ISellerProductService.CreateProductAsync"/> so all
/// the normal rules (SKU/slug uniqueness, moderation status) apply. Imported products land
/// as Draft with no images — the seller reviews and completes them afterwards.
/// </summary>
public sealed class ProductImportService : IProductImportService
{
    private readonly ISellerProductService _products;

    public ProductImportService(ISellerProductService products)
    {
        _products = products;
    }

    // Spreadsheet column headers (row 1). Matched case-insensitively, spaces ignored.
    private static readonly string[] Headers =
    {
        "ProductName", "Category", "Brand", "Description", "BasePrice", "ImageUrls",
        "VariantName", "SKU", "Price", "Stock"
    };

    private const int MaxImagesPerProduct = 5;

    // ---------------------------------------------------------------- Template
    public async Task<byte[]> GenerateTemplateAsync(CancellationToken ct = default)
    {
        var categories = await _products.GetCategoriesAsync();
        var brands = await _products.GetBrandsAsync();
        return BuildTemplate(
            categories.Select(c => c.Name).OrderBy(n => n).ToList(),
            brands.Select(b => b.Name).OrderBy(n => n).ToList());
    }

    private static byte[] BuildTemplate(IReadOnlyList<string> categories, IReadOnlyList<string> brands)
    {
        using var wb = new XLWorkbook();

        var ws = wb.Worksheets.Add("Products");
        for (int i = 0; i < Headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = Headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1A56DB");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Two example rows: one product ("Gaming Mouse X") with two variants.
        object[,] examples =
        {
            { "Gaming Mouse X", categories.FirstOrDefault() ?? "Mouse", brands.FirstOrDefault() ?? "Logitech", "RGB wired gaming mouse", 350000, "https://picsum.photos/seed/mouse1/600 | https://picsum.photos/seed/mouse2/600", "Black", "GMX-BLK", 350000, 25 },
            { "Gaming Mouse X", "", "", "", "", "", "White", "GMX-WHT", 360000, 10 }
        };
        for (int r = 0; r < 2; r++)
            for (int c = 0; c < Headers.Length; c++)
                ws.Cell(2 + r, c + 1).Value = XLCellValue.FromObject(examples[r, c]);

        ws.Row(1).SetAutoFilter();
        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();
        ws.Column(4).Width = 30; // Description

        // Instructions sheet.
        var help = wb.Worksheets.Add("Instructions");
        var lines = new[]
        {
            "HOW TO IMPORT PRODUCTS",
            "",
            "1. Fill the 'Products' sheet. One row = one variant.",
            "2. To add several variants to the same product, repeat the SAME ProductName on each row.",
            "   Category / Brand / Description / BasePrice / ImageUrls only need to be filled on the FIRST row of a product.",
            "3. Category and Brand must match a name from the 'Valid Categories' / 'Valid Brands' sheets exactly.",
            "4. SKU must be unique across the whole system.",
            "5. Price and Stock are required on every row. Price must be greater than 0.",
            "6. ImageUrls (optional): one or more public image URLs separated by '|' (max 5).",
            "   The images are fetched and re-hosted on the store's Cloudinary; a bad URL is skipped.",
            "",
            "Imported products are created as DRAFT. Open each product afterwards to review and submit.",
            "",
            "Delete these example rows before importing your own data."
        };
        for (int i = 0; i < lines.Length; i++)
        {
            help.Cell(i + 1, 1).Value = lines[i];
            if (i == 0) help.Cell(i + 1, 1).Style.Font.Bold = true;
        }
        help.Column(1).Width = 100;

        var catRange = AddListSheet(wb, "Valid Categories", categories);
        var brandRange = AddListSheet(wb, "Valid Brands", brands);

        // Turn the Category (col 2) and Brand (col 3) columns into in-cell dropdowns bound to the
        // reference sheets, so the seller picks a valid value instead of typing it. Named ranges are
        // used because Excel needs them for cross-sheet list validation. Blanks stay allowed so a
        // variant row can leave these empty and inherit from the product's first row.
        const int lastDataRow = 1000;
        if (catRange != null)
        {
            catRange.AddToNamed("CategoryList", XLScope.Workbook);
            var dv = ws.Range(2, 2, lastDataRow, 2).CreateDataValidation();
            dv.List("=CategoryList", true);
            dv.IgnoreBlanks = true;
            dv.ErrorTitle = "Invalid category";
            dv.ErrorMessage = "Pick a category from the list (see the 'Valid Categories' sheet).";
        }
        if (brandRange != null)
        {
            brandRange.AddToNamed("BrandList", XLScope.Workbook);
            var dv = ws.Range(2, 3, lastDataRow, 3).CreateDataValidation();
            dv.List("=BrandList", true);
            dv.IgnoreBlanks = true;
            dv.ErrorTitle = "Invalid brand";
            dv.ErrorMessage = "Pick a brand from the list (see the 'Valid Brands' sheet).";
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static IXLRange? AddListSheet(XLWorkbook wb, string title, IReadOnlyList<string> values)
    {
        var ws = wb.Worksheets.Add(title);
        ws.Cell(1, 1).Value = title;
        ws.Cell(1, 1).Style.Font.Bold = true;
        for (int i = 0; i < values.Count; i++)
            ws.Cell(i + 2, 1).Value = values[i];
        ws.Column(1).AdjustToContents();
        return values.Count > 0 ? ws.Range(2, 1, values.Count + 1, 1) : null;
    }

    // ----------------------------------------------------------------- Preview
    public async Task<ProductImportPreviewDto> PreviewAsync(byte[] fileBytes, Guid storeId, CancellationToken ct = default)
    {
        var products = await ValidateAsync(fileBytes, storeId);

        var rows = products
            .SelectMany(p => p.Rows.Select(r => new ProductImportRowResultDto
            {
                RowNumber = r.RowNumber,
                ProductName = p.Name,
                Category = p.CategoryName,
                Brand = p.BrandName,
                VariantName = r.VariantName,
                Sku = r.Sku,
                Price = r.Price,
                Stock = r.Stock,
                IsValid = r.IsValid,
                Action = !r.IsValid ? string.Empty
                       : r.Action == "Restock" ? "Restock"
                       : p.IsExisting ? "New variant"
                       : "New product",
                Errors = r.Errors
            }))
            .OrderBy(r => r.RowNumber)
            .ToList();

        return new ProductImportPreviewDto
        {
            TotalRows = rows.Count,
            ValidRows = rows.Count(r => r.IsValid),
            InvalidRows = rows.Count(r => !r.IsValid),
            ProductCount = products.Count(p => !p.IsExisting && p.Rows.Any(r => r.IsValid && r.Action == "New")),
            NewVariants = rows.Count(r => r.Action == "New variant"),
            Restocks = rows.Count(r => r.Action == "Restock"),
            Rows = rows
        };
    }

    // ------------------------------------------------------------------ Import
    public async Task<ProductImportResultDto> ImportAsync(byte[] fileBytes, Guid storeId, string userId, CancellationToken ct = default)
    {
        var products = await ValidateAsync(fileBytes, storeId);
        var result = new ProductImportResultDto();

        foreach (var p in products)
        {
            var validRows = p.Rows.Where(r => r.IsValid).ToList();
            var invalidCount = p.Rows.Count - validRows.Count;
            if (validRows.Count == 0)
            {
                result.RowsSkipped += p.Rows.Count;
                continue;
            }

            var restockRows = validRows.Where(r => r.Action == "Restock" && r.ExistingVariantId.HasValue).ToList();
            var newRows = validRows.Where(r => r.Action == "New").ToList();

            try
            {
                // Existing SKUs → add the file quantity to current stock.
                foreach (var r in restockRows)
                {
                    await _products.RestockVariantAsync(r.ExistingVariantId!.Value, r.Stock, userId);
                    result.VariantsRestocked++;
                }

                // New SKUs → add to the existing product, or create a brand-new product.
                if (newRows.Count > 0)
                {
                    if (p.IsExisting)
                    {
                        foreach (var r in newRows)
                        {
                            await _products.AddVariantToProductAsync(p.ExistingProductId, r.VariantName, r.Sku, r.Price, r.Stock, userId);
                            result.VariantsCreated++;
                        }
                    }
                    else
                    {
                        var dto = new CreateProductDto
                        {
                            Name = p.Name,
                            Description = p.Description,
                            CategoryId = p.CategoryId,
                            BrandId = p.BrandId,
                            BasePrice = p.BasePrice,
                            IsDraft = true,
                            ImageUrls = p.ImageUrls,
                            Variants = newRows.Select(v => new ProductVariantDto
                            {
                                VariantName = v.VariantName,
                                Sku = v.Sku,
                                Price = v.Price,
                                StockQuantity = v.Stock
                            }).ToList()
                        };
                        await _products.CreateProductAsync(dto, storeId, userId);
                        result.ProductsCreated++;
                        result.VariantsCreated += newRows.Count;
                    }
                }

                result.RowsSkipped += invalidCount;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{p.Name}: {ex.Message}");
                result.RowsSkipped += p.Rows.Count;
            }
        }

        return result;
    }

    // -------------------------------------------------------------- Validation
    private sealed class ValidatedRow
    {
        public int RowNumber { get; set; }
        public string VariantName { get; set; } = "Default";
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsValid { get; set; }
        public string Action { get; set; } = "New"; // "New" | "Restock"
        public Guid? ExistingVariantId { get; set; }
        public List<string> Errors { get; } = new();
    }

    private sealed class ValidatedProduct
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public bool IsExisting { get; set; }
        public Guid ExistingProductId { get; set; }
        public List<string> ImageUrls { get; } = new();
        public List<string> ProductErrors { get; } = new();
        public List<ValidatedRow> Rows { get; } = new();
    }

    private async Task<List<ValidatedProduct>> ValidateAsync(byte[] fileBytes, Guid storeId)
    {
        var raw = Parse(fileBytes);

        var categories = await _products.GetCategoriesAsync();
        var brands = await _products.GetBrandsAsync();
        var catByName = categories
            .GroupBy(c => Norm(c.Name)).ToDictionary(g => g.Key, g => g.First());
        var brandByName = brands
            .GroupBy(b => Norm(b.Name)).ToDictionary(g => g.Key, g => g.First());

        // File-wide SKU checks.
        var allSkus = raw.Where(r => !string.IsNullOrWhiteSpace(r.Sku))
            .Select(r => r.Sku!.Trim()).ToList();
        var dupSkusInFile = allSkus
            .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingSkus = await _products.GetExistingSkusAsync(allSkus);          // any store
        var storeVariants = await _products.GetStoreVariantsBySkuAsync(storeId, allSkus); // this store → restock target

        // Group rows into products by product name (rows with a blank name become their own error rows).
        var groups = new List<(string Key, List<ProductImportRawRow> Rows)>();
        var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in raw)
        {
            var name = (r.ProductName ?? string.Empty).Trim();
            var key = string.IsNullOrEmpty(name) ? $"__blank__{r.RowNumber}" : Norm(name);
            if (!index.TryGetValue(key, out var i))
            {
                index[key] = groups.Count;
                groups.Add((key, new List<ProductImportRawRow>()));
            }
            groups[index[key]].Rows.Add(r);
        }

        // Slugs, to detect in-store duplicates and in-file collisions.
        var slugByGroup = groups.ToDictionary(
            g => g.Key,
            g => Slugify((g.Rows[0].ProductName ?? string.Empty).Trim()));
        var storeProducts = await _products.GetStoreProductIdsBySlugAsync(storeId, slugByGroup.Values);
        var dupSlugsInFile = slugByGroup.Values
            .Where(s => !string.IsNullOrEmpty(s))
            .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var results = new List<ValidatedProduct>();
        foreach (var (key, rows) in groups)
        {
            var vp = new ValidatedProduct();
            var name = (rows[0].ProductName ?? string.Empty).Trim();
            vp.Name = name;
            vp.Slug = Slugify(name);
            vp.Description = FirstNonEmpty(rows, r => r.Description) ?? string.Empty;

            // Existing product (same name/slug already in the store) → we add variants / restock to it.
            if (!string.IsNullOrEmpty(name) && storeProducts.TryGetValue(vp.Slug, out var existingProductId))
            {
                vp.IsExisting = true;
                vp.ExistingProductId = existingProductId;
            }

            // ---- Classify each variant row first (Restock / New / error) so we know whether a new
            // product will actually be created before deciding if category/brand are required.
            foreach (var r in rows)
            {
                var vr = new ValidatedRow { RowNumber = r.RowNumber };
                var vn = (r.VariantName ?? string.Empty).Trim();
                vr.VariantName = string.IsNullOrEmpty(vn) ? "Default" : vn;

                vr.Sku = (r.Sku ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(vr.Sku))
                    vr.Errors.Add("SKU is required.");
                else if (dupSkusInFile.Contains(vr.Sku))
                    vr.Errors.Add($"SKU '{vr.Sku}' is duplicated in the file.");
                else if (storeVariants.TryGetValue(vr.Sku, out var existingVariant))
                {
                    vr.Action = "Restock"; // same SKU already in this store → add the file quantity
                    vr.ExistingVariantId = existingVariant.VariantId;
                }
                else if (existingSkus.Contains(vr.Sku))
                    vr.Errors.Add($"SKU '{vr.Sku}' belongs to another seller.");

                if (!TryParseDecimal(r.Price, out var price) || price <= 0)
                    vr.Errors.Add("Price must be a number greater than 0.");
                vr.Price = price;

                if (!TryParseInt(r.Stock, out var stock) || stock < 0)
                    vr.Errors.Add("Stock must be a whole number of 0 or more.");
                vr.Stock = stock;

                vp.Rows.Add(vr);
            }

            // ---- Product-name checks (apply to new-SKU rows only; restocks are keyed by SKU).
            if (string.IsNullOrEmpty(name))
                vp.ProductErrors.Add("Product name is required.");
            else if (dupSlugsInFile.Contains(vp.Slug))
                vp.ProductErrors.Add($"Product name '{name}' is duplicated in the file.");

            // Category / brand / base price are only needed when this group actually creates a NEW
            // product (new name + at least one otherwise-valid new-SKU variant). A group that only
            // restocks, or adds variants to an existing product, doesn't need them.
            var createsNewProduct = !vp.IsExisting && vp.Rows.Any(r => r.Action == "New" && r.Errors.Count == 0);
            if (createsNewProduct)
            {
                var catText = FirstNonEmpty(rows, r => r.Category);
                if (string.IsNullOrWhiteSpace(catText))
                    vp.ProductErrors.Add("Category is required.");
                else if (catByName.TryGetValue(Norm(catText), out var cat))
                {
                    vp.CategoryId = cat.Id;
                    vp.CategoryName = cat.Name;
                }
                else
                    vp.ProductErrors.Add($"Category '{catText}' was not found.");

                var brandText = FirstNonEmpty(rows, r => r.Brand);
                if (string.IsNullOrWhiteSpace(brandText))
                    vp.ProductErrors.Add("Brand is required.");
                else if (brandByName.TryGetValue(Norm(brandText), out var brand))
                {
                    vp.BrandId = brand.Id;
                    vp.BrandName = brand.Name;
                }
                else
                    vp.ProductErrors.Add($"Brand '{brandText}' was not found.");

                var basePriceText = FirstNonEmpty(rows, r => r.BasePrice);
                if (!string.IsNullOrWhiteSpace(basePriceText))
                {
                    if (TryParseDecimal(basePriceText, out var bp) && bp >= 0)
                        vp.BasePrice = bp;
                    else
                        vp.ProductErrors.Add("Base price must be a number.");
                }
                else
                {
                    var newPrices = vp.Rows.Where(r => r.Action == "New" && r.Errors.Count == 0).Select(r => r.Price).ToList();
                    vp.BasePrice = newPrices.Count > 0 ? newPrices.Min() : 0m;
                }

                // Image URLs (product-level, first non-empty in the group): split on '|', keep http(s), cap at 5.
                var imageText = FirstNonEmpty(rows, r => r.ImageUrls);
                if (!string.IsNullOrWhiteSpace(imageText))
                {
                    vp.ImageUrls.AddRange(imageText
                        .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Where(u => u.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        .Take(MaxImagesPerProduct));
                }
            }

            // Product-level errors invalidate the new-SKU rows (restocks depend only on SKU + qty).
            foreach (var vr in vp.Rows)
            {
                if (vr.Action == "New")
                    vr.Errors.AddRange(vp.ProductErrors);
                vr.IsValid = vr.Errors.Count == 0;
            }

            results.Add(vp);
        }

        return results;
    }

    // ------------------------------------------------------------- Xlsx parsing
    private static List<ProductImportRawRow> Parse(byte[] fileBytes)
    {
        using var ms = new MemoryStream(fileBytes);
        using var wb = new XLWorkbook(ms);

        // Read the "Products" sheet by name; fall back to the first sheet for older/edited files.
        var ws = wb.Worksheets.FirstOrDefault(s => string.Equals(s.Name, "Products", StringComparison.OrdinalIgnoreCase))
                 ?? wb.Worksheets.FirstOrDefault();
        if (ws == null) return new List<ProductImportRawRow>();

        var headerRow = ws.FirstRowUsed();
        if (headerRow == null) return new List<ProductImportRawRow>();
        var headerRowNumber = headerRow.RowNumber();

        // Map header text -> absolute worksheet column number.
        var colOf = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.CellsUsed())
        {
            var h = Norm(cell.GetString());
            if (!string.IsNullOrEmpty(h) && !colOf.ContainsKey(h))
                colOf[h] = cell.Address.ColumnNumber;
        }

        string Get(IXLRow row, string header) =>
            colOf.TryGetValue(Norm(header), out var col) ? row.Cell(col).GetString().Trim() : string.Empty;

        var rows = new List<ProductImportRawRow>();
        foreach (var row in ws.RowsUsed().Where(r => r.RowNumber() > headerRowNumber))
        {
            var raw = new ProductImportRawRow
            {
                RowNumber = row.RowNumber(),
                ProductName = Get(row, "ProductName"),
                Category = Get(row, "Category"),
                Brand = Get(row, "Brand"),
                Description = Get(row, "Description"),
                BasePrice = Get(row, "BasePrice"),
                ImageUrls = Get(row, "ImageUrls"),
                VariantName = Get(row, "VariantName"),
                Sku = Get(row, "SKU"),
                Price = Get(row, "Price"),
                Stock = Get(row, "Stock")
            };

            // Skip completely empty rows.
            if (string.IsNullOrWhiteSpace(raw.ProductName) && string.IsNullOrWhiteSpace(raw.Sku) &&
                string.IsNullOrWhiteSpace(raw.VariantName) && string.IsNullOrWhiteSpace(raw.Price) &&
                string.IsNullOrWhiteSpace(raw.Stock))
                continue;

            rows.Add(raw);
        }

        return rows;
    }

    // ---------------------------------------------------------------- Helpers
    private static string Norm(string? s) =>
        (s ?? string.Empty).Trim().Replace(" ", string.Empty).ToLowerInvariant();

    private static string Slugify(string name) =>
        (name ?? string.Empty).ToLower().Replace(" ", "-");

    private static string? FirstNonEmpty(IEnumerable<ProductImportRawRow> rows, Func<ProductImportRawRow, string?> pick) =>
        rows.Select(pick).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim();

    private static bool TryParseDecimal(string? s, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(s)) return false;
        var cleaned = s.Trim().Replace(",", string.Empty).Replace(" ", string.Empty);
        return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseInt(string? s, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(s)) return false;
        var cleaned = s.Trim().Replace(",", string.Empty).Replace(" ", string.Empty);
        // Accept "10" and "10.0".
        if (int.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) return true;
        if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) && d == Math.Floor(d))
        {
            value = (int)d;
            return true;
        }
        return false;
    }
}
