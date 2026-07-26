using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Public.User
{
    [Authorize]
    public class RegisterSellerModel : PageModel
    {
        private const string ProgressPath = "/api/seller-registration/progress";

        // Consumes GearZone.Api over HTTP instead of the seller store service in-process.
        private readonly IApiClient _api;
        private readonly IConfiguration _configuration;

        public RegisterSellerModel(IApiClient api, IConfiguration configuration)
        {
            _api = api;
            _configuration = configuration;
        }

        [BindProperty]
        public Step1Dto Step1Input { get; set; } = new();

        [BindProperty]
        public Step2Dto Step2Input { get; set; } = new();

        [BindProperty]
        public Step3Dto Step3Input { get; set; } = new();

        public int CurrentStep { get; set; } = 1;
        public Guid? StoreId { get; set; }
        public RegistrationProgressDto? Progress { get; set; }
        public StoreStatus? ExistingStoreStatus { get; set; }
        public string? ExistingStoreRejectReason { get; set; }
        public string GoongMapKey => _configuration["GOONG_MAP_KEY"] ?? "";

        public async Task<IActionResult> OnGetAsync(bool reapply = false, CancellationToken ct = default)
        {
            var state = await _api.GetAsync<SellerRegistrationStateDto>(ProgressPath, ct);

            ExistingStoreStatus = state?.ExistingStore?.Status;
            ExistingStoreRejectReason = state?.ExistingStore?.RejectReason;

            if (reapply && state?.ExistingStore is { Status: StoreStatus.Pending or StoreStatus.Rejected })
            {
                var reapplyResult = await _api.PostAsync("/api/seller-registration/reapply", ct);
                if (!reapplyResult.Success)
                {
                    TempData["ErrorMessage"] = reapplyResult.FirstError;
                    return RedirectToPage("/Public/User/Profile");
                }

                TempData["InfoMessage"] = "Your application has been reopened for editing.";
                return RedirectToPage(new { step = 1 });
            }

            if (state?.ExistingStore != null && state.ExistingStore.Status != StoreStatus.Draft)
            {
                if (state.ExistingStore.Status == StoreStatus.Approved)
                    return RedirectToPage("/StoreOwner/Dashboard");
                if (state.ExistingStore.Status == StoreStatus.Pending)
                {
                    TempData["InfoMessage"] = "Your registration is pending approval.";
                    return RedirectToPage("/Public/User/Profile");
                }
            }

            // Load draft progress
            if (state?.Progress != null)
            {
                Progress = ToProgressDto(state.Progress);
                CurrentStep = Progress.CurrentStep;
                StoreId = Progress.StoreId;
                Step1Input = Progress.Step1;
                Step2Input = Progress.Step2;
                Step3Input = Progress.Step3;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostStep1Async(CancellationToken ct = default)
        {
            var result = await _api.PostAsync("/api/seller-registration/step1", Step1Input, ct);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.FirstError ?? "Could not save step 1.");
                CurrentStep = 1;
                return Page();
            }

            return RedirectToPage(new { step = 2 });
        }

        public async Task<IActionResult> OnPostStep2Async(CancellationToken ct = default)
        {
            if (!await LoadProgressAsync(ct)) return RedirectToPage();

            // Step 2 carries the ID card uploads, so it goes over multipart/form-data.
            using var form = new MultipartFormDataContent();
            AddField(form, nameof(Step2Dto.FullName), Step2Input.FullName);
            AddField(form, nameof(Step2Dto.IdentityNumber), Step2Input.IdentityNumber);
            AddField(form, nameof(Step2Dto.IdentityIssuedDate), Step2Input.IdentityIssuedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            AddField(form, nameof(Step2Dto.IdentityIssuedPlace), Step2Input.IdentityIssuedPlace);
            AddField(form, nameof(Step2Dto.TaxCode), Step2Input.TaxCode);
            AddField(form, nameof(Step2Dto.IdentityCardFrontImageUrl), Step2Input.IdentityCardFrontImageUrl);
            AddField(form, nameof(Step2Dto.IdentityCardBackImageUrl), Step2Input.IdentityCardBackImageUrl);
            AddFile(form, nameof(Step2Dto.IdentityCardFrontImage), Step2Input.IdentityCardFrontImage);
            AddFile(form, nameof(Step2Dto.IdentityCardBackImage), Step2Input.IdentityCardBackImage);

            var result = await _api.PostFormAsync("/api/seller-registration/step2", form, ct);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.FirstError ?? "Could not save step 2.");
                CurrentStep = 2;
                return Page();
            }

            return RedirectToPage(new { step = 3 });
        }

        public async Task<IActionResult> OnPostStep3Async(CancellationToken ct = default)
        {
            if (!await LoadProgressAsync(ct)) return RedirectToPage();

            var result = await _api.PostAsync("/api/seller-registration/step3", Step3Input, ct);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.FirstError ?? "Could not save step 3.");
                CurrentStep = 3;
                return Page();
            }

            return RedirectToPage(new { step = 4 });
        }

        public async Task<IActionResult> OnPostSubmitAsync(CancellationToken ct = default)
        {
            if (!await LoadProgressAsync(ct)) return RedirectToPage();

            var result = await _api.PostAsync("/api/seller-registration/submit", ct);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.FirstError ?? "Could not submit the registration.");
                CurrentStep = 4;
                return Page();
            }

            TempData["SuccessMessage"] = "Your registration has been submitted successfully! We will review and respond as soon as possible.";
            return RedirectToPage("/Public/User/Profile");
        }

        public async Task OnGetLoadStepAsync(int step, CancellationToken ct = default)
        {
            var state = await _api.GetAsync<SellerRegistrationStateDto>(ProgressPath, ct);
            if (state?.Progress != null)
            {
                Progress = ToProgressDto(state.Progress);
                StoreId = Progress.StoreId;
                Step1Input = Progress.Step1;
                Step2Input = Progress.Step2;
                Step3Input = Progress.Step3;
            }
            CurrentStep = step;
        }

        /// <summary>
        /// Populates <see cref="Progress"/> and reports whether a draft store exists to save into.
        /// Leaves the bound inputs alone so a failed post redisplays what the user typed.
        /// </summary>
        private async Task<bool> LoadProgressAsync(CancellationToken ct)
        {
            var state = await _api.GetAsync<SellerRegistrationStateDto>(ProgressPath, ct);
            Progress = state?.Progress == null ? null : ToProgressDto(state.Progress);
            return Progress?.StoreId != null;
        }

        private static RegistrationProgressDto ToProgressDto(SellerRegistrationProgressStateDto state) => new()
        {
            StoreId = state.StoreId,
            CurrentStep = state.CurrentStep,
            Step1 = state.Step1,
            Step2 = state.Step2.ToInput(),
            Step3 = state.Step3
        };

        private static void AddField(MultipartFormDataContent form, string name, string? value)
        {
            if (!string.IsNullOrEmpty(value)) form.Add(new StringContent(value), name);
        }

        private static void AddFile(MultipartFormDataContent form, string name, IFormFile? file)
        {
            if (file == null || file.Length == 0) return;

            var content = new StreamContent(file.OpenReadStream());
            if (!string.IsNullOrEmpty(file.ContentType))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            }
            form.Add(content, name, file.FileName);
        }
    }
}
