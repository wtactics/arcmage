using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Matrix.Sdk.Api;
using Matrix.Sdk.Api.Events;
using Matrix.Sdk.Api.Requests.Session;
using Matrix.Sdk.Api.Responses.Events.Room;

namespace Arcmage.Matrix.MatchBot
{
    public class MatchBot
    {
        private MatrixAPI MatrixApi { get; }
     
      
        private Storage Storage { get; }

        private HttpClient HttpClient { get; }

        private MatchBotSettings Settings { get; }

        private bool RefreshLogin { get; set; }

        private CancellationTokenSource CancellationTokenSource { get; set; }


        public MatchBot(MatchBotSettings settings)
        {
            Settings = settings;
            HttpClient = new HttpClient();
            Storage = Storage.Load(settings.StorageFile);

            MatrixApi = new MatrixAPI(Settings.HomeServer);
            MatrixApi.ApplicationID = Settings.ApplicationID;
            MatrixApi.ApplicationName = Settings.ApplicationName;
            MatrixApi.DeviceID = Settings.DeviceID;
            MatrixApi.DeviceName = Settings.DeviceName;

            MatrixApi.Events.LoginEvent += LoginHandler;
            MatrixApi.Events.RoomJoinEvent += RoomJoinHandler;

            MatrixApi.ClientLogin(new MatrixLoginPassword(Settings.User, Settings.Password));

        }

        public async Task StartAsync()
        {
            MatrixApi.StartSync(TimeSpan.FromMilliseconds(250));
            RefreshLogin = true;
            CancellationTokenSource = new CancellationTokenSource();
            try
            {
                AutoRefreshLogin(CancellationTokenSource.Token);
            }
            catch (Exception e)
            {

            }
        }

