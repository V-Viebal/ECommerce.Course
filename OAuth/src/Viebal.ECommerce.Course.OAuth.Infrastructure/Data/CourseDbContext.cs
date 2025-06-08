using Microsoft.EntityFrameworkCore;
using Viebal.ECommerce.Course.OAuth.Domain.Entities;
using Viebal.ECommerce.Course.OAuth.Infrastructure.Data.Configurations;

namespace Viebal.ECommerce.Course.OAuth.Infrastructure.Data;

public class CourseDbContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<Courses> Courses => Set<Courses>();
    public DbSet<Lesson> Lessons => Set<Lesson>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration<Courses>(new CourseConfiguration());
        modelBuilder.ApplyConfiguration<Lesson>(new LessonConfiguration());
    }
}
