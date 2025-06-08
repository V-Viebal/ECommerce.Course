using Microsoft.EntityFrameworkCore;
using Viebal.ECommerce.Course.OAuth.Domain.Entities;
using Viebal.ECommerce.Course.OAuth.Infrastructure.Data.Configurations;

namespace Viebal.ECommerce.Course.OAuth.Infrastructure.Data;

public class AppDbContext(DbContextOptions opts) : DbContext(opts)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //var assembly = Assembly.GetExecutingAssembly();
        //modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        modelBuilder.ApplyConfiguration<User>(new UserConfiguration());
        modelBuilder.ApplyConfiguration<RefreshToken>(new RefreshTokenConfiguration());
    }
}
