using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoringHub.Domain.Entities;

namespace TutoringHub.Infrastructure.Persistence.Configurations;

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("Quizzes");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Title).HasMaxLength(120).IsRequired();
        builder.Property(q => q.QuestionType).IsRequired();
        builder.Property(q => q.CreatedAtUtc).IsRequired();
        builder.HasOne(q => q.Teacher)
            .WithMany()
            .HasForeignKey(q => q.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}