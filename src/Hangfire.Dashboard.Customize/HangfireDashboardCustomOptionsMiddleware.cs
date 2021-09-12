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
        private readonly Regex _browserTitleRegex = new Regex(@"(<title>.*\s*\-?\s*)(Hangfire|Hangfire\s+Dashboard)(\s*</title>)", RegexOptions.Compiled);
        private readonly Regex _dashboardTitleRegex = new Regex(@"(>\s*)(Hangfire\ Dashboard)(\s*<)", RegexOptions.Compiled);

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

                if (!string.IsNullOrWhiteSpace(newContent))
                {
                    var indexOfBody = newContent.IndexOf("<body>", StringComparison.OrdinalIgnoreCase);
                    if (indexOfBody >= 0)
                    {
                        var newBrowserTitle = _options?.BrowserTitle?.Invoke();
                        if (!string.IsNullOrWhiteSpace(newBrowserTitle))
                        {
                            newContent = _browserTitleRegex.Replace(newContent, $"$1{newBrowserTitle}$3", 1);
                        }

                        var newDashboardTitle = _options?.DashboardTitle?.Invoke();
                        if (!string.IsNullOrWhiteSpace(newDashboardTitle))
                        {
                            newContent = _dashboardTitleRegex.Replace(newContent, $"$1{newDashboardTitle}$3", 1, indexOfBody);
                        }
                    }

                    var appendToHeadContent = _options?.AppendToHead?.Invoke();
                    if (!string.IsNullOrWhiteSpace(appendToHeadContent))
                    {
                        var indexOfCloseHtmlHead = newContent.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
                        if (indexOfCloseHtmlHead >= 0)
                        {
                            // Covers both \r\n and \n
                            if (!appendToHeadContent.EndsWith("\n"))
                            {
                                appendToHeadContent += Environment.NewLine;
                            }

                            newContent = newContent.Insert(indexOfCloseHtmlHead, appendToHeadContent);
                        }
                    }

                    var appendToBodyContent = _options?.AppendToBody?.Invoke();
                    if (!string.IsNullOrWhiteSpace(appendToBodyContent))
                    {
                        var indexOfCloseHtmlBody = newContent.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                        if (indexOfCloseHtmlBody >= 0)
                        {
                            // Covers both \r\n and \n
                            if (!appendToBodyContent.EndsWith("\n"))
                            {
                                appendToBodyContent += Environment.NewLine;
                            }

                            newContent = newContent.Insert(indexOfCloseHtmlBody, appendToBodyContent);
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
