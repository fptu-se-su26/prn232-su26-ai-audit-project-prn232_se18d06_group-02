using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Admin.ViewModels;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Users
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)]
        public UserQueryDto Query { get; set; } = new UserQueryDto();

        [BindProperty]
        public CreateUserViewModel CreateUserRequest { get; set; } = new CreateUserViewModel();

        [BindProperty]
        public EditUserViewModel EditUserRequest { get; set; } = new EditUserViewModel();

        public PagedResult<UserViewModel> Users { get; set; } = new PagedResult<UserViewModel>();
        public List<string> Roles { get; set; } = new();
        public UserStatsDto UserStats { get; set; } = new();

        public async Task OnGetAsync(CancellationToken ct)
        {
            await LoadDataAsync(ct);
        }

        public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
        {
            var otherKeys = ModelState.Keys
                .Where(k => !k.StartsWith(nameof(CreateUserRequest) + ".") && k != nameof(CreateUserRequest))
                .ToList();
            foreach (var key in otherKeys)
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                await LoadDataAsync(ct);
                return Page();
            }

            var result = await _api.PostAsync("/api/admin/users", CreateUserRequest, ct);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            TempData["ErrorMessage"] = "Failed to create user. Please check the errors.";

            await LoadDataAsync(ct);
            return Page();
        }

        public async Task<IActionResult> OnPostEditAsync(CancellationToken ct)
        {
            var otherKeys = ModelState.Keys
                .Where(k => !k.StartsWith(nameof(EditUserRequest) + ".") && k != nameof(EditUserRequest))
                .ToList();
            foreach (var key in otherKeys)
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = "Validation failed: " + string.Join(" ", errors);
                await LoadDataAsync(ct);
                return Page();
            }

            var result = await _api.PutAsync("/api/admin/users", EditUserRequest, ct);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToPage();
            }

            var identityErrors = string.Join(" ", result.Errors);
            TempData["ErrorMessage"] = "Failed to update user: " + identityErrors;

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            await LoadDataAsync(ct);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id, CancellationToken ct)
        {
            var result = await _api.DeleteAsync($"/api/admin/users/{Uri.EscapeDataString(id)}", ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestoreAsync(string id, CancellationToken ct)
        {
            var result = await _api.PostAsync($"/api/admin/users/{Uri.EscapeDataString(id)}/restore", ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "User restored successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to restore user.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetUserDetailsAsync(string userId, CancellationToken ct)
        {
            var user = await _api.GetAsync<UserViewModel>(
                $"/api/admin/users/{Uri.EscapeDataString(userId)}", ct);
            if (user == null) return NotFound();

            return new JsonResult(user);
        }

        private async Task LoadDataAsync(CancellationToken ct)
        {
            if (Query.PageNumber < 1) Query.PageNumber = 1;
            if (Query.PageSize < 1) Query.PageSize = 10;

            var data = await _api.GetAsync<AdminUserListResponseDto>(
                $"/api/admin/users{ApiQueryStringBuilder.Build(Query)}", ct);
            if (data is not null)
            {
                Users = data.Users;
                Roles = data.Roles;
                UserStats = data.Stats;
            }
        }
    }
}
