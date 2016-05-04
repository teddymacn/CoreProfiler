using Microsoft.AspNetCore.Builder;

namespace CoreProfiler.Web
{
    public static class CoreProfilerMiddlewareExtensions
{
    public static IApplicationBuilder UseCoreProfiler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CoreProfilerMiddleware>();
    }
}
}