using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Arcmage.Configuration
{
    public class Settings
    {
        // The current settings loaded from appsettings.json 
        public static readonly Settings Current = new Settings();

        #region api settings

        // Connection string to the arcmage database
        public string ArcmageConnectionString { get; set; }

        // File path to the parent directory of the Cards, Decks and CardTemplates folder (can be absolute or relative from the executing program
        public string RepositoryRootPah { get; set; }


        // Token Encryption Key, 20 char long, to encrypt login tokens
        public string TokenEncryptionKey { get; set; }

        // Url of the front end
        public string PortalUrl { get; set; }

        // Url of the backend
        public string ApiUrl { get; set; }

        // Listening ports for the backend
        public string ApiListenUrls { get; set; }

        // Send grid api key
        public string SendGridApiKey { get; set; }



        #region inkscape settings

        // Inkscape path, defaults to 'C:\Program Files\Inkscape\inkscape.exe', use '/bin/inkscape' on linux
        public string InkscapeExe { get; set; }

        // Inkscape version, set to '1.0.2' to use the newer inkscape command line syntax
        public string InkscapeVersion { get; set; }

        // Enable this setting to run the inkscape export system-process with the InkscapeUser and InkscapePassword below (windows only)
        public bool ForceInkscapeUserImpersonate { get; set; }

        public string InkscapeUser { get; set; }

        public string InkscapePassword { get; set; }

        #endregion inkscape settings





        #region matchbot/arcbot hosting

        // Arcbot is a matrix bot that supports matchmaking and game creation from a matrix room

        // Enable or disable hosting for the arcbot
        public bool HostMatchBot { get; set; }

        // Matrix home server to connect to
        public string MatrixHomeServer { get; set; }

        // Bot's matrix username 
        public string MatrixUser { get; set; }

        // Bot's matrix password
        public string MatrixPassword { get; set; }

        // Matrix rooms (raw room ids) to join and listen to commands
        public List<string> MatrixRoomIds { get; set; }


        #endregion matchbot/arcbot hosting


        #endregion api settings



        #region database seed

        // Before running the database seed, fill in the default service user info.
        // After seeding, please remove the settings (no longer need)

        // The default admin user name
        public string ServiceUserName { get; set; }

        // The default admin email (account name)
        public string ServiceUserEmail { get; set; }

        // The default admin password
        public string ServiceUserPassword { get; set; }


        #endregion database seed



        #region game runtime

        // Game api url 
        public string GameApiUrl { get; set; }

        // Listening ports for the game runtime
        public string GameApiListenUrls { get; set; }

        // Location of the game runtime dll
        public string GameRuntimePath { get; set; }

        // The system user and password for the gameruntime's parent process
        public string GameRuntimeUser { get; set; }

        public string GameRuntimeUserPassword { get; set; }

        // Setup the game runtime to use https, and fetch the ssl cert from the given SslStore with the given SslSubject
        public bool GameRuntimeUseHttps { get; set; }

        public string GameRuntimeSslStore { get; set; }

        public string GameRuntimeSslSubject { get; set; }

        #endregion game runtime

        

        #region construction
        
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
            if (string.IsNullOrWhiteSpace(Current.GameRuntimePath))
            {
                Current.GameRuntimePath = configurationDirectory;
            }

        }

        #endregion construction

    }
}
