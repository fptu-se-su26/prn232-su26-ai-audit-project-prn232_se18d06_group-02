using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Public.Auth
{
    public class VerifyEmailModel : PageModel
    {
        private readonly IAuthService _authService;

        public VerifyEmailModel(IAuthService authService)
        {
            _authService = authService;
        }

        public string? StatusMessage { get; set; }
        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Index");
            }

            var result = await _authService.ConfirmEmailAsync(userId, token);
            if (result)
            {
                await _authService.SignInAsync(userId);
                return RedirectToPage("/Index");
            }
            else
            {
                StatusMessage = "Error confirming your email. The link may be expired or invalid.";
                IsSuccess = false;
                return Page();
            }
        }
    }
}
