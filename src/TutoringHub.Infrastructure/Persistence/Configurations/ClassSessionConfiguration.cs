using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoringHub.Domain.Entities;

namespace TutoringHub.Infrastructure.Persistence.Configurations;

public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        builder.ToTable("ClassSessions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Date).IsRequired();
        builder.HasIndex(s => new { s.ClassGroupId, s.Date }).IsUnique();
        builder.HasOne(s => s.ClassGroup)
            .WithMany()
            .HasForeignKey(s => s.ClassGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}