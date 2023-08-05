using System;
using System.Collections.Generic;
using System.Globalization;

using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Arcmage.Client;
using Arcmage.Model;
using Matrix.Sdk.Api;
using Matrix.Sdk.Api.Events;
using Matrix.Sdk.Api.Requests.Session;
using Matrix.Sdk.Api.Responses.Events.Room;

namespace Arcmage.Matrix.MatchBot
{
    public class MatchBot
    {
        private MatrixAPI MatrixApi { get; }


        private ApiClient ApiClient { get; }



        private Storage Storage { get; }

        private HttpClient HttpClient { get; }

        private MatchBotSettings Settings { get; }

        private bool RefreshLogin { get; set; }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        private Dictionary<string, Command> Commands { get; set; }

        private const string CommandChar = "!";

        public MatchBot(MatchBotSettings settings)
        {

            ApiClient = new ApiClient(settings.AmindunaApi);
         
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
                // join each room
                foreach (var roomId in Settings.RoomIds)
                {
                    await MatrixApi.JoinRoom(roomId);
                    // N.G. Remark: say hello after login. Or just be quiet.
                    // await MatrixApi.SendTextMessageToRoom(roomId, "Hello World, eh.. hello Arcmage players!");
                }

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
            if (Settings.RoomIds.Contains(e.Room))
            {
                // only room messages (no joins, or other kind of events)
                var messageList = e.Event?.Timeline?.Events?.Where(x => x.Type == "m.room.message").ToList().Cast<Message>();
                if (messageList != null)
                {
                    foreach (var message in messageList)
                    {
                        // Check if the event is an unprocessed arcbot command
                        if (!ShouldProcess(e.Room, message)) continue;
                        try
                        {
                            if (IsInlineCardSearch(message.Content.Body))
                            {
                                ParseInlineCommand(e.Room, message.Sender, message.Content.Body);
                            }
                            else
                            {
                                ParseCommand(e.Room, message.Sender, message.Content.Body);
                            }
                            
                        }
                        catch (Exception exception)
                        {
                            // something went wrong
                        }
                    }
                }
            }
        }

        private void ParseInlineCommand(string roomId, string messageSender, string contentBody)
        {
            if (string.IsNullOrWhiteSpace(contentBody)) return;
            var matches = Regex.Matches(contentBody, @"\[\[(.*?)\]\]");
            foreach (Match match in matches)
            {
                var command = match.Value?.Trim('[', ']').Trim();
                if (string.IsNullOrEmpty(command)) continue;
                var arguments = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (arguments.Length > 0)
                {
                    CardCommand(roomId, messageSender, arguments);
                }

            }
        }

        // Parse and execute command, available commands are: .help, .add, .remove, .list, .match, .play
        private void ParseCommand(string roomId, string messageSender, string command)
        {
            if (string.IsNullOrWhiteSpace(roomId) || !Settings.RoomIds.Contains(roomId)) return;

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
            commandToExecute.Execute(roomId, messageSender, arguments);

        }

        // Check if we should process the message (we keep the last 10 processed message id's)
        private bool ShouldProcess(string roomId, Message message)
        {
            // only text messages that start whit "." are commands
            if (message?.Content?.MessageType != "m.text") return false;
            var command = message?.Content?.Body;
            if (!IsInlineCardSearch(command) && !command.StartsWith(CommandChar)) return false;
            // check if we haven't processed it already
            if (Storage.ProcessedMessageIds.Contains(message.EventID)) return false;

            // Add the event to the processed list
            Storage.ProcessedMessageIds.Add(message.EventID);
            Storage.ProcessedMessageIds = Storage.ProcessedMessageIds.TakeLast(10).ToList();
            Storage.Save(Settings.StorageFile);

            return true;
        }

