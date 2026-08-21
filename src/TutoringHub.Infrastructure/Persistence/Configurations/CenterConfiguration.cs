using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoringHub.Domain.Entities;

namespace TutoringHub.Infrastructure.Persistence.Configurations;

public class CenterConfiguration : IEntityTypeConfiguration<Center>
{
    public void Configure(EntityTypeBuilder<Center> builder)
    {
        builder.ToTable("Centers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.LocationDetails).HasMaxLength(300).IsRequired();
        builder.HasOne(c => c.Teacher)
            .WithMany(t => t.Centers)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}