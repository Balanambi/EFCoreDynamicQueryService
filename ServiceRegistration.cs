
// Dependency injection setup (in Program.cs or Startup.cs)
public class ServiceRegistration
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IDynamicQueryService, DynamicQueryService>();
    }
}