        private bool IsInlineCardSearch(string contentBody)
        {
            if (string.IsNullOrWhiteSpace(contentBody)) return false;
            var matches = Regex.Matches(contentBody, @"\[\[(.*?)\]\]");
            foreach (Match match in matches)
            {
                var command = match.Value?.Trim('[',']').Trim();
                if (string.IsNullOrEmpty(command)) continue;
                var arguments = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (arguments.Length > 0)
                {
                    return true;
                }

            }
            return false;
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
                new Command() { Name = $"{CommandChar}card", Execute = CardCommand },
                new Command() { Name = $"{CommandChar}c", Execute = CardCommand },
                new Command() { Name = $"{CommandChar}full", Execute = FullCardCommand },
                new Command() { Name = $"{CommandChar}f", Execute = FullCardCommand },
                new Command() { Name = $"{CommandChar}text", Execute = TextCardCommand },
                new Command() { Name = $"{CommandChar}t", Execute = TextCardCommand },
                new Command() { Name = $"{CommandChar}cardlink", Execute = CardLinkCommand },
                new Command() { Name = $"{CommandChar}cl", Execute = CardLinkCommand },
                new Command() { Name = $"{CommandChar}deck", Execute = DeckCommand },
                new Command() { Name = $"{CommandChar}d", Execute = DeckCommand },
                new Command() { Name = $"{CommandChar}textdeck", Execute = TextDeckCommand },
                new Command() { Name = $"{CommandChar}td", Execute = TextDeckCommand },
                new Command() { Name = $"{CommandChar}decklink", Execute = DeckLinkCommand },
                new Command() { Name = $"{CommandChar}dl", Execute = DeckLinkCommand },
            };

            Commands = commandList.ToDictionary(x => x.Name, x => x);

            
        }

        private async void TextDeckCommand(string roomId, string sender, string[] arguments)
        {
            try
            {
                if (arguments.Length > 0)
                {
                    string search;
                    string languageCode = null;

                    if (arguments.Length > 1 && arguments[0].Length == 2)
                    {
                        languageCode = arguments[0];

                        search = string.Join(" ", arguments.Skip(1).ToList());
                    }

                    else
                    {
                        search = string.Join(" ", arguments.ToList());
                    }

                    if (search.Length < 3)
                    {
                        await MatrixApi.SendTextMessageToRoom(roomId, $"Provide at least three characters of the deck's name.");
                    }
                    else
                    {
                        // search for a deck (without deckcards)
                        var deckSearchOptions = new DeckSearchOptions
                        {
                            PageNumber = 1,
                            PageSize = 10,
                            Language = languageCode != null ? new Language() { LanguageCode = languageCode } : null,
                            Search = search,
                        };
                        var result = await ApiClient.Search<Deck, DeckSearchOptions>(deckSearchOptions);
                        var deck = result.Items?.FirstOrDefault();
                        
                        if (deck != null)
                        {
                            // get deck detailed information including deckcards
                            deck = await ApiClient.GetByGuid<Deck>(deck.Guid);
                            var plainDeckInfo = PlainDeckInfo(deck);
                            await MatrixApi.SendTextMessageToRoom(roomId, plainDeckInfo);
                        }
                        else
                        {
                            await MatrixApi.SendTextMessageToRoom(roomId, $"Unfortunately, I couldn't find the deck you were looking for.");
                        }

                    }

                }
                else
                {
                    await MatrixApi.SendTextMessageToRoom(roomId, $"Tell me what you are looking for and I'll fetch it at once!");
                }
            }
            catch
            {
                await MatrixApi.SendTextMessageToRoom(roomId, $"Something went wrong.");
            }

        }


        private async void DeckCommand(string roomId, string sender, string[] arguments)
        {
            try
            {
                if (arguments.Length > 0)
                {
                    string search;
                    string languageCode = null;

                    if (arguments.Length > 1 && arguments[0].Length == 2)
                    {
                        languageCode = arguments[0];

                        search = string.Join(" ", arguments.Skip(1).ToList());
                    }

                    else
                    {
                        search = string.Join(" ", arguments.ToList());
                    }

                    if (search.Length < 3)
                    {
                        await MatrixApi.SendTextMessageToRoom(roomId, $"Provide at least three characters of the deck's name.");
                    }
                    else
                    {
                        // search for a deck (without deckcards)
                        var deckSearchOptions = new DeckSearchOptions
                        {
                            PageNumber = 1,
                            PageSize = 10,
                            Language = languageCode != null ? new Language() { LanguageCode = languageCode } : null,
                            Search = search,
                        };
                        var result = await ApiClient.Search<Deck, DeckSearchOptions>(deckSearchOptions);
                        var deck = result.Items?.FirstOrDefault();

                        if (deck != null)
                        {
                            // get deck detailed information including deckcards
                            deck = await ApiClient.GetByGuid<Deck>(deck.Guid);
                            var plainDeckInfo = PlainDeckInfo(deck);
                            var formattedDeckInfo = FormatDeckInfo(deck);
                            await MatrixApi.SendTextMessageToRoom(roomId, plainDeckInfo, formattedDeckInfo);
                        }
                        else
                        {
                            await MatrixApi.SendTextMessageToRoom(roomId, $"Unfortunately, I couldn't find the deck you were looking for.");
                        }

                    }

                }
                else
                {
                    await MatrixApi.SendTextMessageToRoom(roomId, $"Tell me what you are looking for and I'll fetch it at once!");
                }
            }
            catch
            {
                await MatrixApi.SendTextMessageToRoom(roomId, $"Something went wrong.");
            }

        }

