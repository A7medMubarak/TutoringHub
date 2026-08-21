using TutoringHub.Application.DTOs.Students;

namespace TutoringHub.Application.Services.Interfaces;

public interface IStudentService
{
    Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);
    Task<StudentDto> GetByIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<List<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default);
}