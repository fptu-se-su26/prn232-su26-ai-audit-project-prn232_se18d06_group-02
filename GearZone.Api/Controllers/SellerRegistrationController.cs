using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers;

[Authorize]
[Route("api/seller-registration")]
[ApiController]
public class SellerRegistrationController : BaseApiController
{
    private readonly ISellerStoreService _sellerStoreService;
    private readonly IAuthService _authService;

    public SellerRegistrationController(ISellerStoreService sellerStoreService, IAuthService authService)
    {
        _sellerStoreService = sellerStoreService;
        _authService = authService;
    }

    // GET /api/seller-registration/progress
    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress()
    {
        var user = await _authService.GetUserAsync(User);
        if (user == null) return FailResponse("Unauthorized.", 401);

        var existingStore = await _sellerStoreService.GetStoreByOwnerIdAsync(user.Id);
        var progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);

        return OkResponse(new SellerRegistrationStateDto
        {
            ExistingStore = existingStore == null ? null : new SellerRegistrationStoreStateDto
            {
                Id = existingStore.Id,
                Status = existingStore.Status,
                RejectReason = existingStore.RejectReason
            },
            Progress = progress == null ? null : SellerRegistrationProgressStateDto.From(progress)
        });
    }

    // POST /api/seller-registration/step1
    [HttpPost("step1")]
    public async Task<IActionResult> Step1([FromBody] Step1Dto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var user = await _authService.GetUserAsync(User);
        if (user == null) return FailResponse("Unauthorized.", 401);

        try
        {
            var storeId = await _sellerStoreService.SaveStep1Async(user.Id, dto);
            return OkResponse(new { storeId }, "Step 1 saved.");
        }
        catch (InvalidOperationException ex)
        {
            return FailResponse(ex.Message);
        }
    }

    // POST /api/seller-registration/step2
    [HttpPost("step2")]
    public async Task<IActionResult> Step2([FromForm] Step2Dto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var user = await _authService.GetUserAsync(User);
        if (user == null) return FailResponse("Unauthorized.", 401);

        var progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);
        if (progress?.StoreId == null) return FailResponse("Start from step 1 first.");

        try
        {
            await _sellerStoreService.SaveStep2Async(progress.StoreId.Value, user.Id, dto);
            return OkResponse("Step 2 saved.");
        }
        catch (InvalidOperationException ex)
        {
            return FailResponse(ex.Message);
        }
    }

    // POST /api/seller-registration/step3
    [HttpPost("step3")]
    public async Task<IActionResult> Step3([FromBody] Step3Dto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var user = await _authService.GetUserAsync(User);
        if (user == null) return FailResponse("Unauthorized.", 401);

        var progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);
        if (progress?.StoreId == null) return FailResponse("Start from step 1 first.");

        try
        {
            await _sellerStoreService.SaveStep3Async(progress.StoreId.Value, dto);
            return OkResponse("Step 3 saved.");
        }
        catch (InvalidOperationException ex)
        {
            return FailResponse(ex.Message);
        }
    }

    // POST /api/seller-registration/submit
    [HttpPost("submit")]
    public async Task<IActionResult> Submit()
    {
        var user = await _authService.GetUserAsync(User);
        if (user == null) return FailResponse("Unauthorized.", 401);

        var progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);
        if (progress?.StoreId == null) return FailResponse("Registration not started.");

        try
        {
            await _sellerStoreService.SubmitRegistrationAsync(progress.StoreId.Value, user.Id);
            return OkResponse("Registration submitted. We will review and respond as soon as possible.");
        }
        catch (InvalidOperationException ex)
        {
            return FailResponse(ex.Message);
        }
    }

    // POST /api/seller-registration/reapply
    [HttpPost("reapply")]
    public async Task<IActionResult> Reapply()
    {
        var user = await _authService.GetUserAsync(User);
        if (user == null) return FailResponse("Unauthorized.", 401);

        var existingStore = await _sellerStoreService.GetStoreByOwnerIdAsync(user.Id);
        if (existingStore == null || (existingStore.Status != StoreStatus.Pending && existingStore.Status != StoreStatus.Rejected))
            return FailResponse("No rejected or pending application to reapply.");

        try
        {
            await _sellerStoreService.StartReapplicationAsync(user.Id);
            return OkResponse("Application reopened for editing.");
        }
        catch (InvalidOperationException ex)
        {
            return FailResponse(ex.Message);
        }
    }
}