        private async void DeckLinkCommand(string roomId, string sender, string[] arguments)
        {
            try
            {
                if (arguments.Length > 0)
                {
                    string search;
                    string languageCode = null;

                    if (arguments.Length > 1 && arguments[0].Length == 2)
                    {
                        languageCode = arguments[0];

                        search = string.Join(" ", arguments.Skip(1).ToList());
                    }

                    else
                    {
                        search = string.Join(" ", arguments.ToList());
                    }

                    if (search.Length < 3)
                    {
                        await MatrixApi.SendTextMessageToRoom(roomId, $"Provide at least three characters of the deck's name.");
                    }
                    else
                    {
                        // search for a deck (without deckcards)
                        var deckSearchOptions = new DeckSearchOptions
                        {
                            PageNumber = 1,
                            PageSize = 10,
                            Language = languageCode != null ? new Language() { LanguageCode = languageCode } : null,
                            Search = search,
                        };
                        var result = await ApiClient.Search<Deck, DeckSearchOptions>(deckSearchOptions);
                        var deck = result.Items?.FirstOrDefault();

                        if (deck != null)
                        {
                            var plainDeckInfo = PlainDeckLinkInfo(deck);
                            var formattedDeckInfo = FormatDeckLinkInfo(deck);
                            await MatrixApi.SendTextMessageToRoom(roomId, plainDeckInfo, formattedDeckInfo);
                        }
                        else
                        {
                            await MatrixApi.SendTextMessageToRoom(roomId, $"Unfortunately, I couldn't find the deck you were looking for.");
                        }

                    }

                }
                else
                {
                    await MatrixApi.SendTextMessageToRoom(roomId, $"Tell me what you are looking for and I'll fetch it at once!");
                }
            }
            catch
            {
                await MatrixApi.SendTextMessageToRoom(roomId, $"Something went wrong.");
            }

        }

        private static string Aminduna = "https://aminduna.arcmage.org/#";

        private static string GetDeckUrl(Deck deck)
        {
            return $"{Aminduna}/decks/{deck.Guid.ToString()}";
        }

        private static string GetCardUrl(Card card)
        {
            return $"{Aminduna}/cards/{card.Guid.ToString()}";
        }

        private string FormatDeckInfo(Deck deck)
        {
            var deckinfo = $"<p><b>{deck.Name}</b></p>";

            var cityGuid = Guid.Parse("c38a9cda-e669-54e2-9c91-db0a4a763460");

            var cities = deck.DeckCards.Where(x => x.Card.Type.Guid == cityGuid).ToList();
            var cards = deck.DeckCards.Where(x => x.Card.Type.Guid != cityGuid).ToList();
            var tokens = cards.Where(x => x.Card?.SubType != null && x.Card.SubType.ToLowerInvariant().Contains("token")).ToList();
            var tokenCardGuids = tokens.Select(x => x.Card.Guid).ToList();
            cards = cards.Where(x => !tokenCardGuids.Contains(x.Card.Guid)).ToList();

            var cardsInfo = string.Join("<br/>", cards.Select(x => $"{x.Quantity} x {x.Card.Name}"));
            var citiesInfo = string.Join("<br/>", cities.Select(x => $"{x.Quantity} x {x.Card.Name}"));
            var tokensInfo = string.Join("<br/>", tokens.Select(x => $"{x.Quantity} x {x.Card.Name}"));

            cardsInfo = $"<p><em>Cards</em></p><p>{cardsInfo}</p>";
            citiesInfo = $"<p><em>Cities</em></p><p>{citiesInfo}</p>";
            tokensInfo = $"<p><em>Tokens</em></p><p>{tokensInfo}</p>";

            if (cards.Count == 0) { cardsInfo = ""; }
            if (cities.Count == 0) { citiesInfo = ""; }
            if (tokens.Count == 0) { tokensInfo = ""; }

            var info = $"<p></p><p><em><a href=\"{GetDeckUrl(deck)}\">more...</a></em></p>";

            var html = $"<blockquote>{deckinfo}{cardsInfo}{citiesInfo}{tokensInfo}{info}</blockquote>";

            return html;
        }

