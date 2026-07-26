using System.Net.Http.Headers;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Products
{
    [Authorize(Roles = "Store Owner")]
    public class ImportModel : PageModel
    {
        private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IApiClient _api;

        public ImportModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public IFormFile? Upload { get; set; }

        // Base64 of the uploaded file, carried in a hidden field so "Confirm" can commit
        // the exact file the seller previewed without re-selecting it.
        [BindProperty]
        public string? FileData { get; set; }

        public ProductImportPreviewDto? Preview { get; private set; }
        public ProductImportResultDto? Result { get; private set; }
        public string? Error { get; private set; }

        public void OnGet() { }

        // Downloads the fill-in template.
        public async Task<IActionResult> OnGetTemplateAsync(CancellationToken ct)
        {
            var file = await _api.GetFileAsync("/api/seller/products/import/template", ct);
            return File(file.Content, file.ContentType, file.FileName);
        }

        public async Task<IActionResult> OnPostPreviewAsync(CancellationToken ct)
        {
            if (Upload == null || Upload.Length == 0)
            {
                Error = "Please choose an .xlsx file first.";
                return Page();
            }

            var bytes = await ToBytesAsync(Upload, ct);
            FileData = Convert.ToBase64String(bytes);

            var res = await _api.PostFormAndReadAsync<ProductImportPreviewDto>(
                "/api/seller/products/import/preview", BuildForm(bytes, Upload.FileName), ct);

            if (!res.Success)
            {
                Error = res.FirstError ?? "Could not read the file.";
                return Page();
            }

            Preview = res.Data;
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(CancellationToken ct)
        {
            if (string.IsNullOrEmpty(FileData))
            {
                Error = "The upload expired — please choose the file again.";
                return Page();
            }

            byte[] bytes;
            try { bytes = Convert.FromBase64String(FileData); }
            catch { Error = "The uploaded data was invalid — please upload again."; return Page(); }

            var res = await _api.PostFormAndReadAsync<ProductImportResultDto>(
                "/api/seller/products/import", BuildForm(bytes, "import.xlsx"), ct);

            if (!res.Success)
            {
                Error = res.FirstError ?? "Import failed.";
                return Page();
            }

            Result = res.Data;
            return Page();
        }

        private static async Task<byte[]> ToBytesAsync(IFormFile file, CancellationToken ct)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            return ms.ToArray();
        }

        private static MultipartFormDataContent BuildForm(byte[] bytes, string? fileName)
        {
            var form = new MultipartFormDataContent();
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue(XlsxContentType);
            form.Add(content, "file", string.IsNullOrWhiteSpace(fileName) ? "import.xlsx" : fileName);
            return form;
        }
    }
}
