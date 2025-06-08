using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viebal.ECommerce.Course.OAuth.Domain.Entities;

namespace Viebal.ECommerce.Course.OAuth.Infrastructure.Data.Configurations;
class CourseConfiguration : IEntityTypeConfiguration<Courses>
{
    public void Configure(EntityTypeBuilder<Courses> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);

        // Navigation
        builder.HasMany(x => x.Lessons)
            .WithMany(x => x.Courses);

        builder.OwnsOne(x => x.TotalDuration, duration =>
        {
            duration.Property(d => d.Days).HasColumnName("TotalDays");
            duration.Property(d => d.Hours).HasColumnName("TotalHours");
            duration.Property(d => d.Minutes).HasColumnName("TotalMinutes");
            duration.ToJson();
        });
    }
}
