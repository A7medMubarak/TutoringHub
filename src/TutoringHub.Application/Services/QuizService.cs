using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TutoringHub.Application.DTOs.Ai;
using TutoringHub.Application.DTOs.Quizzes;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Enums;
using TutoringHub.Domain.Interfaces;

namespace TutoringHub.Application.Services;

public class QuizService : IQuizService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public QuizService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<QuizDetailDto> CreateAsync(CreateQuizRequest request, CancellationToken cancellationToken = default)
    {
        var teacherId = EnsureTeacher();

        if (request.Questions.Count == 0)
            throw new ArgumentException("At least one question is required.");

        var questionType = ResolveQuestionType(request.Questions);
        var questions = request.Questions
            .Select((q, i) => new QuizQuestion
            {
                SortOrder = i,
                Question = q.Question,
                Options = JsonSerializer.Serialize(q.Options ?? new List<string>(), JsonOptions),
                CorrectIndex = q.CorrectIndex,
                IsTrue = q.IsTrue
            })
            .ToList();

        var quiz = new Quiz
        {
            TeacherId = teacherId,
            Title = request.Title,
            QuestionType = questionType,
            Questions = questions
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetDetailAsync(quiz.Id, cancellationToken);
    }

    public async Task<QuizDetailDto> PublishAsync(int quizId, PublishQuizRequest request, CancellationToken cancellationToken = default)
    {
        var teacherId = EnsureTeacher();

        var quiz = await _context.Quizzes
            .Include(q => q.Assignments)
            .SingleOrDefaultAsync(q => q.Id == quizId && q.TeacherId == teacherId, cancellationToken)
            ?? throw new ArgumentException("Quiz not found.");

        var classIds = request.ClassGroupIds.Distinct().ToList();
        var owned = await _context.ClassGroups
            .AsNoTracking()
            .Where(c => c.TeacherId == teacherId && classIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (owned.Count != classIds.Count)
            throw new ArgumentException("Class not found.");

        var existing = quiz.Assignments.Select(a => a.ClassGroupId).ToHashSet();

        foreach (var classId in classIds)
        {
            if (existing.Contains(classId))
                continue;

            quiz.Assignments.Add(new QuizAssignment { ClassGroupId = classId });
        }

        if (quiz.Assignments.Count > 0 && quiz.PublishedAtUtc is null)
            quiz.PublishedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetDetailAsync(quiz.Id, cancellationToken);
    }

    public async Task<QuizDetailDto> UnpublishAsync(int quizId, int classGroupId, CancellationToken cancellationToken = default)
    {
        var teacherId = EnsureTeacher();

        var quiz = await _context.Quizzes
            .Include(q => q.Assignments)
            .SingleOrDefaultAsync(q => q.Id == quizId && q.TeacherId == teacherId, cancellationToken)
            ?? throw new ArgumentException("Quiz not found.");

        var assignment = quiz.Assignments.SingleOrDefault(a => a.ClassGroupId == classGroupId)
            ?? throw new ArgumentException("Quiz is not assigned to this class.");

        _context.QuizAssignments.Remove(assignment);
        quiz.Assignments.Remove(assignment);

        if (quiz.Assignments.Count == 0)
            quiz.PublishedAtUtc = null;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetDetailAsync(quiz.Id, cancellationToken);
    }

    public async Task DeleteAsync(int quizId, CancellationToken cancellationToken = default)
    {
        var teacherId = EnsureTeacher();

        var quiz = await _context.Quizzes
            .SingleOrDefaultAsync(q => q.Id == quizId && q.TeacherId == teacherId, cancellationToken)
            ?? throw new ArgumentException("Quiz not found.");

        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<QuizDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var teacherId = EnsureTeacher();

        return await _context.Quizzes
            .AsNoTracking()
            .Where(q => q.TeacherId == teacherId)
            .OrderByDescending(q => q.CreatedAtUtc)
            .Select(q => new QuizDto
            {
                Id = q.Id,
                Title = q.Title,
                QuestionType = q.QuestionType,
                CreatedAtUtc = q.CreatedAtUtc,
                PublishedAtUtc = q.PublishedAtUtc,
                QuestionCount = q.Questions.Count,
                AttemptCount = q.Attempts.Count,
                PublishedClassNames = q.Assignments.Select(a => a.ClassGroup.Name).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<QuizDetailDto> GetDetailAsync(int quizId, CancellationToken cancellationToken = default)
    {
        var teacherId = EnsureTeacher();

        var quiz = await _context.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions)
            .Include(q => q.Assignments)
            .ThenInclude(a => a.ClassGroup)
            .SingleOrDefaultAsync(q => q.Id == quizId && q.TeacherId == teacherId, cancellationToken)
            ?? throw new ArgumentException("Quiz not found.");

        return MapToDetailDto(quiz, await _context.QuizAttempts
            .CountAsync(a => a.QuizId == quizId, cancellationToken));
    }

    public async Task<List<QuizScoreDto>> GetResultsAsync(int quizId, CancellationToken cancellationToken = default)
    {
        var teacherId = EnsureTeacher();

        if (!await _context.Quizzes.AnyAsync(q => q.Id == quizId && q.TeacherId == teacherId, cancellationToken))
            throw new ArgumentException("Quiz not found.");

        var attempts = await _context.QuizAttempts
            .AsNoTracking()
            .Include(a => a.Student)
            .Where(a => a.QuizId == quizId)
            .OrderBy(a => a.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

        var scores = attempts
            .GroupBy(a => a.StudentId)
            .Select(g =>
            {
                var first = g.First();
                return new QuizScoreDto
                {
                    StudentId = first.StudentId,
                    StudentName = first.Student.FullName,
                    HasTaken = true,
                    FirstAttemptCorrect = first.CorrectCount,
                    FirstAttemptTotal = first.TotalCount,
                    FirstAttemptAtUtc = first.SubmittedAtUtc,
                    AttemptCount = g.Count()
                };
            })
            .ToDictionary(s => s.StudentId);

        var classIds = await _context.QuizAssignments
            .AsNoTracking()
            .Where(a => a.QuizId == quizId)
            .Select(a => a.ClassGroupId)
            .ToListAsync(cancellationToken);

        var enrolled = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Where(e => classIds.Contains(e.ClassGroupId))
            .Select(e => new { e.StudentId, e.Student.FullName })
            .ToListAsync(cancellationToken);

        foreach (var item in enrolled)
        {
            if (!scores.ContainsKey(item.StudentId))
                scores[item.StudentId] = new QuizScoreDto
                {
                    StudentId = item.StudentId,
                    StudentName = item.FullName,
                    HasTaken = false
                };
        }

        return scores.Values.OrderBy(s => s.StudentName).ToList();
    }

    private static QuestionType ResolveQuestionType(List<GeneratedQuestionDto> questions)
    {
        var firstIsMcq = questions[0].Options is not null;
        var type = firstIsMcq ? QuestionType.MultipleChoice : QuestionType.TrueFalse;

        foreach (var question in questions)
        {
            var isMcq = question.Options is not null;
            if (isMcq != firstIsMcq)
                throw new ArgumentException("All questions must be the same type.");

            if (!IsValid(question, isMcq))
                throw new ArgumentException("Quiz questions are invalid.");
        }

        return type;
    }

    private static bool IsValid(GeneratedQuestionDto question, bool isMcq)
    {
        if (string.IsNullOrWhiteSpace(question.Question))
            return false;

        return isMcq
            ? question.Options?.Count == 4 &&
              question.Options.All(o => !string.IsNullOrWhiteSpace(o)) &&
              question.CorrectIndex is >= 0 and <= 3
            : question.IsTrue is not null;
    }

    private static QuizDetailDto MapToDetailDto(Quiz quiz, int attemptCount)
    {
        return new QuizDetailDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            QuestionType = quiz.QuestionType,
            CreatedAtUtc = quiz.CreatedAtUtc,
            PublishedAtUtc = quiz.PublishedAtUtc,
            AttemptCount = attemptCount,
            Questions = quiz.Questions
                .OrderBy(q => q.SortOrder)
                .Select(q => new QuizQuestionDto
                {
                    Id = q.Id,
                    Question = q.Question,
                    Options = string.IsNullOrWhiteSpace(q.Options)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(q.Options, JsonOptions) ?? new List<string>(),
                    CorrectIndex = q.CorrectIndex,
                    IsTrue = q.IsTrue
                })
                .ToList(),
            PublishedClassNames = quiz.Assignments.Select(a => a.ClassGroup.Name).ToList()
        };
    }

    private int EnsureTeacher()
    {
        return _currentUser.TeacherId
            ?? throw new UnauthorizedAccessException("Only teachers can manage quizzes.");
    }
}