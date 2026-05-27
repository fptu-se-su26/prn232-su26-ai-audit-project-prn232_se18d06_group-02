# AI Prompts Log -- Feature -- Messaging and Chat

Branch: `feature/messaging-chat`
Scope: Real-time buyer-seller chat via SignalR: conversation threads, read/unread state, inbox with unread counts

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Real-time buyer-seller chat via SignalR: conversation threads, read/unread state, inbox with unread counts

**Prompt:**
> Design a real-time chat system between buyers and sellers using SignalR in ASP.NET Core -- what is the correct Hub design?

**AI Output Summary:**
ChatHub.SendMessage: persists to DB via ChatService; sends to recipient user group via Clients.User(recipientId); returns to sender for confirmation.

**Used in files:** Features/Chat/*, Controllers/Api/ChatController.cs, Hubs/ChatHub.cs, Pages/Public/User/Messages/*, Pages/StoreOwner/Messages/*

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Real-time buyer-seller chat via SignalR: conversation threads, read/unread state, inbox with unread counts

**Prompt:**
> How do I implement unread message counts efficiently without querying all messages?

**AI Output Summary:**
ChatMessage.IsReadByRecipient boolean; unread count = COUNT WHERE IsReadByRecipient=false AND RecipientId=currentUser; updated on conversation open.

**Used in files:** Features/Chat/*, Controllers/Api/ChatController.cs, Hubs/ChatHub.cs, Pages/Public/User/Messages/*, Pages/StoreOwner/Messages/*

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Real-time buyer-seller chat via SignalR: conversation threads, read/unread state, inbox with unread counts

**Prompt:**
> How should I handle SignalR connection management for users who have multiple browser tabs open?

**AI Output Summary:**
Hub.Groups with userId as group name; multiple connections per user automatically receive messages.

**Used in files:** Features/Chat/*, Controllers/Api/ChatController.cs, Hubs/ChatHub.cs, Pages/Public/User/Messages/*, Pages/StoreOwner/Messages/*

---
