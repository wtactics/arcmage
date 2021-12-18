using System;
using System.Collections.Generic;
using System.Globalization;
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

        private Dictionary<string, Command> Commands { get; set; }

        private const string CommandChar = "!";

        public MatchBot(MatchBotSettings settings)
        {
         
            Settings = settings;
            HttpClient = new HttpClient();
            Storage = Storage.Load(settings.StorageFile);
            SetupCommands();

            MatrixApi = new MatrixAPI(Settings.HomeServer);
            MatrixApi.ApplicationID = Settings.ApplicationID;
            MatrixApi.ApplicationName = Settings.ApplicationName;
            MatrixApi.DeviceID = Settings.DeviceID;
            MatrixApi.DeviceName = Settings.DeviceName;

            MatrixApi.Events.LoginEvent += LoginHandler;
            MatrixApi.Events.RoomJoinEvent += RoomMessageHandler;

            // Log in to matrix
            MatrixApi.ClientLogin(new MatrixLoginPassword(Settings.User, Settings.Password));

        }


        #region matrix life cylce

        // Start processing matrix room messages and auto refresh login
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

        // Auto Refresh login each day
        private async void AutoRefreshLogin(CancellationToken cancellationToken)
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromDays(1), cancellationToken);
                MatrixApi.ClientLogin(new MatrixLoginPassword(Settings.User, Settings.Password));
            }
        }

        // Once logged in to matrix, join the room (if not joined)
        private void LoginHandler(object sender, Events.LoginEventArgs e)
        {
            Task.Run(async () =>
            {
                await MatrixApi.JoinRoom(Settings.RoomId);
                // N.G. Remark: say hello after login. Or just be quiet.
                // await MatrixApi.SendTextMessageToRoom(RoomId, "Hello World, eh.. hello Arcmage players!");
            });
        }

        // Stop processing matrix room messages
        public async Task StopAsync()
        {
            CancellationTokenSource.Cancel();
            MatrixApi.StopSync();
        }

        // Processing room messages and execute the command if need be
        private void RoomMessageHandler(object sender, Events.RoomJoinEventArgs e)
        {
            // We're only parsing messages of the given room
            if (e.Room == Settings.RoomId)
            {
                // only room messages (no joins, or other kind of events)
                var messageList = e.Event?.Timeline?.Events?.Where(x => x.Type == "m.room.message").ToList().Cast<Message>();
                if (messageList != null)
                {
                    foreach (var message in messageList)
                    {
                        // Check if the event is an unprocessed arcbot command
                        if (!ShouldProcess(message)) continue;
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
            // not a command (should not happen as ShouldProcess checks it as well.)
            if (string.IsNullOrWhiteSpace(command) || !command.StartsWith(CommandChar)) return;

            var commandParts = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            // not a known command
            if (commandParts.Length < 1 || !Commands.ContainsKey(commandParts[0])) return;

            // command to execute
            var commandToExecute = Commands[commandParts[0]];

            // get arguments
            var arguments = command.Length == 1 ? new string[0] : commandParts[1..];

            // execute
            commandToExecute.Execute(messageSender, arguments);

        }

        // Check if we should process the message (we keep the last 10 processed message id's)
        private bool ShouldProcess(Message message)
        {
            // only text messages that start whit "." are commands
            if (message.Content.MessageType != "m.text") return false;
            var command = message.Content.Body;
            if (!command.StartsWith(CommandChar)) return false;
            // check if we haven't processed it already
            if (Storage.ProcessedMessageIds.Contains(message.EventID)) return false;

            // Add the event to the processed list
            Storage.ProcessedMessageIds.Add(message.EventID);
            Storage.ProcessedMessageIds = Storage.ProcessedMessageIds.TakeLast(10).ToList();
            Storage.Save(Settings.StorageFile);

            return true;
        }


        #endregion matrix life cylce

        #region command implementations

        private void SetupCommands()
        {
            var commandList = new List<Command>
            {
                new Command() { Name = $"{CommandChar}help", Execute = HelpCommand },
                new Command() { Name = $"{CommandChar}add", Execute = AddCommand },
                new Command() { Name = $"{CommandChar}remove", Execute = RemoveCommand },
                new Command() { Name = $"{CommandChar}list", Execute = ListCommand },
                new Command() { Name = $"{CommandChar}match", Execute = MatchCommand },
                new Command() { Name = $"{CommandChar}play", Execute = PlayCommand },
                new Command() { Name = $"{CommandChar}lic", Execute = LicenseCommand },
                new Command() { Name = $"{CommandChar}timezone", Execute = TimeZoneCommand },
            };

            Commands = commandList.ToDictionary(x => x.Name, x => x);
        }

        private void TimeZoneCommand(string sender, string[] arguments)
        {
            if (Storage.Players.ContainsKey(sender))
            {

                var player = Storage.Players[sender];

                // setting the time zone
                if (arguments.Length > 0)
                {
                    try
                    {
                        if (!(arguments[0].StartsWith("UTC+") || arguments[0].StartsWith("UTC-"))) throw new Exception("Not a valid UTC offset");
                        var negative = arguments[0][3] == '-';
                        var timezoneString = arguments[0].Substring(4);

                        
                        if (timezoneString.Length < 3)
                        {
                            timezoneString = timezoneString + ":00";
                        }

                        if (timezoneString.Length == 4)
                        {
                            timezoneString = "0" + timezoneString;
                        }


                        var utcOffSet = TimeSpan.ParseExact(timezoneString, "hh\\:mm", CultureInfo.InvariantCulture);
                        if (negative) utcOffSet = -utcOffSet;
                       
                        player.TimeZoneOffset = utcOffSet;
                        player.TimeZone = arguments[0];
                        Storage.Save(Settings.StorageFile);

                    }
                    catch (Exception e)
                    {
                        MatrixApi.SendTextMessageToRoom(Settings.RoomId, $"Could not parse the timezone. Example usage: {CommandChar}timezone UTC+03:00 or {CommandChar}timezone UTC-11:00");
                        return;
                    }
                }

                var plain = $"I'm in sync with your timezone ({ player.TimeZone}), my liege.";
                var formatted = $"I'm in sync with your timezone ({ player.TimeZone}), my liege.";
                MatrixApi.SendTextMessageToRoom(Settings.RoomId, plain, formatted);

            }
            else
            {
                MatrixApi.SendTextMessageToRoom(Settings.RoomId, $"According to my ledgers, your benevolence is not in the notification list, my liege.");
            }
        }

        private void LicenseCommand(string sender, string[] arguments)
        {
            var helpList = new List<string>()
            {
                "My avatar was drawn by Emilien Rotival under CC-BY-SA4.",
                "My source code is under GPLv3 and is available at https://github.com/wtactics/arcmage",
            };
            var plainHelp = string.Join('\n', helpList);
            var formattedHelp = string.Join("<br/>", helpList);
            MatrixApi.SendTextMessageToRoom(Settings.RoomId, plainHelp, formattedHelp);
        }

        private void PlayCommand(string sender, string[] arguments)
        {
            try
            {
                var gameName = $"{FormatPlayerDisplayName(sender)}'s game";

                var response = HttpClient.PostAsJsonAsync("https://aminduna.arcmage.org:9090/api/Games", new Game { name = gameName }).Result;
                var game = HttpContentExtensions.ReadAsAsync<Game>(response.Content).Result;

                var gameInvite = $"{gameName} invite link: https://aminduna.arcmage.org/#/invite/{game.guid}?invitedBy=arcbot";
                var chatLink = $"jit.si voice chat: https://meet.jit.si/arcmage_{game.guid}";
                var plain = gameInvite + "\n" + chatLink;

                var gameInviteFormatted = $"<a href=\"https://aminduna.arcmage.org/#/invite/{game.guid}?invitedBy=arcbot\">{gameName} invite link</a>";
                var chatLinkFormatted = $"<a href=\"https://meet.jit.si/arcmage_{game.guid}\">jit.si voice chat</a>";
                var formatted = gameInviteFormatted + "<br/>" + chatLinkFormatted;

                MatrixApi.SendTextMessageToRoom(Settings.RoomId, plain, formatted);
            }
            catch (Exception e)
            {
                // Couldn't create the game
            }
        }

        private void MatchCommand(string sender, string[] arguments)
        {
            if (Storage.Players.ContainsKey(sender))
            {

                var player = Storage.Players[sender];

                if (arguments.Length == 1)
                {

                    try
                    {
                        var matchTimeString = arguments[0];

                        if (matchTimeString.Length < 3)
                        {
                            matchTimeString = matchTimeString + ":00";
                        }

                        if (matchTimeString.Length == 4)
                        {
                            matchTimeString = "0" + matchTimeString;
                        }

                        var matchTime = TimeSpan.ParseExact(matchTimeString, "hh\\:mm", CultureInfo.InvariantCulture);
                        var utcMatchDate = new DateTimeOffset(DateTimeOffset.UtcNow.Date.Add(matchTime), player.TimeZoneOffset);


                        var plainplayers = string.Join(",", Storage.Players.Values.Select(x => $"{x.DisplayName} ({ utcMatchDate.ToOffset(x.TimeZoneOffset).ToString("HH:mm", CultureInfo.InvariantCulture) })"));
                        var plain = $"Hey arc-mages, up for a game at { utcMatchDate.ToUniversalTime().ToString("HH:mm", CultureInfo.InvariantCulture) } UTC? {plainplayers}, anyone?";

                        var players = string.Join(",", Storage.Players.Values.Select(x => $"{x.FormattedName} ({ utcMatchDate.ToOffset(x.TimeZoneOffset).ToString("HH:mm", CultureInfo.InvariantCulture) })"));
                        var formatted = $"Hey arc-mages, up for a game at { utcMatchDate.ToUniversalTime().ToString("HH:mm", CultureInfo.InvariantCulture) } UTC? {players}, anyone?";
                        MatrixApi.SendTextMessageToRoom(Settings.RoomId, plain, formatted);

                    }
                    catch (Exception e)
                    {
                        MatrixApi.SendTextMessageToRoom(Settings.RoomId, $"Could not parse the match time. Example usage: {CommandChar}match 20:00");
                        return;
                    }
                }
                else
                {
                    var plainplayers = string.Join(",", Storage.Players.Values.Select(x => x.DisplayName));
                    var plain = $"Hey arc-mages, up for a game? {plainplayers}, anyone?";

                    var players = string.Join(",", Storage.Players.Values.Select(x => x.FormattedName));
                    var formatted = $"Hey arc-mages, up for a game? {players}, anyone?";

                    MatrixApi.SendTextMessageToRoom(Settings.RoomId, plain, formatted);
                }

            }
            else
            {
                MatrixApi.SendTextMessageToRoom(Settings.RoomId, $"According to my ledgers, your benevolence is not in the notification list, my liege.");
            }

        }

        private void ListCommand(string sender, string[] arguments)
        {
            var plainplayers = string.Join(",", Storage.Players.Values.Select(x=>x.DisplayName));
            var plain = $"Here are your foes, my liege: {plainplayers}";

            var players = string.Join(",", Storage.Players.Values.Select(x=>x.FormattedName));
            var formatted = $"Here are your foes, my liege: {players}";
            MatrixApi.SendTextMessageToRoom(Settings.RoomId, plain, formatted);
        }

        private void RemoveCommand(string sender, string[] arguments)
        {
            if (Storage.Players.ContainsKey(sender))
            {
                var player = Storage.Players[sender];
                Storage.Players.Remove(sender);
                Storage.Save(Settings.StorageFile);
                
                var plain = $"Removed {player.DisplayName} from the notification list, as you commanded.";
                var formatted = $"Removed {player.FormattedName} from the notification list, as you commanded.";
                MatrixApi.SendTextMessageToRoom(Settings.RoomId, plain, formatted);
            }
            else
            {
                MatrixApi.SendTextMessageToRoom(Settings.RoomId, $"According to my ledgers, your benevolence is not in the notification list, my liege.");
            }
        }

        private void HelpCommand(string sender, string[] arguments)
        {
            var helpList = new List<string>()
            {
                "Hey, I'm arcbot, here to help you find arcmage players and set up arcmage games.",
                "",
                $" {CommandChar}help   =>  show this help",
                $" {CommandChar}add    =>  mark yourself as willing to play and add you to the match notification list",
                $" {CommandChar}remove =>  remove yourself from the match notification list",
                $" {CommandChar}list   =>  show all arcmage players on the notification list",
                $" {CommandChar}match [hh:mm] =>  let all arcmage players on the notification list know your up for a game [at your local time]. Example usage: {CommandChar}match 18:00",
                $" {CommandChar}play   =>  create and set up the arcmage game battlefield, the game link shared, and two players can join the game",
                $" {CommandChar}lic    =>  show the lic information",
                $" {CommandChar}timezone [offset] => shows or change your timezone setting. Example usage: {CommandChar}timezone UTC+03:00 or {CommandChar}timezone UTC-11:00 "
            };
            var plainHelp = string.Join('\n', helpList);
            var formattedHelp = string.Join("<br/>", helpList);
            MatrixApi.SendTextMessageToRoom(Settings.RoomId, plainHelp, formattedHelp);
        }

        private void AddCommand(string sender, string[] arguments)
        {
            if (!Storage.Players.ContainsKey(sender))
            {
                var player = CreatePlayer(sender);
                Storage.Players.Add(player.Id, player);
                Storage.Save(Settings.StorageFile);
                var plain = $"Added {player.DisplayName} to the notification list, as you commanded.";
                var formatted = $"Added {player.FormattedName} to the notification list, as you commanded.";
                MatrixApi.SendTextMessageToRoom(Settings.RoomId, plain, formatted);
            }
            else
            {
                MatrixApi.SendTextMessageToRoom(Settings.RoomId, $"Your benevolence is already in the notification list, my liege.");
            }
        }

       
        private Player CreatePlayer(string playerId)
        {
            var displayName = FormatPlayerDisplayName(playerId);
            return new Player()
            {
                Id = playerId,
                DisplayName = displayName,
                FormattedName = $"<a href=\"https://matrix.to/#/{playerId}\">{displayName}</a>",
                TimeZoneOffset = TimeSpan.Zero,
                TimeZone = "UTC+00:00"
            };
        }

        private string FormatPlayerDisplayName(string player)
        {
            var parts = player.Split(":");
            var displayName = parts[0];
            displayName = displayName.TrimStart('@').Split("#")[0];
            return displayName;
        }

        #endregion command implementations







    }
}
