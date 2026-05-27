using AutoMapper;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Controllers.Api;
using GearZone.Web.Pages.Admin.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers.Api.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/users")]
[ApiController]
public class UsersController : BaseApiController
{
    private readonly IAdminUserService _userService;
    private readonly IMapper _mapper;

    public UsersController(IAdminUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    // GET /api/admin/users?[query]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] UserQueryDto query)
    {
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1) query.PageSize = 10;

        var result = await _userService.GetPaginatedUsersAsync(query);
        var viewModels = _mapper.Map<List<UserViewModel>>(result.Items);
        var stats = await _userService.GetUserStatsAsync();
        var roles = await _userService.GetAllRolesAsync();

        return OkResponse(new
        {
            users = new { items = viewModels, result.TotalCount, result.PageNumber, result.PageSize },
            stats,
            roles
        });
    }

    // GET /api/admin/users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return FailResponse("User not found.", 404);
        return OkResponse(_mapper.Map<UserViewModel>(user));
    }

    // POST /api/admin/users
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserViewModel request)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var dto = _mapper.Map<CreateUserDto>(request);
        var result = await _userService.CreateUserAsync(dto);

        if (result.Succeeded) return OkResponse("User created.");
        return FailResponse("Failed to create user.", result.Errors.Select(e => e.Description));
    }

    // PUT /api/admin/users
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] EditUserViewModel request)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var dto = _mapper.Map<EditUserDto>(request);
        var result = await _userService.UpdateUserAsync(dto);

        if (result.Succeeded) return OkResponse("User updated.");
        return FailResponse("Failed to update user.", result.Errors.Select(e => e.Description));
    }

    // DELETE /api/admin/users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _userService.SoftDeleteUserAsync(id, User.Identity!.Name ?? "System");
        return result.Succeeded ? OkResponse("User deleted.") : FailResponse("Failed to delete user.");
    }

    // POST /api/admin/users/{id}/restore
    [HttpPost("{id}/restore")]
    public async Task<IActionResult> Restore(string id)
    {
        var result = await _userService.RestoreUserAsync(id);
        return result.Succeeded ? OkResponse("User restored.") : FailResponse("Failed to restore user.");
    }
}
