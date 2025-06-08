using Viebal.ECommerce.Course.OAuth.Domain.ValueObjects;

namespace Viebal.ECommerce.Course.OAuth.Domain.Entities;

public class Courses
{
    public Guid Id { get; set; }

    public string Title { get; set; } = default!;
    public string? Image { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }

    // ✅ Flattened Metadata Fields
    public string? Level { get; set; }
    public string? DurationText { get; set; }
    public string? Instructor { get; set; }
    public string? Category { get; set; }

    public List<string> Tags { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new();
    public List<string> Features { get; set; } = new();
    public List<string> Topics { get; set; } = new();
    public List<Guid> CareerIDs { get; set; } = new();
    public List<Guid> GroupIDs { get; set; } = new();

    // ✅ Aggregated Data
    public int? TotalLessons { get; set; }
    public Duration? TotalDuration { get; set; }
    public double? Rating { get; set; }
    public int? StudentsCount { get; set; }

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
