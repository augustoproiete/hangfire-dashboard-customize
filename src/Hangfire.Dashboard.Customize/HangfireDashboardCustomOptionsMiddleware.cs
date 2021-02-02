using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#if ASPNETCORE
using Microsoft.AspNetCore.Http;
#endif

namespace Hangfire
{
#if ASPNETCORE
    using RequestDelegate = RequestDelegate;
    using HttpContext = HttpContext;
#elif OWIN
    using RequestDelegate = Func<System.Collections.Generic.IDictionary<string, object>, Task>;
    using HttpContext = Microsoft.Owin.IOwinContext;
#endif

    internal class HangfireDashboardCustomOptionsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HangfireDashboardCustomOptions _options;
        private readonly Regex _titleRegex = new Regex(@"(\s*)(Hangfire\ Dashboard)(\s*)", RegexOptions.Compiled);

        public HangfireDashboardCustomOptionsMiddleware(RequestDelegate next, HangfireDashboardCustomOptions options)
        {
            _next = next;
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

#if ASPNETCORE
        // ReSharper disable once UnusedMember.Global
        public async Task Invoke(HttpContext context)
#elif OWIN
        // ReSharper disable once UnusedMember.Global
        public async Task Invoke(System.Collections.Generic.IDictionary<string, object> environment)
#endif
        {
#if OWIN
            var context = new Microsoft.Owin.OwinContext(environment);
#endif

            if (!IsHtmlPageRequest(context))
            {
#if ASPNETCORE
                await _next.Invoke(context);
#elif OWIN
                await _next.Invoke(environment);
#endif
                return;
            }

            var originalBody = context.Response.Body;

            using (var newBody = new MemoryStream())
            {
                context.Response.Body = newBody;

#if ASPNETCORE
                await _next.Invoke(context);
#elif OWIN
                await _next.Invoke(environment);
#endif

                context.Response.Body = originalBody;

                newBody.Seek(0, SeekOrigin.Begin);

                string newContent;
                using (var reader = new StreamReader(newBody, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                {
                    newContent = await reader.ReadToEndAsync();
                }

                var newBrowserTitle = _options?.BrowserTitle?.Invoke();
                var newDashboardTitle = _options?.DashboardTitle?.Invoke();

                if (!string.IsNullOrWhiteSpace(newContent))
                {
                    var indexOfBody = newContent.IndexOf("<body>", StringComparison.OrdinalIgnoreCase);
                    if (indexOfBody >= 0)
                    {
                        if (!string.IsNullOrWhiteSpace(newBrowserTitle))
                        {
                            newContent = _titleRegex.Replace(newContent, $"$1{newBrowserTitle}$3", 1);
                        }

                        if (!string.IsNullOrWhiteSpace(newDashboardTitle))
                        {
                            newContent = _titleRegex.Replace(newContent, $"$1{newDashboardTitle}$3", 1, indexOfBody);
                        }
                    }
                }

                await context.Response.WriteAsync(newContent);
            }
        }

        private static bool IsHtmlPageRequest(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("Accept", out var accept)) return false;
            if (!accept.Any(a => a.IndexOf("text/html", StringComparison.OrdinalIgnoreCase) >= 0)) return false;

            return true;
        }
    }
}
