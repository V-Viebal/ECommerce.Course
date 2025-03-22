var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Viebal_ECommerce_Course_OAuth_API>("viebal-ecommerce-course-oauth-api");

builder.Build().Run();