        private string PlainDeckInfo(Deck deck)
        {
            var deckInfo = new List<string>();
            deckInfo.Add($"{deck.Name}");
            deckInfo.Add("");
            foreach (var deckCard in deck.DeckCards)
            {
                deckInfo.Add($"{deckCard.Quantity} x {deckCard.Card.Name}");
            }
            return string.Join("\n", deckInfo);
        }

        private async void TextCardCommand(string roomId, string sender, string[] arguments)
        {
            try
            {
                if (arguments.Length > 0)
                {
                    string search;
                    string languageCode = null;

                    if (arguments.Length > 1 && arguments[0].Length == 2)
                    {
                        languageCode = arguments[0];

                        search = string.Join(" ", arguments.Skip(1).ToList());
                    }

                    else
                    {
                        search = string.Join(" ", arguments.ToList());
                    }

                    if (search.Length < 3)
                    {
                        await MatrixApi.SendTextMessageToRoom(roomId, $"Provide at least three characters of the card's name.");
                    }
                    else
                    {
                        // Search of the card has already been translated into the given language
                        var cardSearchOptions = new CardSearchOptions
                        {
                            PageNumber = 1,
                            PageSize = 10,
                            Language = languageCode != null ? new Language() { LanguageCode = languageCode } : null,
                            Search = search,
                            SearchNameOnly = true
                        };
                        var result = await ApiClient.Search<Card, CardSearchOptions>(cardSearchOptions);
                        var card = result.Items?.FirstOrDefault();

                        if (card != null)
                        {
                            var data = await ApiClient.GetContentBytes(card.Webp);
                            if (data != null)
                            {
                               
                                var plainCardInfo = PlainCardTextInfo(card);
                                var formattedCardInfo = FormatCardTextInfo(card);
                                await MatrixApi.SendTextMessageToRoom(roomId, plainCardInfo, formattedCardInfo);

                            }
                            else
                            {
                                await MatrixApi.SendTextMessageToRoom(roomId, $"Unfortunately, I couldn't find the card's image.");
                            }
                        }
                        else
                        {
                            await MatrixApi.SendTextMessageToRoom(roomId, $"Unfortunately, I couldn't find the card you were looking for.");
                        }

                    }

                }
                else
                {
                    await MatrixApi.SendTextMessageToRoom(roomId, $"Tell me what you are looking for and I'll fetch it at once!");
                }
            }
            catch
            {
                await MatrixApi.SendTextMessageToRoom(roomId, $"Something went wrong.");
            }

        }

