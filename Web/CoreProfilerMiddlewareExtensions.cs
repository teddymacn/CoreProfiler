using Microsoft.AspNetCore.Builder;

namespace EF.Diagnostics.Profiling.Web
{
    public static class CoreProfilerMiddlewareExtensions
{
    public static IApplicationBuilder UseCoreProfiler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CoreProfilerMiddleware>();
    }
}
}