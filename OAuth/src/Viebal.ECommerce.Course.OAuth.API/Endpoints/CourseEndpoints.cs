using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Viebal.ECommerce.Course.OAuth.API.Infrastructure;
using Viebal.ECommerce.Course.OAuth.Domain.Entities;
using Viebal.ECommerce.Course.OAuth.Domain.ValueObjects;
using Viebal.ECommerce.Course.OAuth.Infrastructure.Data;

namespace Viebal.ECommerce.Course.OAuth.API.Endpoints;

class CourseEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication webApp)
    {
        var router = webApp.MapGroup(this, "Courses");

        // Get all courses
        router.Map(GetAllCourses);

        // Get course by id
        router.Map(GetCourseById);

        // Create new course
        router.Map(CreateCourse);

        // Update course
        router.Map(UpdateCourse);

        // Delete course
        router.Map(DeleteCourse);
    }

    [AllowAnonymous]
    [HttpGet("")]
    async Task<IResult> GetAllCourses(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? level,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CourseDbContext dbContext = null!,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Courses.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Title.Contains(search) ||
                                   (c.Description != null && c.Description.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(c => c.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(level))
        {
            query = query.Where(c => c.Level == level);
        }

        // Calculate pagination
        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // Apply pagination
        var courses = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Results.Ok(new
        {
            Items = courses,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        });
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    async Task<IResult> GetCourseById(
        [FromRoute] Guid id,
        CourseDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var course = await dbContext.Courses
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (course is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(course);
    }

    [HttpPost("")]
    async Task<IResult> CreateCourse(
        [FromBody] CourseCreateRequest request,
        CourseDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Create a new course entity
        var course = new Courses
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Image = request.Image,
            Status = request.Status,
            Level = request.Level,
            DurationText = request.DurationText,
            Instructor = request.Instructor,
            Category = request.Category,
            Tags = request.Tags ?? new List<string>(),
            Prerequisites = request.Prerequisites ?? new List<string>(),
            Features = request.Features ?? new List<string>(),
            Topics = request.Topics ?? new List<string>(),
            CareerIDs = request.CareerIDs ?? new List<Guid>(),
            GroupIDs = request.GroupIDs ?? new List<Guid>(),
            TotalLessons = 0,
            TotalDuration = new Duration
            {
                Days = request.TotalDuration?.Days,
                Hours = request.TotalDuration?.Hours,
                Minutes = request.TotalDuration?.Minutes
            },
            Rating = 0,
            StudentsCount = 0
        };

        await dbContext.Courses.AddAsync(course, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/v1/courses/{course.Id}", course);
    }

    [HttpPut("{id}")]
    async Task<IResult> UpdateCourse(
        [FromRoute] Guid id,
        [FromBody] CourseUpdateRequest request,
        CourseDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var course = await dbContext.Courses.FindAsync(new object[] { id }, cancellationToken);

        if (course is null)
        {
            return Results.NotFound();
        }

        // Update course properties
        course.Title = request.Title;
        course.Description = request.Description;
        course.Image = request.Image;
        course.Status = request.Status;
        course.Level = request.Level;
        course.DurationText = request.DurationText;
        course.Instructor = request.Instructor;
        course.Category = request.Category;
        course.Tags = request.Tags ?? course.Tags;
        course.Prerequisites = request.Prerequisites ?? course.Prerequisites;
        course.Features = request.Features ?? course.Features;
        course.Topics = request.Topics ?? course.Topics;
        course.CareerIDs = request.CareerIDs ?? course.CareerIDs;
        course.GroupIDs = request.GroupIDs ?? course.GroupIDs;
        course.TotalDuration = new Duration
        {
            Days = request.TotalDuration?.Days,
            Hours = request.TotalDuration?.Hours,
            Minutes = request.TotalDuration?.Minutes
        };

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(course);
    }

    [HttpDelete("{id}")]
    async Task<IResult> DeleteCourse(
        [FromRoute] Guid id,
        CourseDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var course = await dbContext.Courses.FindAsync(new object[] { id }, cancellationToken);

        if (course is null)
        {
            return Results.NotFound();
        }

        dbContext.Courses.Remove(course);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}

// Request DTOs
class CourseCreateRequest
{
    public string Title { get; set; } = default!;
    public string? Image { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Level { get; set; }
    public string? DurationText { get; set; }
    public string? Instructor { get; set; }
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
    public List<string>? Prerequisites { get; set; }
    public List<string>? Features { get; set; }
    public List<string>? Topics { get; set; }
    public List<Guid>? CareerIDs { get; set; }
    public List<Guid>? GroupIDs { get; set; }
    public Duration? TotalDuration { get; set; }
}

class CourseUpdateRequest : CourseCreateRequest
{
}
