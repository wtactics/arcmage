using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Arcmage.Configuration
{
    public class Settings
    {

        public static readonly Settings Current = new Settings();

        static Settings()
        {
            var configurationDirectory = Directory.GetCurrentDirectory();
            var configurationFile = "appsettings.json";
            var developmentConfigurationFile = "appsettings_development.json";
            var releaseConfigurationFile = "appsettings_release.json";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(configurationDirectory)
                .AddJsonFile(configurationFile, false)
                .AddJsonFile(releaseConfigurationFile, true)
                .AddJsonFile(developmentConfigurationFile, true)
                .Build();

            configuration.GetSection("App").Bind(Current);
            if (!Path.IsPathRooted(Current.RepositoryRootPah))
            {
                Current.RepositoryRootPah = Path.Combine(configurationDirectory, Current.RepositoryRootPah);
            }
            
        }
      

        public string ArcmageConnectionString { get; set; }

        public string TokenEncryptionKey { get; set; }

        public string RepositoryRootPah { get; set; }

        public string PortalUrl { get; set; }

        public string ApiUrl { get; set; }

        public string GameApiUrl { get; set; }

        public string ServiceUserName { get; set; }

        public string ServiceUserEmail { get; set; }

        public string ServiceUserPassword { get; set; }

        // Enable this setting to run the inkscape export system-process with the InkscapeUser and InkscapePassword below
        public bool ForceInkscapeUserImpersonate { get; set; }

        public string InkscapeUser { get; set; }

        public string InkscapePassword { get; set; }


    }
}
