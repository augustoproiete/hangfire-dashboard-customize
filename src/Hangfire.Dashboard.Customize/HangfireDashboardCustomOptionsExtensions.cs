using System;
#if ASPNETCORE
using Microsoft.AspNetCore.Builder;
#elif OWIN
using Owin;
#endif

namespace Hangfire
{
#if ASPNETCORE
    using IApplicationBuilder = IApplicationBuilder;
#elif OWIN
    using IApplicationBuilder = IAppBuilder;
#endif

#if ASPNETCORE
    /// <summary>
    /// Provides extension methods for the <c>IApplicationBuilder</c> interface
    /// defined in the <see href="https://www.nuget.org/packages/Microsoft.AspNetCore.Http.Abstractions/">Microsoft.AspNetCore.Http.Abstractions</see>
    /// NuGet package to simplify the integration with AspNetCore applications
    /// </summary>
    public static class HangfireDashboardCustomOptionsExtensions
#elif OWIN
    /// <summary>
    /// Provides extension methods for the <c>IAppBuilder</c> interface
    /// defined in the <see href="https://www.nuget.org/packages/Owin/">Owin</see>
    /// NuGet package to simplify the integration with OWIN applications
    /// </summary>
    public static class HangfireDashboardCustomOptionsExtensions
#endif
    {
        /// <summary>
        /// Configure custom options for the Hangfire Dashboard
        /// </summary>
        /// <param name="app">The application builder instance</param>
        /// <param name="options">The custom configuration options for the Hangfire Dashboard</param>
        /// <returns></returns>
        public static IApplicationBuilder UseHangfireDashboardCustomOptions(this IApplicationBuilder app, HangfireDashboardCustomOptions options)
        {
            if (app is null) throw new ArgumentNullException(nameof(app));
            if (options is null) throw new ArgumentNullException(nameof(options));

#if ASPNETCORE
            return app.UseMiddleware<HangfireDashboardCustomOptionsMiddleware>(options);
#elif OWIN
            return app.Use<HangfireDashboardCustomOptionsMiddleware>(options);
#endif
        }
    }
}
