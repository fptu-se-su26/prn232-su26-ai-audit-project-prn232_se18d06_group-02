using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Domain.Entities;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Admin.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Users
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminUserService _adminUserService;
        private readonly IMapper _mapper;

        public IndexModel(
            IAdminUserService adminUserService,
            IMapper mapper)
        {
            _adminUserService = adminUserService;
            _mapper = mapper;
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

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
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
                await LoadDataAsync();
                return Page();
            }

            var dto = _mapper.Map<CreateUserDto>(CreateUserRequest);
            var result = await _adminUserService.CreateUserAsync(dto);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            TempData["ErrorMessage"] = "Failed to create user. Please check the errors.";

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEditAsync()
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
                await LoadDataAsync();
                return Page();
            }

            var dto = _mapper.Map<EditUserDto>(EditUserRequest);
            var result = await _adminUserService.UpdateUserAsync(dto);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToPage();
            }

            var identityErrors = string.Join(" ", result.Errors.Select(e => e.Description));
            TempData["ErrorMessage"] = "Failed to update user: " + identityErrors;

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var result = await _adminUserService.SoftDeleteUserAsync(id, User.Identity!.Name ?? "System");
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestoreAsync(string id)
        {
            var result = await _adminUserService.RestoreUserAsync(id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User restored successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to restore user.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetUserDetailsAsync(string userId)
        {
            var user = await _adminUserService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            var viewModel = _mapper.Map<UserViewModel>(user);
            return new JsonResult(viewModel);
        }

        private async Task LoadDataAsync()
        {
            if (Query.PageNumber < 1) Query.PageNumber = 1;
            if (Query.PageSize < 1) Query.PageSize = 10;

            var domainPagedResult = await _adminUserService.GetPaginatedUsersAsync(Query);

            var viewModels = _mapper.Map<List<UserViewModel>>(domainPagedResult.Items);

            Users = new PagedResult<UserViewModel>(
                viewModels,
                domainPagedResult.TotalCount,
                domainPagedResult.PageNumber,
                domainPagedResult.PageSize
            );

            Roles = await _adminUserService.GetAllRolesAsync();
            UserStats = await _adminUserService.GetUserStatsAsync();
        }
    }
}
