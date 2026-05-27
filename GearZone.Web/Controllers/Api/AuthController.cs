using GearZone.Application.Abstractions.Services;
using GearZone.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GearZone.Web.Controllers.Api;

[AllowAnonymous]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthController(IAuthService authService, IHttpContextAccessor httpContextAccessor)
    {
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var result = await _authService.LoginAsync(request.Username, request.Password, request.RememberMe);

        if (result.Status == LoginStatus.Success)
        {
            var role = await _authService.GetUserRoleAsync(result.UserId!);
            return OkResponse(new { userId = result.UserId, role }, "Login successful.");
        }

        if (result.Status == LoginStatus.EmailNotConfirmed)
        {
            if (result.UserId != null)
            {
                var callbackUrl = BuildVerifyEmailUrl(result.UserId);
                await _authService.SendVerificationEmailAsync(result.UserId, request.Username, callbackUrl);
                return FailResponse("Email not confirmed. A new verification email has been sent.", 403);
            }
            return FailResponse("Email not confirmed. Please check your inbox.", 403);
        }

        var message = result.Status switch
        {
            LoginStatus.LockedOut => "Account locked due to too many failed attempts.",
            LoginStatus.InvalidCredentials => "Invalid username or password.",
            _ => "Login failed."
        };

        return FailResponse(message, 401);
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var (succeeded, errors, userId) = await _authService.RegisterAsync(
            request.FullName, request.Email, request.Password);

        if (!succeeded || userId == null)
            return FailResponse("Registration failed.", errors);

        var callbackUrl = BuildVerifyEmailUrl(userId);
        await _authService.SendVerificationEmailAsync(userId, request.Email, callbackUrl);

        return OkResponse("Registration successful. Please check your email to verify your account.");
    }

    // POST /api/auth/logout
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.SignOutAsync();
        return OkResponse("Logged out successfully.");
    }

    // GET /api/auth/me
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _authService.GetUserAsync(User);
        if (user == null) return FailResponse("User not found.", 404);
        return OkResponse(user);
    }

    // GET /api/auth/verify-email?userId=&token=
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return FailResponse("Invalid verification link.");

        var result = await _authService.ConfirmEmailAsync(userId, token);
        if (!result)
            return FailResponse("Email verification failed. The link may be expired or invalid.");

        await _authService.SignInAsync(userId);
        return OkResponse("Email verified successfully.");
    }

    // POST /api/auth/resend-verification
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var userId = await _authService.GetUserIdAsync(request.Email);
        if (userId == null)
            return OkResponse("If that email exists, a verification link has been sent.");

        var callbackUrl = BuildVerifyEmailUrl(userId);
        await _authService.SendVerificationEmailAsync(userId, request.Email, callbackUrl);
        return OkResponse("Verification email resent.");
    }

    // GET /api/auth/external-login?provider=Google&returnUrl=
    [HttpGet("external-login")]
    public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string? returnUrl = null)
    {
        var callbackUrl = Url.Action(nameof(GoogleCallback), "Auth",
            new { returnUrl }, Request.Scheme)!;
        var properties = _authService.GetExternalAuthenticationProperties(provider, callbackUrl);
        return Challenge(properties, provider);
    }

    // GET /api/auth/google-callback
    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? returnUrl = null, [FromQuery] string? remoteError = null)
    {
        var frontendBase = Request.Headers["Origin"].FirstOrDefault()
            ?? "http://localhost:5173";

        if (remoteError != null)
            return Redirect($"{frontendBase}/login?error={Uri.EscapeDataString(remoteError)}");

        var (succeeded, error, userId) = await _authService.HandleExternalLoginCallbackAsync();
        if (!succeeded)
            return Redirect($"{frontendBase}/login?error={Uri.EscapeDataString(error ?? "External login failed.")}");

        var role = string.IsNullOrEmpty(userId)
            ? null
            : await _authService.GetUserRoleAsync(userId);

        var redirectPath = role switch
        {
            "Super Admin" => "/admin/dashboard",
            "Store Owner" => "/seller/dashboard",
            _ => returnUrl ?? "/"
        };

        return Redirect($"{frontendBase}{redirectPath}");
    }

    private string BuildVerifyEmailUrl(string userId)
    {
        var token = _authService.GenerateEmailConfirmationTokenAsync(userId).GetAwaiter().GetResult();
        return Url.Action(nameof(VerifyEmail), "Auth",
            new { userId, token }, Request.Scheme)!;
    }
}

public class LoginRequest
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class RegisterRequest
{
    [Required][StringLength(100)] public string FullName { get; set; } = string.Empty;
    [Required][EmailAddress] public string Email { get; set; } = string.Empty;
    [Required][StringLength(100, MinimumLength = 6)] public string Password { get; set; } = string.Empty;
    [Required][Compare(nameof(Password))] public string ConfirmPassword { get; set; } = string.Empty;
}

public class ResendVerificationRequest
{
    [Required][EmailAddress] public string Email { get; set; } = string.Empty;
}
