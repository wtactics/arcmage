using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arcmage.Configuration;
using Arcmage.Matrix.MatchBot;
using Microsoft.Extensions.Hosting;

namespace Arcmage.Server.Api.MatchBotService
{
    public class MatchBotService: IHostedService
    {
        private MatchBot MatchBot { get; set; }

         public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (Settings.Current.HostMatchBot)
            {
                var matchBotSettings = new MatchBotSettings()
                {
                    HomeServer = Settings.Current.MatrixHomeServer,
                    User = Settings.Current.MatrixUser,
                    Password = Settings.Current.MatrixPassword,
                    RoomIds = Settings.Current.MatrixRoomIds,
                    AmindunaApi = Settings.Current.ApiUrl,
                    ApplicationID = "org.arcmage.arcbot",
                    ApplicationName = "Arcmage MatchBot",
                    DeviceID = "org.arcmage.aminduna",
                    DeviceName = "Aminduna",
                    StorageFile = "storage.json"
                };
                MatchBot = new MatchBot(matchBotSettings);
                MatchBot.StartAsync();
            }
          
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (MatchBot != null)
            {
                MatchBot.StopAsync();
            }
        }
    }
}
