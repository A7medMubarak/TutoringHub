using TutoringHub.Application.DTOs.Enrollments;

namespace TutoringHub.Application.Services.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentDto> EnrollAsync(int classGroupId, int studentId, CancellationToken cancellationToken = default);
    Task UnenrollAsync(int classGroupId, int studentId, CancellationToken cancellationToken = default);
}