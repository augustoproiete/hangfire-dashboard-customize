using System;

namespace Hangfire
{
    /// <summary>
    /// Custom configuration options for the Hangfire Dashboard
    /// </summary>
    public class HangfireDashboardCustomOptions
    {
        /// <summary>
        /// The title to display in the HTML header title (e.g. browser tab) of the Hangfire Dashboard
        /// </summary>
        public Func<string> BrowserTitle { get; set; }

        /// <summary>
        /// The title to display in the navigation bar of the Hangfire Dashboard
        /// </summary>
        public Func<string> DashboardTitle { get; set; }
    }
}
