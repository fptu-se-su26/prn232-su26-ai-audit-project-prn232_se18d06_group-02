using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.Services
{
    public enum LoginStatus
    {
        Success,
        EmailNotConfirmed,
        LockedOut,
        InvalidCredentials,
        Failure
    }

    public class LoginResponse
    {
        public LoginStatus Status { get; set; }
        public string? UserId { get; set; }
    }

    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string username, string password, bool rememberMe);
        Task<(bool Succeeded, string[] Errors, string? UserId)> RegisterAsync(string fullName, string email, string password);
        Task<string> GenerateEmailConfirmationTokenAsync(string userId);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task SignInAsync(string userId, bool isPersistent = false);
        Task SignOutAsync();
        Task<UserDto?> GetUserAsync(ClaimsPrincipal user);
        Task<string?> GetUserRoleAsync(string userId);
        Task<string?> GetUserIdAsync(string emailOrUsername);
        Task SendVerificationEmailAsync(string userId, string email, string callbackUrl);
        AuthenticationProperties GetExternalAuthenticationProperties(string provider, string redirectUrl);
        Task<(bool Succeeded, string? Error, string? UserId)> HandleExternalLoginCallbackAsync();
    }
}
