
using Viebal.ECommerce.Course.OAuth.API.Infrastructure;

namespace Viebal.ECommerce.Course.OAuth.API.Endpoints;

class CourseEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication webApp)
    {
        var router = webApp.MapGroup(this, "Courses");
    }
}
