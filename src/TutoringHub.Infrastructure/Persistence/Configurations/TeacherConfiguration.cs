using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutoringHub.Domain.Entities;

namespace TutoringHub.Infrastructure.Persistence.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.FullName).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Username).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Phone).HasMaxLength(20).IsRequired();
        builder.Property(t => t.PasswordHash).HasMaxLength(200).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.HasIndex(t => t.Username).IsUnique();
        builder.HasIndex(t => t.Phone).IsUnique();
    }
}