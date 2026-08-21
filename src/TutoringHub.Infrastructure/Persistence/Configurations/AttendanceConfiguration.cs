using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoringHub.Domain.Entities;

namespace TutoringHub.Infrastructure.Persistence.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendances");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.ClassSessionId, a.StudentId }).IsUnique();
        builder.HasOne(a => a.ClassSession)
            .WithMany(s => s.Attendances)
            .HasForeignKey(a => a.ClassSessionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.QuotaRow)
            .WithMany()
            .HasForeignKey(a => a.QuotaRowId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}