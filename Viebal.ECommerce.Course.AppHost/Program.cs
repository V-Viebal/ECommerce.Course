var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Viebal_ECommerce_Course>("viebal-ecommerce-course");

builder.Build().Run();
