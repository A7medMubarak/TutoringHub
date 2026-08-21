using TutoringHub.Domain.Enums;

namespace TutoringHub.Application.Services.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    Role? Role { get; }
    int? TeacherId { get; }
}