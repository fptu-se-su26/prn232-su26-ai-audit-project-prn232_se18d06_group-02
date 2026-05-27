# AI Prompts Log -- Feature -- Authentication and Identity

Branch: `feature/auth-identity`
Scope: User registration (multi-step + email verification), login, JWT issuance, ASP.NET Core Identity configuration

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** User registration (multi-step + email verification), login, JWT issuance, ASP.NET Core Identity configuration

**Prompt:**
> Implement multi-step user registration in ASP.NET Core with email OTP verification using a stateless JWT approach.

**AI Output Summary:**
Store pending registration in a short-lived temp record; OTP generated with RandomNumberGenerator; verified then promoted to confirmed user.

**Used in files:** Features/Auth/*, Controllers/Api/AuthController.cs, Pages/Public/Auth/*, Infrastructure/Repositories/UserRepository.cs

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** User registration (multi-step + email verification), login, JWT issuance, ASP.NET Core Identity configuration

**Prompt:**
> How do I configure ASP.NET Core Identity with a custom ApplicationUser that adds extra profile fields?

**AI Output Summary:**
IdentityDbContext<ApplicationUser>; ApplicationUser extends IdentityUser with DisplayName, AvatarUrl, WalletBalance, etc.

**Used in files:** Features/Auth/*, Controllers/Api/AuthController.cs, Pages/Public/Auth/*, Infrastructure/Repositories/UserRepository.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** User registration (multi-step + email verification), login, JWT issuance, ASP.NET Core Identity configuration

**Prompt:**
> Generate a secure OTP email verification flow -- how long should tokens be valid and how should they be stored?

**AI Output Summary:**
6-digit numeric OTP stored hashed; 15-minute expiry; resend after 60-second cooldown to prevent flooding.

**Used in files:** Features/Auth/*, Controllers/Api/AuthController.cs, Pages/Public/Auth/*, Infrastructure/Repositories/UserRepository.cs

---
