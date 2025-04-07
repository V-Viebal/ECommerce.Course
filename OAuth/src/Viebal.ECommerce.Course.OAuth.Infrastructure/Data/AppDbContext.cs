using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Viebal.ECommerce.Course.OAuth.Domain.Entities;

namespace Viebal.ECommerce.Course.OAuth.Infrastructure.Data;

public class AppDbContext(DbContextOptions opts) : DbContext(opts)
{
    public DbSet<Domain.Entities.User> Users => Set<Domain.Entities.User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var assembly = Assembly.GetExecutingAssembly();
        modelBuilder.ApplyConfigurationsFromAssembly(assembly);
    }
}
