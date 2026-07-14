using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Chat.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers;

[Authorize]
public class ChatController : BaseApiController
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    // ─── Buyer ────────────────────────────────────────────────────────────────

    // GET /api/chat/buyer/bootstrap
    [HttpGet("buyer/bootstrap")]
    public async Task<IActionResult> BuyerBootstrap([FromQuery] ChatWidgetBootstrapQueryDto query)
    {
        var result = await _chatService.GetBuyerWidgetBootstrapAsync(CurrentUserId!, query);
        return OkResponse(result);
    }

    // GET /api/chat/buyer/inbox
    [HttpGet("buyer/inbox")]
    public async Task<IActionResult> BuyerInbox([FromQuery] ChatInboxQueryDto query)
    {
        var conversations = await _chatService.GetBuyerInboxAsync(CurrentUserId!, query);
        return OkResponse(conversations);
    }

    // GET /api/chat/buyer/conversations/{id}/thread
    [HttpGet("buyer/conversations/{id:guid}/thread")]
    public async Task<IActionResult> BuyerThread(Guid id, [FromQuery] ChatThreadQueryDto query)
    {
        var thread = await _chatService.GetBuyerThreadAsync(CurrentUserId!, id, query);
        if (thread == null) return FailResponse("Conversation not found.", 404);

        await _chatService.MarkConversationReadAsync(CurrentUserId!, id);
        return OkResponse(thread);
    }

    // GET /api/chat/buyer/conversations/{id}/update
    [HttpGet("buyer/conversations/{id:guid}/update")]
    public async Task<IActionResult> BuyerConversationUpdate(Guid id)
    {
        var update = await _chatService.GetConversationUpdateForBuyerAsync(CurrentUserId!, id);
        return OkResponse(update);
    }

    // GET /api/chat/buyer/unread
    [HttpGet("buyer/unread")]
    public async Task<IActionResult> BuyerUnread()
    {
        var count = await _chatService.GetBuyerUnreadCountAsync(CurrentUserId!);
        return OkResponse(new { unreadCount = count });
    }

    // GET /api/chat/buyer/scope-options
    [HttpGet("buyer/scope-options")]
    public async Task<IActionResult> BuyerScopeOptions()
    {
        var options = await _chatService.GetBuyerCounterpartScopeOptionsAsync(CurrentUserId!);
        return OkResponse(options);
    }

    // POST /api/chat/buyer/conversations/ensure
    [HttpPost("buyer/conversations/ensure")]
    public async Task<IActionResult> EnsureBuyerConversation([FromBody] EnsureBuyerConversationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.StoreSlug))
            return FailResponse("StoreSlug is required.");

        var conversationId = await _chatService.EnsureBuyerConversationAsync(CurrentUserId!, request.StoreSlug);
        if (conversationId == null) return FailResponse("Could not open conversation with that store.");
        return OkResponse(new { conversationId });
    }

    // ─── Seller ───────────────────────────────────────────────────────────────

    // GET /api/chat/seller/inbox
    [HttpGet("seller/inbox")]
    public async Task<IActionResult> SellerInbox([FromQuery] ChatInboxQueryDto query)
    {
        var conversations = await _chatService.GetSellerInboxAsync(CurrentUserId!, query);
        return OkResponse(conversations);
    }

    // GET /api/chat/seller/conversations/{id}/thread
    [HttpGet("seller/conversations/{id:guid}/thread")]
    public async Task<IActionResult> SellerThread(Guid id, [FromQuery] ChatThreadQueryDto query)
    {
        var thread = await _chatService.GetSellerThreadAsync(CurrentUserId!, id, query);
        if (thread == null) return FailResponse("Conversation not found.", 404);

        await _chatService.MarkConversationReadAsync(CurrentUserId!, id);
        return OkResponse(thread);
    }

    // GET /api/chat/seller/conversations/{id}/update
    [HttpGet("seller/conversations/{id:guid}/update")]
    public async Task<IActionResult> SellerConversationUpdate(Guid id)
    {
        var update = await _chatService.GetConversationUpdateForSellerAsync(CurrentUserId!, id);
        return OkResponse(update);
    }

    // GET /api/chat/seller/unread
    [HttpGet("seller/unread")]
    public async Task<IActionResult> SellerUnread()
    {
        var count = await _chatService.GetSellerUnreadCountAsync(CurrentUserId!);
        return OkResponse(new { unreadCount = count });
    }

    // GET /api/chat/seller/scope-options
    [HttpGet("seller/scope-options")]
    public async Task<IActionResult> SellerScopeOptions()
    {
        var options = await _chatService.GetSellerCounterpartScopeOptionsAsync(CurrentUserId!);
        return OkResponse(options);
    }

    // POST /api/chat/seller/conversations/ensure-from-order
    [HttpPost("seller/conversations/ensure-from-order")]
    public async Task<IActionResult> EnsureSellerConversationFromOrder([FromBody] EnsureSellerConversationRequest request)
    {
        var conversationId = await _chatService.EnsureSellerConversationFromSubOrderAsync(CurrentUserId!, request.SubOrderId);
        if (conversationId == null) return FailResponse("Could not link conversation to that order.");
        return OkResponse(new { conversationId });
    }

    // ─── Shared ───────────────────────────────────────────────────────────────

    // POST /api/chat/send
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendChatMessageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            return FailResponse("Message content cannot be empty.");

        var result = await _chatService.SendMessageAsync(CurrentUserId!, dto);
        return OkResponse(result);
    }

    // PATCH /api/chat/conversations/{id}/mark-read
    [HttpPatch("conversations/{id:guid}/mark-read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var count = await _chatService.MarkConversationReadAsync(CurrentUserId!, id);
        return OkResponse(new { markedCount = count });
    }
}

public class EnsureBuyerConversationRequest
{
    public string StoreSlug { get; set; } = string.Empty;
}

public class EnsureSellerConversationRequest
{
    public Guid SubOrderId { get; set; }
}
