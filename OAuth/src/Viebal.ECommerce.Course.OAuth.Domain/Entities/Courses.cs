using Viebal.ECommerce.Course.OAuth.Domain.ValueObjects;

namespace Viebal.ECommerce.Course.OAuth.Domain.Entities;

public class Courses
{
    public Guid Id { get; private set; }

    public string Title { get; private set; } = default!;
    public string? Image { get; private set; }
    public string? Description { get; private set; }
    public string? Status { get; private set; }

    // ✅ Flattened Metadata Fields
    public string? Level { get; private set; }
    public string? DurationText { get; private set; }
    public string? Instructor { get; private set; }
    public string? Category { get; private set; }

    public List<string> Tags { get; private set; } = new();
    public List<string> Prerequisites { get; private set; } = new();
    public List<string> Features { get; private set; } = new();
    public List<string> Topics { get; private set; } = new();
    public List<Guid> CareerIDs { get; private set; } = new();
    public List<Guid> GroupIDs { get; private set; } = new();

    // ✅ Aggregated Data
    public int? TotalLessons { get; private set; }
    public Duration? TotalDuration { get; private set; }
    public double? Rating { get; private set; }
    public int? StudentsCount { get; private set; }

    public ICollection<Lesson> Lessons { get; private set; } = new List<Lesson>();
}
