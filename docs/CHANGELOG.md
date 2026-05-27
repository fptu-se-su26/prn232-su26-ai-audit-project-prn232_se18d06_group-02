# Changelog -- Core -- Logging Infrastructure

All notable changes on branch `core/logging-infrastructure` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Cross-cutting logging: IAppLogger<T> abstraction, SerilogAppLogger<T>, RequestLoggingMiddleware, AuditTrailLogger

### Added
- IAppLogger<T> interface in Application layer (LogInformation, LogWarning, LogError, LogDebug, LogCritical)
- SerilogAppLogger<T> bridging IAppLogger<T> to ASP.NET Core ILogger<T>/Serilog sinks
- RequestLoggingMiddleware logging HTTP method, path, status code, elapsed ms; auto-promotes 4xx/5xx to Warning/Error
- IAuditTrailLogger + AuditTrailLogger for structured CREATE/UPDATE/DELETE audit entries
- UseRequestLogging() extension method for clean middleware registration

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