        private async  void AutoRefreshLogin(CancellationToken cancellationToken)
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromDays(1), cancellationToken);
                MatrixApi.ClientLogin(new MatrixLoginPassword(Settings.User, Settings.Password));
            }
        }

        public async Task StopAsync()
        {
            CancellationTokenSource.Cancel();

            MatrixApi.StopSync();
        }



        private void RoomJoinHandler(object sender, Events.RoomJoinEventArgs e)
        {
            if (e.Room == Settings.RoomId)
            {
                var messageList = e.Event?.Timeline?.Events?.Where(x => x.Type == "m.room.message").ToList().Cast<Message>();
                if (messageList != null)
                {
                    foreach (var message in messageList)
                    {
                        // Check if the event is an unprocessed arcbot command
                        if (!ShouldProcess(message)) continue;
                        // Add the event to the processed list
                        AddMessageId(message.EventID);

                        try
                        {
                            ParseCommand(message.Sender, message.Content.Body);
                        }
                        catch (Exception exception)
                        {
                            // something went wrong
                        }
                    }
                }
            }
        }

        // Parse and execute command, available commands are: .help, .add, .remove, .list, .match, .play
        private void ParseCommand(string messageSender, string command)
        {
            command = command.Trim();

            if (command.StartsWith(".help"))
            {
                var helpList = new List<string>()
                {
                    "Hey, I'm arcbot, here to help you find arcmage players and set up arcmage games.",
                    " .help   =>  show this help",
                    " .add    =>  mark yourself as willing to play and add you to the match notification list",
                    " .remove =>  remove yourself from the match notification list",
                    " .list   =>  show all arcmage players on the notification list",
                    " .match  =>  let all arcmage players on the notification list know your up for a game",
                    " .play   =>  create and set up the arcmage game battlefield, the game link shared, and two players can join the game"
                };
                var plainHelp = string.Join('\n', helpList);
                var formattedHelp = string.Join("<br/>", helpList);
                MatrixApi.SendTextMessageToRoom(Settings.RoomId, plainHelp, formattedHelp);
            }

            if (".add".Equals(command))
            {
                if (!Storage.Players.Contains(messageSender))
                {
                    Storage.Players.Add(messageSender);
                    Storage.Save(Settings.StorageFile);
                    var plain = $"Added {messageSender} to the notification list, as you commanded.";
                    var formatted = $"Added {FormatPlayerHtml(messageSender)} to the notification list, as you commanded.";
                    MatrixApi.SendTextMessageToRoom(Settings.RoomId, plain, formatted );
                }
                else
                {
                    MatrixApi.SendTextMessageToRoom(Settings.RoomId, $"Your benevolence is already in the notification list, my liege.");
                }
              
            }
            if (".remove".Equals(command))
            {
                if (Storage.Players.Contains(messageSender))
                {
                    Storage.Players.Remove(messageSender);
                    Storage.Save(Settings.StorageFile);
                    var plain = $"Removed {messageSender} from the notification list, as you commanded.";
                    var formatted = $"Removed {FormatPlayerHtml(messageSender)} from the notification list, as you commanded.";
                    MatrixApi.SendTextMessageToRoom(Settings.RoomId, plain, formatted);
                }
                else
                {
                    MatrixApi.SendTextMessageToRoom(Settings.RoomId, $"According to my ledgers, your benevolence is not in the notification list, my liege.");
                }
            }
            if (".list".Equals(command))
            {
                var plainplayers = string.Join(",", Storage.Players.Select(FormatPlayerDisplayName));
                var plain = $"Here are your foes, my liege: {plainplayers}";

                var players = string.Join(",", Storage.Players.Select(FormatPlayerHtml));
                var formatted = $"Here are your foes, my liege: {players}";
                MatrixApi.SendTextMessageToRoom(Settings.RoomId, plain, formatted);
            }
            if (".match".Equals(command))
            {
                var plainplayers = string.Join(",", Storage.Players);
                var plain = $"Hey arc-mages, up for a game? {plainplayers}, anyone?";

                var players = string.Join(",", Storage.Players.Select(FormatPlayerHtml));
                var formatted = $"Hey arc-mages, up for a game? {players}, anyone?";

                MatrixApi.SendTextMessageToRoom(Settings.RoomId, plain, formatted);
            }

            if (".play".Equals(command))
            {
                var response = HttpClient.PostAsJsonAsync("https://aminduna.arcmage.org:9090/api/Games", new Game { name = $"{ FormatPlayerDisplayName(messageSender)}'s game"}).Result;
                var game = HttpContentExtensions.ReadAsAsync<Game>(response.Content).Result;

                var gameInvite = $"https://aminduna.arcmage.org/#/invite/{game.guid}?invitedBy=arcbot";
                MatrixApi.SendTextMessageToRoom(Settings.RoomId, gameInvite);

            }
        }

        private string FormatPlayerDisplayName(string player)
        {
            var parts = player.Split(":");
            var displayName = parts[0];
            displayName = displayName.TrimStart('@');
            return displayName;
        }

        private string FormatPlayerHtml(string player)
        {
            var parts = player.Split(":");
            var displayName = parts[0];
            displayName = displayName.TrimStart('@');
            return $"<a href=\"https://matrix.to/#/{player}\">{displayName}</a>";
        }

        private bool ShouldProcess(Message message)
        {
            if (message.Content.MessageType != "m.text") return false;
            var command = message.Content.Body;
            if (!command.StartsWith(".")) return false;
            if (Storage.ProcessedMessageIds.Contains(message.EventID)) return false;
            return true;
        }

        private void AddMessageId(string messageEventId)
        {
            Storage.ProcessedMessageIds.Add(messageEventId);
            Storage.ProcessedMessageIds = Storage.ProcessedMessageIds.TakeLast(10).ToList();
            Storage.Save(Settings.StorageFile);
        }

        private void LoginHandler(object sender, Events.LoginEventArgs e)
        {
            Task.Run(async () =>
            {
                await MatrixApi.JoinRoom(Settings.RoomId);
                // N.G. Remark: say hello after login. Or just be quiet.
                // await MatrixApi.SendTextMessageToRoom(RoomId, "Hello World, eh.. hello Arcmage players!");
            });
        }


    }
}
