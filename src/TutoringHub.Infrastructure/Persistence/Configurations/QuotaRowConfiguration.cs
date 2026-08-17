using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoringHub.Domain.Entities;

namespace TutoringHub.Infrastructure.Persistence.Configurations;

public class QuotaRowConfiguration : IEntityTypeConfiguration<QuotaRow>
{
    public void Configure(EntityTypeBuilder<QuotaRow> builder)
    {
        builder.ToTable("QuotaRows");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.TotalSessions).IsRequired();
        builder.Property(q => q.RemainingSessions).IsRequired();
        builder.Property(q => q.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(q => q.PeriodStart).IsRequired();
        builder.Property(q => q.PeriodEnd).IsRequired();
        builder.Property(q => q.CreatedAt).IsRequired();
        builder.HasOne(q => q.Student)
            .WithMany()
            .HasForeignKey(q => q.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(q => q.ClassGroup)
            .WithMany()
            .HasForeignKey(q => q.ClassGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}