# Changelog -- Feature -- Authentication and Identity

All notable changes on branch `feature/auth-identity` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
User registration (multi-step + email verification), login, JWT issuance, ASP.NET Core Identity configuration

### Added
- Multi-step registration flow: basic info -> email OTP verification -> profile completion
- JWT Bearer token issuance with configurable expiry and refresh logic
- Email verification via SMTP OTP with 15-minute TTL
- AuthController endpoints: /register, /login, /verify-email, /resend-verification
- LoginViewModel, RegisterViewModel with validation attributes
- IAuthService contract and implementation
- ASP.NET Core Identity configuration with custom ApplicationUser

### Changed
- Adapted existing code patterns to align with Clean Architecture conventions

### Fixed
- N/A (initial implementation on this branch)

### Notes
- All changes target `develop` as the merge destination
- No direct commits to `main`

---

## Previous Releases
See `main` branch CHANGELOG for project-level release history.
