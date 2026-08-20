using FluentAssertions;
using TutoringHub.Application.DTOs.Ai;
using TutoringHub.Application.DTOs.Quizzes;
using TutoringHub.Application.Services;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Application.Tests.TestCommon;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Enums;
using TutoringHub.Infrastructure.Persistence;

namespace TutoringHub.Application.Tests.Services;

public class QuizServiceTests
{
    private static QuizService CreateService(ApplicationDbContext context, int? teacherId) =>
        new(context, new StubCurrentUser(teacherId));

    private static ClassGroup NewClass(Center center, int teacherId, string name = "Math Saturday 4PM") =>
        new()
        {
            Name = name,
            TeacherId = teacherId,
            Center = center,
            DayOfWeek = DayOfWeek.Saturday,
            StartTime = new TimeSpan(16, 0, 0),
            Frequency = Frequency.OncePerWeek,
            IsActive = true
        };

    private static Student NewStudent(string fullName, string phone, int teacherId) =>
        new() { FullName = fullName, Phone = phone, PinHash = "x", TeacherId = teacherId };

    private static CreateQuizRequest McqRequest(string title = "Math Quiz") => new()
    {
        Title = title,
        Questions =
        [
            new GeneratedQuestionDto { Question = "1+1?", Options = ["1", "2", "3", "4"], CorrectIndex = 1 },
            new GeneratedQuestionDto { Question = "2+2?", Options = ["1", "2", "3", "4"], CorrectIndex = 3 }
        ]
    };

    private static CreateQuizRequest TrueFalseRequest() => new()
    {
        Title = "Basics Quiz",
        Questions =
        [
            new GeneratedQuestionDto { Question = "Sky is blue", IsTrue = true },
            new GeneratedQuestionDto { Question = "Winter is hot", IsTrue = false }
        ]
    };

    private static Quiz NewPublishedQuiz(ClassGroup cls, string title = "Math Quiz")
    {
        var quiz = new Quiz
        {
            TeacherId = cls.TeacherId,
            Title = title,
            QuestionType = QuestionType.MultipleChoice,
            PublishedAtUtc = DateTime.UtcNow,
            Questions =
            [
                new QuizQuestion { SortOrder = 0, Question = "1+1?", Options = """["1","2","3","4"]""", CorrectIndex = 1 },
                new QuizQuestion { SortOrder = 1, Question = "2+2?", Options = """["1","2","3","4"]""", CorrectIndex = 3 }
            ]
        };
        quiz.Assignments.Add(new QuizAssignment { ClassGroup = cls });
        return quiz;
    }

    [Fact]
    public async Task CreateAsync_ValidMcq_CreatesQuizWithAnswerKeyAndNoPublish()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var ctx = MockDbContext.Create(centers: [center]);
        var service = CreateService(ctx, teacherId: 3);

        var result = await service.CreateAsync(McqRequest());

