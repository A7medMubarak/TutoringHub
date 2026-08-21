using TutoringHub.Application.DTOs.Quizzes;

namespace TutoringHub.Application.Services.Interfaces;

public interface IQuizService
{
    Task<QuizDetailDto> CreateAsync(CreateQuizRequest request, CancellationToken cancellationToken = default);
    Task<QuizDetailDto> PublishAsync(int quizId, PublishQuizRequest request, CancellationToken cancellationToken = default);
    Task<QuizDetailDto> UnpublishAsync(int quizId, int classGroupId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int quizId, CancellationToken cancellationToken = default);
    Task<List<QuizDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<QuizDetailDto> GetDetailAsync(int quizId, CancellationToken cancellationToken = default);
    Task<List<QuizScoreDto>> GetResultsAsync(int quizId, CancellationToken cancellationToken = default);
}