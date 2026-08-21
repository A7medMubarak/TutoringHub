using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoringHub.Domain.Entities;

namespace TutoringHub.Infrastructure.Persistence.Configurations;

public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("QuizQuestions");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.SortOrder).IsRequired();
        builder.Property(q => q.Question).HasMaxLength(1000).IsRequired();
        builder.Property(q => q.Options).HasMaxLength(1000).IsRequired();
        builder.HasIndex(q => new { q.QuizId, q.SortOrder }).IsUnique();
        builder.HasOne(q => q.Quiz)
            .WithMany(q => q.Questions)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}