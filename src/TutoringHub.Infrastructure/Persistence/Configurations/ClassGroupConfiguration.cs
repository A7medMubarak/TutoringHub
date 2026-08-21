using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoringHub.Domain.Entities;

namespace TutoringHub.Infrastructure.Persistence.Configurations;

public class ClassGroupConfiguration : IEntityTypeConfiguration<ClassGroup>
{
    public void Configure(EntityTypeBuilder<ClassGroup> builder)
    {
        builder.ToTable("ClassGroups");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.DayOfWeek).HasConversion<int>().IsRequired();
        builder.Property(c => c.StartTime).IsRequired();
        builder.Property(c => c.Frequency).HasConversion<int>().IsRequired();
        builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
        builder.HasOne(c => c.Teacher)
            .WithMany(t => t.ClassGroups)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Center)
            .WithMany(c => c.ClassGroups)
            .HasForeignKey(c => c.CenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}