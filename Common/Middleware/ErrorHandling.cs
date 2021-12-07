using Microsoft.AspNetCore.Builder;

namespace Common.Middleware
{
    public static class ErrorHandling
    {
        public static void ConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}