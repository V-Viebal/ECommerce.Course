using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viebal.ECommerce.Course.OAuth.Domain.Entities;

namespace Viebal.ECommerce.Course.OAuth.Infrastructure.Data.Configurations;

class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.HasIndex(x => x.Email)
            .IsUnique();
    }
}