        private async void CardLinkCommand(string roomId, string sender, string[] arguments)
        {
            try
            {
                if (arguments.Length > 0)
                {
                    string search;
                    string languageCode = null;

                    if (arguments.Length > 1 && arguments[0].Length == 2)
                    {
                        languageCode = arguments[0];

                        search = string.Join(" ", arguments.Skip(1).ToList());
                    }

                    else
                    {
                        search = string.Join(" ", arguments.ToList());
                    }

                    if (search.Length < 3)
                    {
                        await MatrixApi.SendTextMessageToRoom(roomId, $"Provide at least three characters of the card's name.");
                    }
                    else
                    {
                        // Search of the card has already been translated into the given language
                        var cardSearchOptions = new CardSearchOptions
                        {
                            PageNumber = 1,
                            PageSize = 10,
                            Language = languageCode != null ? new Language() { LanguageCode = languageCode } : null,
                            Search = search,
                            SearchNameOnly = true
                        };
                        var result = await ApiClient.Search<Card, CardSearchOptions>(cardSearchOptions);
                        var card = result.Items?.FirstOrDefault();

                        if (card != null)
                        {
                            var plainCardInfo = PlainCardLinkInfo(card);
                            var formattedCardInfo = FormatCardLinkInfo(card);
                            await MatrixApi.SendTextMessageToRoom(roomId, plainCardInfo, formattedCardInfo);
                        }
                        else
                        {
                            await MatrixApi.SendTextMessageToRoom(roomId, $"Unfortunately, I couldn't find the card you were looking for.");
                        }

                    }

                }
                else
                {
                    await MatrixApi.SendTextMessageToRoom(roomId, $"Tell me what you are looking for and I'll fetch it at once!");
                }
            }
            catch
            {
                await MatrixApi.SendTextMessageToRoom(roomId, $"Something went wrong.");
            }

        }




        private async void FullCardCommand(string roomId, string sender, string[] arguments)
        {
            try
            {
                if (arguments.Length > 0)
                {
                    string search;
                    string languageCode = null;

                    if (arguments.Length > 1 && arguments[0].Length == 2)
                    {
                        languageCode = arguments[0];

                        search = string.Join(" ", arguments.Skip(1).ToList());
                    }

                    else
                    {
                        search = string.Join(" ", arguments.ToList());
                    }

                    if (search.Length < 3)
                    {
                        await MatrixApi.SendTextMessageToRoom(roomId, $"Provide at least three characters of the card's name.");
                    }
                    else
                    {
                        // Search of the card has already been translated into the given language
                        var cardSearchOptions = new CardSearchOptions
                        {
                            PageNumber = 1,
                            PageSize = 10,
                            Language = languageCode != null ? new Language() { LanguageCode = languageCode } : null,
                            Search = search,
                            SearchNameOnly = true
                        };
                        var result = await ApiClient.Search<Card, CardSearchOptions>(cardSearchOptions);
                        var card = result.Items?.FirstOrDefault();

                        if (card != null)
                        {
                            var data = await ApiClient.GetContentBytes(card.Webp);
                            if (data != null)
                            {
                                var contentUri = await MatrixApi.MediaUpload("image/webp", data);
                                await MatrixApi.SendImageToRoom(roomId, contentUri, card.Name, "image/webp", 308, 437, data.Length);

                                var plainCardInfo = PlainCardInfo(card);
                                var formattedCardInfo = FormatCardTextInfo(card);

                                await MatrixApi.SendTextMessageToRoom(roomId,plainCardInfo, formattedCardInfo);

                            }
                            else
                            {
                                await MatrixApi.SendTextMessageToRoom(roomId, $"Unfortunately, I couldn't find the card's image.");
                            }
                        }
                        else
                        {
                            await MatrixApi.SendTextMessageToRoom(roomId, $"Unfortunately, I couldn't find the card you were looking for.");
                        }

                    }

                }
                else
                {
                    await MatrixApi.SendTextMessageToRoom(roomId, $"Tell me what you are looking for and I'll fetch it at once!");
                }
            }
            catch
            {
                await MatrixApi.SendTextMessageToRoom(roomId, $"Something went wrong.");
            }

        }


        private string PlainCardInfo(Card card)
        {
            var subtype = card.SubType;
            // Use the creature guid to identity the type
            if (card.Type.Guid == Guid.Parse("bc752e51-034a-56fa-a9b6-a4cb86049ec3"))
            {
                subtype = $"Creature - {subtype} - {card.Attack}/{card.Defense}";
            }
            var info = new List<string>()
            {
                $"Name: {card.Name}",
                $"Type: {subtype}",
                $"Rule: {card.RuleText}"
            };
            if (!string.IsNullOrWhiteSpace(card.FlavorText))
            {
                info.Add($"Flavour: {card.FlavorText}");
            }
            return string.Join("\n", info) + "\n";
        }

        private string PlainCardLinkInfo(Card card)
        {
            var info = new List<string>()
            {
                $"{card.Name}",
                $"{GetCardUrl(card)}"
            };
            return string.Join("\n", info) + "\n";
        }


