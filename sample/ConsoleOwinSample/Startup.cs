using Hangfire;
using Hangfire.MemoryStorage;
using Owin;

namespace ConsoleOwinSample
{
    public class Startup
    {
        // ReSharper disable once UnusedMember.Global
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();

            // The next three lines is all you need
            app.UseHangfireDashboardCustomOptions(new HangfireDashboardCustomOptions
            {
                DashboardTitle = () => "New Dashboard Title :)",
            });

            app.UseHangfireDashboard(string.Empty, new DashboardOptions
            {
                AppPath = null,
                IsReadOnlyFunc = context => true,
            });
        }
    }
}
