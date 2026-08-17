using System.Security.Claims;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Enums;

namespace TutoringHub.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public Role? Role
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<Role>(value, out var role) ? role : null;
        }
    }

    public int? TeacherId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue("TeacherId");
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}