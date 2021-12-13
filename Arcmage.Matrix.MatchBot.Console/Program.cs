using System;
using System.Threading.Tasks;

namespace Arcmage.Matrix.MatchBot.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // N.G. Remark: supply your own homeserver, bot credentials, and the matrix room id for the bot (not the alias, but the real room id)
            var settings = new MatchBotSettings()
            {
                HomeServer = "https://matrix.org",
                User = "",
                Password = "",
                RoomId = "",
                ApplicationID = "org.arcmage.arcbot",
                ApplicationName = "Arcmage MatchBot",
                DeviceID = "org.arcmage.aminduna",
                DeviceName = "Aminduna",
                StorageFile = "storage.json"
            };



            var matchBot = new MatchBot(settings);
            matchBot.StartAsync();

            System.Console.WriteLine("Arcmage match bot started, press any key to quit");
            System.Console.ReadKey();

            matchBot.StopAsync();

        }

    }
}
