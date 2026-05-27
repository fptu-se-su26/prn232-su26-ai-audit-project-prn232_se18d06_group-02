# Changelog -- Feature -- Messaging and Chat

All notable changes on branch `feature/messaging-chat` are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] -- 2026-05-27

### Scope
Real-time buyer-seller chat via SignalR: conversation threads, read/unread state, inbox with unread counts

### Added
- Conversation entity linking Buyer (ApplicationUser) and Store
- ChatMessage entity with SenderId, Body, SentAt, IsReadByRecipient
- ChatHub (SignalR) for real-time message delivery and read receipts
- ChatController REST endpoints for message history pagination (SignalR for real-time)
- Inbox view showing conversations sorted by latest message with unread count badges
- IChatService contract and implementation

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
