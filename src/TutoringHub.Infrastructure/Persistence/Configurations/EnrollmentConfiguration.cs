using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoringHub.Domain.Entities;

namespace TutoringHub.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EnrolledAt).IsRequired();
        builder.HasIndex(e => new { e.StudentId, e.ClassGroupId }).IsUnique();
        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ClassGroup)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.ClassGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}