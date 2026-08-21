using FluentAssertions;
using TutoringHub.Application.DTOs.Quizzes;
using TutoringHub.Application.Services;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Application.Tests.TestCommon;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Enums;
using TutoringHub.Infrastructure.Persistence;

namespace TutoringHub.Application.Tests.Services;

public class StudentQuizServiceTests
{
    private static StudentQuizService CreateService(ApplicationDbContext context, int studentId) =>
        new(context, new StubStudentUser(studentId));

    private static StudentQuizService CreateTeacherService(ApplicationDbContext context) =>
        new(context, new StubTeacherUser());

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

    private static Quiz NewPublishedTrueFalseQuiz(ClassGroup cls)
    {
        var quiz = new Quiz
        {
            TeacherId = cls.TeacherId,
            Title = "Basics Quiz",
            QuestionType = QuestionType.TrueFalse,
            PublishedAtUtc = DateTime.UtcNow,
            Questions =
            [
                new QuizQuestion { SortOrder = 0, Question = "Sky is blue", Options = "[]", IsTrue = true },
                new QuizQuestion { SortOrder = 1, Question = "Winter is hot", Options = "[]", IsTrue = false }
            ]
        };
        quiz.Assignments.Add(new QuizAssignment { ClassGroup = cls });
        return quiz;
    }

    private static SubmitAttemptRequest McqAnswers(params int[] optionIndices) =>
        new() { Answers = optionIndices.Select(i => new AnswerItemDto { OptionIndex = i }).ToList() };