        private string FormatCardLinkInfo(Card card)
        {
            var cardinfo = $"<p><b>{card.Name}<b></p>";
            cardinfo += $"<p></p><p><a href=\"{GetCardUrl(card)}\"><em>more...</em></a></p>";
            var html = $"<blockquote>{cardinfo}</blockquote>";
            return html;
        }

        private string PlainDeckLinkInfo(Deck deck)
        {
            var info = new List<string>()
            {
                $"{deck.Name}",
                $"{GetDeckUrl(deck)}"
            };
            return string.Join("\n", info) + "\n";
        }

        private string FormatDeckLinkInfo(Deck deck)
        {
            var cardinfo = $"<p><b>{deck.Name}<b></p>";
            cardinfo += $"<p></p><p><a href=\"{GetDeckUrl(deck)}\"><em>more...</em></a></p>";
            var html = $"<blockquote>{cardinfo}</blockquote>";
            return html;
        }

        private string FormatCardTextInfo(Card card)
        {
            var subtype = card.SubType;
            // Use the creature guid to identity the type
            if (card.Type.Guid == Guid.Parse("bc752e51-034a-56fa-a9b6-a4cb86049ec3"))
            {
                subtype = $"Creature - {subtype} - {card.Attack}/{card.Defense}";
            }

          

            var cost = "";
            if (!string.IsNullOrWhiteSpace(card.Cost))
            {
                cost = $" - {card.Cost}";
            }

            for (int l = 0; l < card.Loyalty; l++)
            {
                cost += "L";
            }

            // Use the city guid to identity the type
            if (card.Type.Guid == Guid.Parse("c38a9cda-e669-54e2-9c91-db0a4a763460"))
            {
                subtype = $"{subtype} - {card.Defense}";
                cost = "";
            }


            var cardinfo = $"<p><b>{card.Name} - {card.Faction.Name}{cost}" +
                           $"<br/>{subtype}</b></p>" +
                           $"<p>{card.RuleText}</p>";
            if (!string.IsNullOrWhiteSpace(card.FlavorText))
            {
                var flavorText = card.FlavorText.Trim();
                cardinfo += $"<p><em>{flavorText}</em></p>";
            }

            cardinfo += $"<p><a href=\"{GetCardUrl(card)}\"><em>more...</em></a></p>";

            var html = $"<blockquote>{cardinfo}</blockquote>";

            return html;
        }

        private string PlainCardTextInfo(Card card)
        {
            var subtype = card.SubType;
            // Use the creature guid to identity the type
            if (card.Type.Guid == Guid.Parse("bc752e51-034a-56fa-a9b6-a4cb86049ec3"))
            {
                subtype = $"Creature - {subtype} - {card.Attack}/{card.Defense}";
            }

            var cost = "";
            if (!string.IsNullOrWhiteSpace(card.Cost))
            {
                cost = $" - {card.Cost}";
            }

            var cardinfo = new List<string>()
            {
                $"{card.Name}{cost} - {subtype}",
                $"{card.RuleText}"
            };

            if (!string.IsNullOrWhiteSpace(card.FlavorText))
            {
                cardinfo.Add($"{card.FlavorText}");
            }

            var html = string.Join("\n", cardinfo);

            return html;
        }

