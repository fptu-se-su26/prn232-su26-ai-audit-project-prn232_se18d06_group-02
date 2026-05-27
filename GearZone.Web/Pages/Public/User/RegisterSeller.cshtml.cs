using GearZone.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Public.User
{
    [Authorize]
    public class RegisterSellerModel : PageModel
    {
        private readonly ISellerStoreService _sellerStoreService;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public RegisterSellerModel(ISellerStoreService sellerStoreService, IAuthService authService, IConfiguration configuration)
        {
            _sellerStoreService = sellerStoreService;
            _authService = authService;
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

        public async Task<IActionResult> OnGetAsync(bool reapply = false)
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Public/Auth/Login");

            // Check if user already has an approved/pending store
            var existingStore = await _sellerStoreService.GetStoreByOwnerIdAsync(user.Id);
            ExistingStoreStatus = existingStore?.Status;
            ExistingStoreRejectReason = existingStore?.RejectReason;

            if (reapply && existingStore is { Status: StoreStatus.Pending or StoreStatus.Rejected })
            {
                try
                {
                    await _sellerStoreService.StartReapplicationAsync(user.Id);
                    TempData["InfoMessage"] = "Your application has been reopened for editing.";
                    return RedirectToPage(new { step = 1 });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToPage("/Public/User/Profile");
                }
            }

            if (existingStore != null && existingStore.Status != StoreStatus.Draft)
            {
                if (existingStore.Status == StoreStatus.Approved)
                    return RedirectToPage("/StoreOwner/Dashboard");
                if (existingStore.Status == StoreStatus.Pending)
                {
                    TempData["InfoMessage"] = "Your registration is pending approval.";
                    return RedirectToPage("/Public/User/Profile");
                }
            }

            // Load draft progress
            Progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);
            if (Progress != null)
            {
                CurrentStep = Progress.CurrentStep;
                StoreId = Progress.StoreId;
                Step1Input = Progress.Step1;
                Step2Input = Progress.Step2;
                Step3Input = Progress.Step3;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostStep1Async()
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Public/Auth/Login");

            try
            {
                var storeId = await _sellerStoreService.SaveStep1Async(user.Id, Step1Input);
                return RedirectToPage(new { step = 2 });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CurrentStep = 1;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostStep2Async()
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Public/Auth/Login");

            Progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);
            if (Progress?.StoreId == null) return RedirectToPage();

            try
            {
                await _sellerStoreService.SaveStep2Async(Progress.StoreId.Value, user.Id, Step2Input);
                return RedirectToPage(new { step = 3 });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CurrentStep = 2;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostStep3Async()
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Public/Auth/Login");

            Progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);
            if (Progress?.StoreId == null) return RedirectToPage();

            try
            {
                await _sellerStoreService.SaveStep3Async(Progress.StoreId.Value, Step3Input);
                return RedirectToPage(new { step = 4 });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CurrentStep = 3;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostSubmitAsync()
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Public/Auth/Login");

            Progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);
            if (Progress?.StoreId == null) return RedirectToPage();

            try
            {
                await _sellerStoreService.SubmitRegistrationAsync(Progress.StoreId.Value, user.Id);
                TempData["SuccessMessage"] = "Your registration has been submitted successfully! We will review and respond as soon as possible.";
                return RedirectToPage("/Public/User/Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CurrentStep = 4;
                return Page();
            }
        }

        public async Task OnGetLoadStepAsync(int step)
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null) return;

            Progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);
            if (Progress != null)
            {
                StoreId = Progress.StoreId;
                Step1Input = Progress.Step1;
                Step2Input = Progress.Step2;
                Step3Input = Progress.Step3;
            }
            CurrentStep = step;
        }
    }
}