    [Fact]
    public async Task ListAsync_ReturnsPublishedQuizzesOfEnrolledClassesOnly()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var other = NewClass(center, 3, "English Sunday 3PM");
        var student = NewStudent("Omar", "01000000001", 3);
        var quiz = NewPublishedQuiz(cls);
        var otherQuiz = NewPublishedQuiz(other, "English Quiz");
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls, other],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }],
            quizzes: [quiz, otherQuiz]);
        var service = CreateService(ctx, student.Id);

        var result = await service.ListAsync();

        result.Should().ContainSingle().Which.Title.Should().Be("Math Quiz");
    }

    [Fact]
    public async Task ListAsync_IncludesAttemptCountAndLastScore()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var student = NewStudent("Omar", "01000000001", 3);
        var quiz = NewPublishedQuiz(cls);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }],
            quizzes: [quiz]);
        ctx.QuizAttempts.AddRange(
            new QuizAttempt { Quiz = quiz, Student = student, SubmittedAtUtc = new DateTime(2026, 8, 20, 9, 0, 0, DateTimeKind.Utc), CorrectCount = 1, TotalCount = 2 },
            new QuizAttempt { Quiz = quiz, Student = student, SubmittedAtUtc = new DateTime(2026, 8, 21, 9, 0, 0, DateTimeKind.Utc), CorrectCount = 2, TotalCount = 2 });
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx, student.Id);

        var result = await service.ListAsync();

        var item = result.Should().ContainSingle().Subject;
        item.QuestionCount.Should().Be(2);
        item.AttemptCount.Should().Be(2);
        item.LastCorrectCount.Should().Be(2);
        item.LastTotalCount.Should().Be(2);
        item.Taken.Should().BeTrue();
    }

    [Fact]
    public async Task ListAsync_ExcludesUnpublishedQuizEvenIfAssigned()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var student = NewStudent("Omar", "01000000001", 3);
        var draft = NewPublishedQuiz(cls, "Draft Quiz");
        draft.PublishedAtUtc = null;
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }],
            quizzes: [draft]);
        var service = CreateService(ctx, student.Id);

        var result = await service.ListAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAsync_ReturnsQuestionsWithoutAnswerKey()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var student = NewStudent("Omar", "01000000001", 3);
        var quiz = NewPublishedQuiz(cls);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }],
            quizzes: [quiz]);
        var service = CreateService(ctx, student.Id);

        var result = await service.GetAsync(quiz.Id);

        result.Title.Should().Be("Math Quiz");
        result.QuestionType.Should().Be(QuestionType.MultipleChoice);
        result.Questions.Should().HaveCount(2);
        result.Questions[0].Question.Should().Be("1+1?");
        result.Questions[0].Options.Should().Equal("1", "2", "3", "4");
    }

    [Fact]
    public async Task GetAsync_UnpublishedQuiz_ThrowsQuizNotFound()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var student = NewStudent("Omar", "01000000001", 3);
        var draft = NewPublishedQuiz(cls);
        draft.PublishedAtUtc = null;
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }],
            quizzes: [draft]);
        var service = CreateService(ctx, student.Id);

        await service.Invoking(s => s.GetAsync(draft.Id))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Quiz not found.");
    }

    [Fact]
    public async Task GetAsync_QuizNotAssignedToEnrolledClass_ThrowsQuizNotFound()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var other = NewClass(center, 3, "English Sunday 3PM");
        var student = NewStudent("Omar", "01000000001", 3);
        var quiz = NewPublishedQuiz(other);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls, other],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }],
            quizzes: [quiz]);
        var service = CreateService(ctx, student.Id);

        await service.Invoking(s => s.GetAsync(quiz.Id))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Quiz not found.");
    }

    [Fact]
    public async Task GetAsync_NotStudentRole_ThrowsUnauthorizedAccessException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var ctx = MockDbContext.Create(centers: [center]);
        var service = CreateTeacherService(ctx);

        await service.Invoking(s => s.GetAsync(1))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task SubmitAsync_McqAnswers_GradesAndRecordsAttempt()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var student = NewStudent("Omar", "01000000001", 3);
        var quiz = NewPublishedQuiz(cls);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }],
            quizzes: [quiz]);
        var service = CreateService(ctx, student.Id);

        var result = await service.SubmitAsync(quiz.Id, McqAnswers(1, 0));

        result.CorrectCount.Should().Be(1);
        result.TotalCount.Should().Be(2);
        result.Percentage.Should().Be(50.0);
        result.IsFirstAttempt.Should().BeTrue();
        result.Results[0].IsCorrect.Should().BeTrue();
        result.Results[0].YourIndex.Should().Be(1);
        result.Results[0].CorrectIndex.Should().Be(1);
        result.Results[1].IsCorrect.Should().BeFalse();
        result.Results[1].YourIndex.Should().Be(0);
        result.Results[1].CorrectIndex.Should().Be(3);
        ctx.QuizAttempts.Should().ContainSingle(a => a.StudentId == student.Id)
            .Which.CorrectCount.Should().Be(1);
    }

    [Fact]
    public async Task SubmitAsync_TrueFalseAnswers_GradesCorrectly()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var student = NewStudent("Omar", "01000000001", 3);
        var quiz = NewPublishedTrueFalseQuiz(cls);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }],
            quizzes: [quiz]);
        var service = CreateService(ctx, student.Id);

        var result = await service.SubmitAsync(quiz.Id, new SubmitAttemptRequest
        {
            Answers =
            [
                new AnswerItemDto { IsTrue = true },
                new AnswerItemDto { IsTrue = true }
            ]
        });

        result.CorrectCount.Should().Be(1);
        result.Results[0].YourAnswer.Should().BeTrue();
        result.Results[0].CorrectAnswer.Should().BeTrue();
        result.Results[1].YourAnswer.Should().BeTrue();
        result.Results[1].CorrectAnswer.Should().BeFalse();
    }

    [Fact]
    public async Task SubmitAsync_SecondAttempt_IsFirstAttemptFalse()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var student = NewStudent("Omar", "01000000001", 3);
        var quiz = NewPublishedQuiz(cls);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }],
            quizzes: [quiz]);
        var service = CreateService(ctx, student.Id);

        await service.SubmitAsync(quiz.Id, McqAnswers(1, 3));
        var second = await service.SubmitAsync(quiz.Id, McqAnswers(0, 1));

        second.IsFirstAttempt.Should().BeFalse();
        ctx.QuizAttempts.Should().HaveCount(2);
    }

    [Fact]
    public async Task SubmitAsync_WrongShapeForMcq_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var student = NewStudent("Omar", "01000000001", 3);
        var quiz = NewPublishedQuiz(cls);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }],
            quizzes: [quiz]);
        var service = CreateService(ctx, student.Id);

        await service.Invoking(s => s.SubmitAsync(quiz.Id, new SubmitAttemptRequest
            {
                Answers = [new AnswerItemDto { IsTrue = true }, new AnswerItemDto { OptionIndex = 3 }]
            }))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid answer format.");
    }

    [Fact]
    public async Task SubmitAsync_AnswerCountMismatch_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var student = NewStudent("Omar", "01000000001", 3);
        var quiz = NewPublishedQuiz(cls);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }],
            quizzes: [quiz]);
        var service = CreateService(ctx, student.Id);

        await service.Invoking(s => s.SubmitAsync(quiz.Id, McqAnswers(1)))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Answer count does not match the quiz questions.");
    }

    [Fact]
    public async Task SubmitAsync_NotStudentRole_ThrowsUnauthorizedAccessException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var ctx = MockDbContext.Create(centers: [center]);
        var service = CreateTeacherService(ctx);

        await service.Invoking(s => s.SubmitAsync(1, McqAnswers(1)))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class StubStudentUser(int studentId) : ICurrentUserService
    {
        public int? UserId => studentId;
        public Domain.Enums.Role? Role => Domain.Enums.Role.Student;
        public int? TeacherId => null;
    }

    private sealed class StubTeacherUser : ICurrentUserService
    {
        public int? UserId => 3;
        public Domain.Enums.Role? Role => Domain.Enums.Role.Teacher;
        public int? TeacherId => 3;
    }
}