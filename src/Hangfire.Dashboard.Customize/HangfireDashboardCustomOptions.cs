using System;

namespace Hangfire
{
    /// <summary>
    /// Custom configuration options for the Hangfire Dashboard
    /// </summary>
    public class HangfireDashboardCustomOptions
    {
        /// <summary>
        /// The title to display in the Hangfire Dashboard
        /// </summary>
        public Func<string> DashboardTitle { get; set; }
    }
}
