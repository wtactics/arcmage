using System.Security.Cryptography.X509Certificates;
using Arcmage.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Serilog;
using Serilog.Events;

namespace Arcmage.Game.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHostBuilder = WebHost.CreateDefaultBuilder(args);
            webHostBuilder.UseStartup<Startup>();
            webHostBuilder.UseSerilog(ConfigureSerilog);
            webHostBuilder.ConfigureKestrel(ConfigureKerstrel);
            webHostBuilder.UseUrls(Settings.Current.GameApiListenUrls);

            var webHost = webHostBuilder.Build();
            webHost.Run();
        }

        private static void ConfigureKerstrel(KestrelServerOptions options)
        {
            options.Limits.MaxRequestBodySize = null;
            if (Settings.Current.GameRuntimeUseHttps)
            {
                options.ConfigureHttpsDefaults(httpsOptions =>
                {
                    httpsOptions.ServerCertificateSelector = ServerCertificateSelector;
                });
            }
           
        }

        private static X509Certificate2 ServerCertificateSelector(ConnectionContext connection, string sni)
        {
            return CertificateLoader.LoadFromStoreCert(Settings.Current.GameRuntimeSslSubject, Settings.Current.GameRuntimeSslStore, StoreLocation.LocalMachine, false); ;
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

            loggerConfiguration.WriteTo.File("gamelogs\\log.txt", rollingInterval: RollingInterval.Day);

        }

    }
}
