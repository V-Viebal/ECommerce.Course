using Viebal.ECommerce.Course.OAuth.Domain.ValueObjects;

namespace Viebal.ECommerce.Course.OAuth.Domain.Entities;

public class Lesson
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public int Order { get; private set; }
    public string? Content { get; private set; }
    public Uri? VideoUrl { get; private set; }

    public Duration? Duration { get; private set; }

    public ICollection<Courses> Courses { get; private set; } = new List<Courses>();
}

