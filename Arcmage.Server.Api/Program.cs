using Arcmage.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Events;




namespace Arcmage.Server.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {

       
            var webHostBuilder = WebHost.CreateDefaultBuilder(args);
            webHostBuilder.UseStartup<Startup>();
            webHostBuilder.UseSerilog(ConfigureSerilog);
            webHostBuilder.ConfigureKestrel(ConfigureKerstrel);
            webHostBuilder.UseUrls(Settings.Current.ApiListenUrls);

            var webHost = webHostBuilder.Build();
            webHost.Run();
        }

        private static void ConfigureKerstrel(KestrelServerOptions options)
        {
            options.Limits.MaxRequestBodySize = null;
        }

        private static void ConfigureSerilog(WebHostBuilderContext context, LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration.Enrich.FromLogContext()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                // N.G Remark : commit this out to log queries, both in the general log and separately to the file defined below
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database", LogEventLevel.Information)
                // N.G. Remark: change this to set the default minimum level
                .MinimumLevel.Information();

               // .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(Matching.FromSource("Microsoft.EntityFrameworkCore.Database")).WriteTo.File("logs\\log_queries.txt", rollOnFileSizeLimit: true, retainedFileCountLimit: 10));

            loggerConfiguration.WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day);

        }


    }
}
