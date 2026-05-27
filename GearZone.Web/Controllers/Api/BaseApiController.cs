using GearZone.Web.Common;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GearZone.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected string? CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    protected IActionResult OkResponse<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult OkResponse(string? message = null) =>
        Ok(ApiResponse.Ok(message));

    protected IActionResult CreatedResponse<T>(T data, string location, string? message = null) =>
        Created(location, ApiResponse<T>.Ok(data, message));

    protected IActionResult FailResponse(string message, int statusCode = 400) =>
        StatusCode(statusCode, ApiResponse.Fail(message));

    protected IActionResult FailResponse(string message, IEnumerable<string> errors, int statusCode = 400) =>
        StatusCode(statusCode, ApiResponse.Fail(message, errors));

    protected IActionResult ValidationFailResponse() =>
        BadRequest(ApiResponse.Fail(
            "Validation failed",
            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
}
