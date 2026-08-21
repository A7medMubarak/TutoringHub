using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoringHub.Domain.Entities;

namespace TutoringHub.Infrastructure.Persistence.Configurations;

public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.ToTable("QuizAttempts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.SubmittedAtUtc).IsRequired();
        builder.Property(a => a.CorrectCount).IsRequired();
        builder.Property(a => a.TotalCount).IsRequired();
        builder.HasOne(a => a.Quiz)
            .WithMany(q => q.Attempts)
            .HasForeignKey(a => a.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}