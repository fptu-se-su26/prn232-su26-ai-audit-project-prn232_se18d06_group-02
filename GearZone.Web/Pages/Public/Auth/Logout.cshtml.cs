using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Public.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;

        public LogoutModel(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await _authService.SignOutAsync();
            return RedirectToPage("/Public/Auth/Login");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _authService.SignOutAsync();
            return RedirectToPage("/Public/Auth/Login");
        }
    }
}
