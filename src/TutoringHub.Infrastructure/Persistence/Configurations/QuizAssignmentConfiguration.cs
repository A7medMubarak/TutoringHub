using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoringHub.Domain.Entities;

namespace TutoringHub.Infrastructure.Persistence.Configurations;

public class QuizAssignmentConfiguration : IEntityTypeConfiguration<QuizAssignment>
{
    public void Configure(EntityTypeBuilder<QuizAssignment> builder)
    {
        builder.ToTable("QuizAssignments");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.QuizId, a.ClassGroupId }).IsUnique();
        builder.HasOne(a => a.Quiz)
            .WithMany(q => q.Assignments)
            .HasForeignKey(a => a.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.ClassGroup)
            .WithMany()
            .HasForeignKey(a => a.ClassGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}