        private async void CardCommand(string roomId, string sender, string[] arguments)
        {
            try
            {
                if (arguments.Length > 0)
                {
                    string search;
                    string languageCode = null;

                    if (arguments.Length > 1 && arguments[0].Length == 2)
                    {
                        languageCode = arguments[0];

                        search = string.Join(" ", arguments.Skip(1).ToList());
                    }

                    else
                    {
                        search = string.Join(" ", arguments.ToList());
                    }

                    if (search.Length < 3)
                    {
                        await MatrixApi.SendTextMessageToRoom(roomId, $"Provide at least three characters of the card's name.");
                    }
                    else
                    {
                        // Search of the card has already been translated into the given language
                        var cardSearchOptions = new CardSearchOptions
                        {
                            PageNumber = 1,
                            PageSize = 10,
                            Language = languageCode != null ? new Language() { LanguageCode = languageCode } : null,
                            Search = search,
                            SearchNameOnly = true
                        };
                        var result = await ApiClient.Search<Card, CardSearchOptions>(cardSearchOptions);
                        var card = result.Items?.FirstOrDefault();

                        if (card != null)
                        {
                            var data = await ApiClient.GetContentBytes(card.Webp);
                            if (data != null)
                            {
                                var contentUri = await MatrixApi.MediaUpload("image/webp", data);
                                await MatrixApi.SendImageToRoom(roomId, contentUri, card.Name, "image/webp", 308, 437, data.Length);

                            }
                            else
                            {
                                await MatrixApi.SendTextMessageToRoom(roomId, $"Unfortunately, I couldn't find the card's image.");
                            }
                        }
                        else
                        {
                            await MatrixApi.SendTextMessageToRoom(roomId, $"Unfortunately, I couldn't find the card you were looking for.");
                        }

                    }

                }
                else
                {
                    await MatrixApi.SendTextMessageToRoom(roomId, $"Tell me what you are looking for and I'll fetch it at once!");
                }
            }
            catch
            {
                await MatrixApi.SendTextMessageToRoom(roomId, $"Something went wrong.");
            }
          
        }

        private async void TimeZoneCommand(string roomId, string sender, string[] arguments)
        {
            if (Storage.Players.ContainsKey(sender))
            {

                var player = Storage.Players[sender];

                // setting the time zone
                if (arguments.Length > 0)
                {
                    try
                    {
                        if (!(arguments[0].ToUpper().StartsWith("UTC+") || arguments[0].ToUpper().StartsWith("UTC-"))) throw new Exception("Not a valid UTC offset");
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
                        await MatrixApi.SendTextMessageToRoom(roomId, $"Could not parse the timezone. Example usage: {CommandChar}timezone UTC+03:00 or {CommandChar}timezone UTC-11:00");
                        return;
                    }
                }

                var plain = $"I'm in sync with your timezone ({ player.TimeZone}), my liege.";
                var formatted = $"I'm in sync with your timezone ({ player.TimeZone}), my liege.";
                await MatrixApi.SendTextMessageToRoom(roomId, plain, formatted);

            }
            else
            {
                await MatrixApi.SendTextMessageToRoom(roomId, $"According to my ledgers, your benevolence is not in the notification list, my liege.");
            }
        }

        private async void LicenseCommand(string roomId, string sender, string[] arguments)
        {
            var helpList = new List<string>()
            {
                "My avatar was drawn by Emilien Rotival under CC-BY-SA4.",
                "My source code is under GPLv3 and is available at https://github.com/wtactics/arcmage",
            };
            var plainHelp = string.Join('\n', helpList);
            var formattedHelp = string.Join("<br/>", helpList);
            await MatrixApi.SendTextMessageToRoom(roomId, plainHelp, formattedHelp);
        }

        private async void PlayCommand(string roomId, string sender, string[] arguments)
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

