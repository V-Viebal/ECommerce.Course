namespace Viebal.ECommerce.Course.OAuth.Infrastructure.Data;

class DatabaseOptions
{
    public Seeding? Seeding { get; set; }
}

sealed class Seeding
{
    public IEnumerable<UserOption>? Users { get; set; }
}

sealed class UserOption
{
    public string? Email { get; set; }

    public string? Password { get; set; }
}
