using TutoringHub.Application.DTOs.Quizzes;

namespace TutoringHub.Application.Services.Interfaces;

public interface IStudentQuizService
{
    Task<List<StudentQuizSummaryDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<StudentQuizDto> GetAsync(int quizId, CancellationToken cancellationToken = default);
    Task<AttemptResultDto> SubmitAsync(int quizId, SubmitAttemptRequest request, CancellationToken cancellationToken = default);
}