                await MatrixApi.SendTextMessageToRoom(roomId, plain, formatted);
            }
            catch (Exception e)
            {
                // Couldn't create the game
            }
        }

        private async void MatchCommand(string roomId, string sender, string[] arguments)
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
                        await MatrixApi.SendTextMessageToRoom(roomId, plain, formatted);

                    }
                    catch (Exception e)
                    {
                        await MatrixApi.SendTextMessageToRoom(roomId, $"Could not parse the match time. Example usage: {CommandChar}match 20:00");
                        return;
                    }
                }
                else
                {
                    var plainplayers = string.Join(",", Storage.Players.Values.Select(x => x.DisplayName));
                    var plain = $"Hey arc-mages, up for a game? {plainplayers}, anyone?";

                    var players = string.Join(",", Storage.Players.Values.Select(x => x.FormattedName));
                    var formatted = $"Hey arc-mages, up for a game? {players}, anyone?";

                    await MatrixApi.SendTextMessageToRoom(roomId, plain, formatted);
                }

            }
            else
            {
                await MatrixApi.SendTextMessageToRoom(roomId, $"According to my ledgers, your benevolence is not in the notification list, my liege.");
            }

        }

        private async void ListCommand(string roomId, string sender, string[] arguments)
        {
            var plainplayers = string.Join(",", Storage.Players.Values.Select(x=>x.DisplayName));
            var plain = $"Here are your foes, my liege: {plainplayers}";

            var players = string.Join(",", Storage.Players.Values.Select(x=>x.FormattedName));
            var formatted = $"Here are your foes, my liege: {players}";
            await MatrixApi.SendTextMessageToRoom(roomId, plain, formatted);
        }

        private async void RemoveCommand(string roomId, string sender, string[] arguments)
        {
            if (Storage.Players.ContainsKey(sender))
            {
                var player = Storage.Players[sender];
                Storage.Players.Remove(sender);
                Storage.Save(Settings.StorageFile);
                
                var plain = $"Removed {player.DisplayName} from the notification list, as you commanded.";
                var formatted = $"Removed {player.FormattedName} from the notification list, as you commanded.";
                await MatrixApi.SendTextMessageToRoom(roomId, plain, formatted);
            }
            else
            {
                await MatrixApi.SendTextMessageToRoom(roomId, $"According to my ledgers, your benevolence is not in the notification list, my liege.");
            }
        }

        private async void HelpCommand(string roomId, string sender, string[] arguments)
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
                $" {CommandChar}timezone [offset] => shows or change your timezone setting. Example usage: {CommandChar}timezone UTC+03:00 or {CommandChar}timezone UTC-11:00 ",
                $" {CommandChar}card [isoLanguageCode] [card name search] => Displays the first card matching the fussy name search. " +
                                                                             $"Use the two digit iso language code to filter on language.",
                $" {CommandChar}c => Shorthand for {CommandChar}card.",
                $" {CommandChar}full [isoLanguageCode] [card name search] => Displays the full information of first card matching the fussy name search. " +
                                                                             $"Use the two digit iso language code to filter on language.",
                $" {CommandChar}f => Shorthand for {CommandChar}full.",
                $" {CommandChar}text [isoLanguageCode] [card name search] => Displays basic information of first card matching the fussy name search. " +
                                                                             $"Use the two digit iso language code to filter on language.",
                $" {CommandChar}t => Shorthand for {CommandChar}text.",
                $" {CommandChar}cardlink [isoLanguageCode] [card name search] => Displays the aminduna link to the first card matching the fussy name search. " +
                                                                             $"Use the two digit iso language code to filter on language.",
                $" {CommandChar}cl => Shorthand for {CommandChar}cardlink.",
                $" {CommandChar}deck [isoLanguageCode] [deck name search] => Displays the card list of the first deck matching the fussy name search. " +
                                                                             $"Use the two digit iso language code to filter on language.",
                $" {CommandChar}d => Shorthand for {CommandChar}deck.",
                $" {CommandChar}textdeck [isoLanguageCode] [deck name search] => Displays the card list of the first deck matching the fussy name search in plain text. " +
                                                                                 $"Use the two digit iso language code to filter on language.",
                $" {CommandChar}td => Shorthand for {CommandChar}textdeck.",
                $" {CommandChar}decllink [isoLanguageCode] [card deck search] => Displays the aminduna link to the first deck matching the fussy name search. " +
                                                                                 $"Use the two digit iso language code to filter on language.",
                $" {CommandChar}dl => Shorthand for {CommandChar}decklink.",

            };
            var plainHelp = string.Join('\n', helpList);
            var formattedHelp = string.Join("<br/>", helpList);
            await MatrixApi.SendTextMessageToRoom(roomId, plainHelp, formattedHelp);
        }

        private async void AddCommand(string roomId, string sender, string[] arguments)
        {
            if (!Storage.Players.ContainsKey(sender))
            {
                var player = CreatePlayer(sender);
                Storage.Players.Add(player.Id, player);
                Storage.Save(Settings.StorageFile);
                var plain = $"Added {player.DisplayName} to the notification list, as you commanded.";
                var formatted = $"Added {player.FormattedName} to the notification list, as you commanded.";
                await MatrixApi.SendTextMessageToRoom(roomId, plain, formatted);
            }
            else
            {
                await MatrixApi.SendTextMessageToRoom(roomId, $"Your benevolence is already in the notification list, my liege.");
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
