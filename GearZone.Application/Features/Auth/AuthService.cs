using GearZone.Application.Abstractions.Services;
using GearZone.Application.Abstractions.External;
using GearZone.Domain.Entities;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Auth
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public AuthService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<LoginResponse> LoginAsync(string username, string password, bool rememberMe)
        {
            var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);
            if (user == null) return new LoginResponse { Status = LoginStatus.InvalidCredentials };

            var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
                return new LoginResponse { Status = LoginStatus.Success, UserId = user.Id };

            if (result.IsNotAllowed)
                return new LoginResponse { Status = LoginStatus.EmailNotConfirmed, UserId = user.Id };

            if (result.IsLockedOut)
                return new LoginResponse { Status = LoginStatus.LockedOut, UserId = user.Id };

            return new LoginResponse { Status = LoginStatus.InvalidCredentials };
        }

        public async Task SignInAsync(string userId, bool isPersistent = false)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent);
            }
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<UserDto?> GetUserAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                AvatarUrl = user.AvatarUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<string?> GetUserRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }

        public async Task<string?> GetUserIdAsync(string emailOrUsername)
        {
            var user = await _userManager.FindByNameAsync(emailOrUsername) ?? await _userManager.FindByEmailAsync(emailOrUsername);
            return user?.Id;
        }

        public async Task<(bool Succeeded, string[] Errors, string? UserId)> RegisterAsync(string fullName, string email, string password)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return (false, new[] { $"Email {email} is already taken." }, null);
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                return (true, Array.Empty<string>(), user.Id);
            }

            return (false, result.Errors.Select(e => e.Description).ToArray(), null);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return string.Empty;
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }

        public async Task SendVerificationEmailAsync(string userId, string email, string callbackUrl)
        {
            var encodedUrl = HtmlEncoder.Default.Encode(callbackUrl);

            var htmlBody = $@"
<div style=""font-family: 'Inter', Helvetica, Arial, sans-serif; background-color: #F8FAFC; padding: 40px 20px; color: #1E293B;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);"">
        <!-- Header -->
        <div style=""background-color: #1A56DB; padding: 30px; text-align: center;"">
            <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 700; letter-spacing: -0.025em;"">GearZone</h1>
        </div>
        
        <!-- Content -->
        <div style=""padding: 40px 30px;"">
            <h2 style=""margin-top: 0; color: #0F172A; font-size: 20px; font-weight: 600;"">Confirm your email address</h2>
            <p style=""font-size: 16px; line-height: 1.6; color: #475569; margin-bottom: 30px;"">
                Welcome to GearZone! We're excited to have you on board. To get started and secure your account, please verify your email address by clicking the button below.
            </p>
            
            <div style=""text-align: center; margin-bottom: 30px;"">
                <a href=""{encodedUrl}"" style=""display: inline-block; background-color: #1A56DB; color: #ffffff; padding: 14px 28px; border-radius: 8px; font-weight: 600; text-decoration: none; border: none; box-shadow: 0 4px 6px -1px rgba(26, 86, 219, 0.2);"">
                    Verify Email Address
                </a>
            </div>
            
            <hr style=""border: 0; border-top: 1px solid #E2E8F0; margin: 40px 0;"" />
            
            <p style=""font-size: 12px; line-height: 1.5; color: #94A3B8; margin-top: 0;"">
                If you didn't create an account with GearZone, you can safely ignore this email.
            </p>
        </div>
        
        <!-- Footer -->
        <div style=""background-color: #F1F5F9; padding: 20px 30px; text-align: center;"">
            <p style=""margin: 0; font-size: 12px; color: #64748B;"">
                &copy; 2024 GearZone Inc. All rights reserved.
            </p>
        </div>
    </div>
</div>";

            await _emailService.SendAsync(email, "Welcome to GearZone - Confirm Your Email", htmlBody);
        }

        public AuthenticationProperties GetExternalAuthenticationProperties(string provider, string redirectUrl)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }

        public async Task<(bool Succeeded, string? Error, string? UserId)> HandleExternalLoginCallbackAsync()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return (false, "Error loading external login information.", null);
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                return (true, null, user?.Id);
            }

            var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        EmailConfirmed = true
                    };
                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        return (false, string.Join(", ", createResult.Errors.Select(e => e.Description)), null);
                    }
                    await _userManager.AddToRoleAsync(user, "Customer");
                }

                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return (true, null, user.Id);
                }
            }

            return (false, "Error during external login.", null);
        }
    }
}

