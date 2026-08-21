using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TutoringHub.Application.DTOs.Quizzes;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Enums;
using TutoringHub.Domain.Interfaces;

namespace TutoringHub.Application.Services;

public class StudentQuizService : IStudentQuizService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public StudentQuizService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<StudentQuizSummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var studentId = EnsureStudent();

        var classIds = await GetEnrolledClassIdsAsync(studentId, cancellationToken);

        if (classIds.Count == 0)
            return new List<StudentQuizSummaryDto>();

        var quizIds = await _context.QuizAssignments
            .AsNoTracking()
            .Where(a => classIds.Contains(a.ClassGroupId))
            .Select(a => a.QuizId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context.Quizzes
            .AsNoTracking()
            .Where(q => q.PublishedAtUtc != null && quizIds.Contains(q.Id))
            .Select(q => new StudentQuizSummaryDto
            {
                QuizId = q.Id,
                Title = q.Title,
                QuestionType = q.QuestionType,
                QuestionCount = q.Questions.Count,
                PublishedAtUtc = q.PublishedAtUtc!.Value,
                AttemptCount = q.Attempts.Count(a => a.StudentId == studentId),
                LastCorrectCount = q.Attempts
                    .Where(a => a.StudentId == studentId)
                    .OrderByDescending(a => a.SubmittedAtUtc)
                    .Select(a => (int?)a.CorrectCount)
                    .FirstOrDefault(),
                LastTotalCount = q.Attempts
                    .Where(a => a.StudentId == studentId)
                    .OrderByDescending(a => a.SubmittedAtUtc)
                    .Select(a => (int?)a.TotalCount)
                    .FirstOrDefault()
            })
            .OrderByDescending(q => q.PublishedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentQuizDto> GetAsync(int quizId, CancellationToken cancellationToken = default)
    {
        var studentId = EnsureStudent();

        var quiz = await LoadAccessibleQuizAsync(quizId, studentId, cancellationToken);

        return new StudentQuizDto
        {
            QuizId = quiz.Id,
            Title = quiz.Title,
            QuestionType = quiz.QuestionType,
            Questions = quiz.Questions
                .OrderBy(q => q.SortOrder)
                .Select(q => new StudentQuizQuestionDto
                {
                    Question = q.Question,
                    Options = string.IsNullOrWhiteSpace(q.Options)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(q.Options, JsonOptions) ?? new List<string>()
                })
                .ToList()
        };
    }

    public async Task<AttemptResultDto> SubmitAsync(int quizId, SubmitAttemptRequest request, CancellationToken cancellationToken = default)
    {
        var studentId = EnsureStudent();

        var quiz = await LoadAccessibleQuizAsync(quizId, studentId, cancellationToken);

        var questions = quiz.Questions.OrderBy(q => q.SortOrder).ToList();

        if (request.Answers.Count != questions.Count)
            throw new ArgumentException("Answer count does not match the quiz questions.");

        var results = new List<GradedQuestionDto>(questions.Count);
        var correctCount = 0;

        for (var i = 0; i < questions.Count; i++)
        {
            var question = questions[i];
            var answer = request.Answers[i];
            var isCorrect = Grade(question, answer, out var result);
            if (isCorrect)
                correctCount++;
            results.Add(result);
        }

        var isFirstAttempt = !await _context.QuizAttempts
            .AnyAsync(a => a.QuizId == quizId && a.StudentId == studentId, cancellationToken);

        _context.QuizAttempts.Add(new QuizAttempt
        {
            QuizId = quizId,
            StudentId = studentId,
            CorrectCount = correctCount,
            TotalCount = questions.Count
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new AttemptResultDto
        {
            QuizId = quizId,
            CorrectCount = correctCount,
            TotalCount = questions.Count,
            Percentage = Math.Round(correctCount * 100.0 / questions.Count, 1),
            IsFirstAttempt = isFirstAttempt,
            Results = results
        };
    }

    private static bool Grade(QuizQuestion question, AnswerItemDto answer, out GradedQuestionDto result)
    {
        if (question.CorrectIndex is not null)
        {
            if (answer.OptionIndex is not { } chosen)
                throw new ArgumentException("Invalid answer format.");

            var isCorrect = chosen == question.CorrectIndex;
            result = new GradedQuestionDto
            {
                Question = question.Question,
                IsCorrect = isCorrect,
                YourIndex = chosen,
                CorrectIndex = question.CorrectIndex
            };
            return isCorrect;
        }

        if (question.IsTrue is not { } choice)
            throw new ArgumentException("Invalid answer format.");

        if (answer.IsTrue is not { } answered)
            throw new ArgumentException("Invalid answer format.");

        var correct = answered == choice;
        result = new GradedQuestionDto
        {
            Question = question.Question,
            IsCorrect = correct,
            YourAnswer = answered,
            CorrectAnswer = choice
        };
        return correct;
    }

    private async Task<Quiz> LoadAccessibleQuizAsync(int quizId, int studentId, CancellationToken cancellationToken)
    {
        var classIds = await GetEnrolledClassIdsAsync(studentId, cancellationToken);

        var quiz = await _context.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions)
            .Include(q => q.Assignments)
            .SingleOrDefaultAsync(q => q.Id == quizId && q.PublishedAtUtc != null, cancellationToken)
            ?? throw new ArgumentException("Quiz not found.");

        if (!quiz.Assignments.Any(a => classIds.Contains(a.ClassGroupId)))
            throw new ArgumentException("Quiz not found.");

        return quiz;
    }

    private async Task<List<int>> GetEnrolledClassIdsAsync(int studentId, CancellationToken cancellationToken)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Select(e => e.ClassGroupId)
            .ToListAsync(cancellationToken);
    }

    private int EnsureStudent()
    {
        if (_currentUser.Role != Role.Student)
            throw new UnauthorizedAccessException("Only students can access this resource.");

        return _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Only students can access this resource.");
    }
}