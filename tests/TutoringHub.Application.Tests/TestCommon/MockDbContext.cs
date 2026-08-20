using Microsoft.EntityFrameworkCore;
using TutoringHub.Domain.Entities;
using TutoringHub.Infrastructure.Persistence;

namespace TutoringHub.Application.Tests.TestCommon;

public static class MockDbContext
{
    public static ApplicationDbContext Create(
        List<Teacher>? teachers = null,
        List<Student>? students = null,
        List<Center>? centers = null,
        List<ClassGroup>? classGroups = null,
        List<Enrollment>? enrollments = null,
        List<RefreshToken>? refreshTokens = null,
        List<ClassSession>? classSessions = null,
        List<Attendance>? attendances = null,
        List<QuotaRow>? quotaRows = null,
        List<Quiz>? quizzes = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        if (teachers is not null) context.Teachers.AddRange(teachers);
        if (students is not null) context.Students.AddRange(students);
        if (centers is not null) context.Centers.AddRange(centers);
        if (classGroups is not null) context.ClassGroups.AddRange(classGroups);
        if (enrollments is not null) context.Enrollments.AddRange(enrollments);
        if (refreshTokens is not null) context.RefreshTokens.AddRange(refreshTokens);
        if (classSessions is not null) context.ClassSessions.AddRange(classSessions);
        if (attendances is not null) context.Attendances.AddRange(attendances);
        if (quotaRows is not null) context.QuotaRows.AddRange(quotaRows);
        if (quizzes is not null) context.Quizzes.AddRange(quizzes);

        context.SaveChanges();
        return context;
    }
}