        result.Title.Should().Be("Math Quiz");
        result.QuestionType.Should().Be(QuestionType.MultipleChoice);
        result.PublishedAtUtc.Should().BeNull();
        result.Questions.Should().HaveCount(2);
        result.Questions[0].Options.Should().Equal("1", "2", "3", "4");
        result.Questions[0].CorrectIndex.Should().Be(1);
        result.Questions[1].CorrectIndex.Should().Be(3);
    }

    [Fact]
    public async Task CreateAsync_ValidTrueFalse_CreatesTrueFalseQuiz()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var ctx = MockDbContext.Create(centers: [center]);
        var service = CreateService(ctx, teacherId: 3);

        var result = await service.CreateAsync(TrueFalseRequest());

        result.QuestionType.Should().Be(QuestionType.TrueFalse);
        result.Questions[0].IsTrue.Should().BeTrue();
        result.Questions[1].IsTrue.Should().BeFalse();
        result.Questions[0].Options.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_MixedQuestionTypes_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var ctx = MockDbContext.Create(centers: [center]);
        var service = CreateService(ctx, teacherId: 3);
        var request = McqRequest();
        request.Questions.Add(new GeneratedQuestionDto { Question = "True?", IsTrue = true });

        await service.Invoking(s => s.CreateAsync(request))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("All questions must be the same type.");
    }

    [Fact]
    public async Task CreateAsync_InvalidMcqOptions_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var ctx = MockDbContext.Create(centers: [center]);
        var service = CreateService(ctx, teacherId: 3);
        var request = McqRequest();
        request.Questions[1].Options = ["1", "2", "3"];

        await service.Invoking(s => s.CreateAsync(request))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Quiz questions are invalid.");
    }

    [Fact]
    public async Task CreateAsync_InvalidTrueFalse_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var ctx = MockDbContext.Create(centers: [center]);
        var service = CreateService(ctx, teacherId: 3);
        var request = TrueFalseRequest();
        request.Questions[1].IsTrue = null;

        await service.Invoking(s => s.CreateAsync(request))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Quiz questions are invalid.");
    }

    [Fact]
    public async Task CreateAsync_NoQuestions_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var ctx = MockDbContext.Create(centers: [center]);
        var service = CreateService(ctx, teacherId: 3);

        await service.Invoking(s => s.CreateAsync(new CreateQuizRequest { Title = "Empty" }))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("At least one question is required.");
    }

    [Fact]
    public async Task CreateAsync_NotTeacher_ThrowsUnauthorizedAccessException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var ctx = MockDbContext.Create(centers: [center]);

        var service = CreateService(ctx, teacherId: null);

        await service.Invoking(s => s.CreateAsync(McqRequest()))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task PublishAsync_OwnedClass_SetsPublishedAtUtcAndCreatesAssignment()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);
        var quiz = await service.CreateAsync(McqRequest());

        var result = await service.PublishAsync(quiz.Id, new PublishQuizRequest { ClassGroupIds = [cls.Id] });

        result.PublishedAtUtc.Should().NotBeNull();
        result.PublishedClassNames.Should().Equal("Math Saturday 4PM");
        ctx.QuizAssignments.Should().ContainSingle(a => a.QuizId == quiz.Id && a.ClassGroupId == cls.Id);
    }

    [Fact]
    public async Task PublishAsync_ClassOfAnotherTeacher_ThrowsArgumentException()
    {
        var center = new Center { Name = "Other", LocationDetails = "Giza", TeacherId = 9 };
        var cls = NewClass(center, 9);
        var ownCenter = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var ctx = MockDbContext.Create(centers: [ownCenter, center], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);
        var quiz = await service.CreateAsync(McqRequest());

        await service.Invoking(s => s.PublishAsync(quiz.Id, new PublishQuizRequest { ClassGroupIds = [cls.Id] }))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Class not found.");
    }

    [Fact]
    public async Task PublishAsync_AlreadyAssignedClass_IsIdempotent()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);
        var quiz = await service.CreateAsync(McqRequest());

        await service.PublishAsync(quiz.Id, new PublishQuizRequest { ClassGroupIds = [cls.Id] });
        await service.PublishAsync(quiz.Id, new PublishQuizRequest { ClassGroupIds = [cls.Id] });

        ctx.QuizAssignments.Should().ContainSingle(a => a.QuizId == quiz.Id);
    }

    [Fact]
    public async Task PublishAsync_QuizOfAnotherTeacher_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], classGroups: [cls]);
        var otherTeacherService = CreateService(ctx, teacherId: 9);
        var quiz = await otherTeacherService.CreateAsync(McqRequest("Foreign Quiz"));

        var service = CreateService(ctx, teacherId: 3);

        await service.Invoking(s => s.PublishAsync(quiz.Id, new PublishQuizRequest { ClassGroupIds = [cls.Id] }))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Quiz not found.");
    }

    [Fact]
    public async Task UnpublishAsync_LastAssignment_ClearsPublishedAtUtc()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var first = NewClass(center, 3, "Math");
        var second = NewClass(center, 3, "English");
        var ctx = MockDbContext.Create(centers: [center], classGroups: [first, second]);
        var service = CreateService(ctx, teacherId: 3);
        var quiz = await service.CreateAsync(McqRequest());
        await service.PublishAsync(quiz.Id, new PublishQuizRequest { ClassGroupIds = [first.Id, second.Id] });

        var afterFirst = await service.UnpublishAsync(quiz.Id, first.Id);
        afterFirst.PublishedAtUtc.Should().NotBeNull();

        var afterSecond = await service.UnpublishAsync(quiz.Id, second.Id);
        afterSecond.PublishedAtUtc.Should().BeNull();
        ctx.QuizAssignments.Should().NotContain(a => a.QuizId == quiz.Id);
    }

    [Fact]
    public async Task UnpublishAsync_NotAssignedClass_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);
        var quiz = await service.CreateAsync(McqRequest());

        await service.Invoking(s => s.UnpublishAsync(quiz.Id, cls.Id))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Quiz is not assigned to this class.");
    }

    [Fact]
    public async Task DeleteAsync_RemovesQuizAndAssignments()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);
        var quiz = await service.CreateAsync(McqRequest());
        await service.PublishAsync(quiz.Id, new PublishQuizRequest { ClassGroupIds = [cls.Id] });

        await service.DeleteAsync(quiz.Id);

        ctx.Quizzes.Should().BeEmpty();
        ctx.QuizAssignments.Should().NotContain(a => a.QuizId == quiz.Id);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOwnQuizzesWithQuestionCountsAndPublishedNames()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var student = NewStudent("Omar", "01000000001", 3);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);
        var quiz = await service.CreateAsync(McqRequest());
        await service.PublishAsync(quiz.Id, new PublishQuizRequest { ClassGroupIds = [cls.Id] });
        ctx.QuizAttempts.Add(new QuizAttempt { QuizId = quiz.Id, StudentId = student.Id, CorrectCount = 2, TotalCount = 2 });
        await ctx.SaveChangesAsync();

        var result = await service.GetAllAsync();

        var item = result.Should().ContainSingle().Subject;
        item.QuestionCount.Should().Be(2);
        item.AttemptCount.Should().Be(1);
        item.PublishedClassNames.Should().Equal("Math Saturday 4PM");
        item.PublishedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDetailAsync_ReturnsQuestionsInSortOrder()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);
        var quiz = await service.CreateAsync(McqRequest());

        var result = await service.GetDetailAsync(quiz.Id);

        result.Questions.Select(q => q.Question).Should().Equal("1+1?", "2+2?");
        result.Questions[1].Options.Should().Equal("1", "2", "3", "4");
    }

    [Fact]
    public async Task GetResultsAsync_FirstAttemptPlusEnrolledNotTaken()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var omar = NewStudent("Omar", "01000000001", 3);
        var ali = NewStudent("Ali", "01000000002", 3);
        var quiz = NewPublishedQuiz(cls);
        var ctx = MockDbContext.Create(centers: [center], students: [omar, ali], classGroups: [cls],
            enrollments:
            [
                new Enrollment { Student = omar, ClassGroup = cls },
                new Enrollment { Student = ali, ClassGroup = cls }
            ],
            quizzes: [quiz]);
        ctx.QuizAttempts.AddRange(
            new QuizAttempt { Quiz = quiz, Student = omar, SubmittedAtUtc = new DateTime(2026, 8, 20, 9, 0, 0, DateTimeKind.Utc), CorrectCount = 1, TotalCount = 2 },
            new QuizAttempt { Quiz = quiz, Student = omar, SubmittedAtUtc = new DateTime(2026, 8, 21, 9, 0, 0, DateTimeKind.Utc), CorrectCount = 2, TotalCount = 2 });
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx, teacherId: 3);

        var result = await service.GetResultsAsync(quiz.Id);

        result.Should().HaveCount(2);
        var omarScore = result.Single(s => s.StudentId == omar.Id);
        omarScore.HasTaken.Should().BeTrue();
        omarScore.FirstAttemptCorrect.Should().Be(1);
        omarScore.FirstAttemptTotal.Should().Be(2);
        omarScore.AttemptCount.Should().Be(2);
        var aliScore = result.Single(s => s.StudentId == ali.Id);
        aliScore.HasTaken.Should().BeFalse();
        aliScore.FirstAttemptCorrect.Should().BeNull();
        aliScore.AttemptCount.Should().Be(0);
    }

    [Fact]
    public async Task GetResultsAsync_QuizOfAnotherTeacher_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], classGroups: [cls]);
        var foreignService = CreateService(ctx, teacherId: 9);
        var quiz = await foreignService.CreateAsync(McqRequest("Foreign Quiz"));

        var service = CreateService(ctx, teacherId: 3);

        await service.Invoking(s => s.GetResultsAsync(quiz.Id))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Quiz not found.");
    }

    private sealed class StubCurrentUser(int? teacherId) : ICurrentUserService
    {
        public int? UserId => teacherId;
        public Domain.Enums.Role? Role => teacherId.HasValue ? Domain.Enums.Role.Teacher : null;
        public int? TeacherId => teacherId;
    }
}