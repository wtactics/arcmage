using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Arcmage.DAL;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Arcmage.Game.Api.GameRuntime
{
    public class GameController: IDisposable
    {
        public static double Scale = 3;

        public static double BattleFieldWidth = 1920 * Scale;
        public static double BattleFieldHeight = 1200 * Scale;

        public static double CenterLeft = 907 * Scale;
        public static double CenterTop = 800 * Scale;
        public static double CardWitdh = 106 * Scale;
        public static double CardHeight = 150 * Scale;
        public static double LayoutGap = 10 * Scale;

        public static double TokenStartLeft = BattleFieldWidth - CardWitdh - CardWitdh - 70 * Scale;
        public static double TokenStartTop = 850 * Scale + CardHeight + 60 * Scale;
        public static double TokenGap = 5 * Scale;

        public static double CityStartLeft = 136 * Scale;
        public static double CityStartTop = 772 * Scale;


        // Games are cancelled after 24 hours.
        public TimeSpan AbsoluteTimeOut = TimeSpan.FromHours(24);
       
        public Game Game { get; private set; }

        private GameRepository GameRepository { get; set; }

        private Timer Timer { get; set; }

        private Random Random { get; set; }

        public GameController(Game game, GameRepository gameRepository)
        {
            Game = game;
            GameRepository = gameRepository;
            Timer = new Timer(30000);
            Timer.Elapsed += Timer_Elapsed;
            Timer.AutoReset = true;
            Timer.Enabled = true;
            Random = new Random();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var player in Game.Players)
            {
                var updateListParam = new UpdateListParam
                {
                    PlayerGuid = player.PlayerGuid,
                    Kind = ListType.Play,
                    Cards = player.Play.Cards.Select(x => new ChangeCardParam()
                    {
                        CardId = x.CardId,
                        CounterA = x.CounterA,
                        CounterB = x.CounterB,
                        IsDraggable = x.IsDraggable,
                        IsFaceDown = x.IsFaceDown,
                        IsMarked = x.IsMarked,
                        Left = x.Left,
                        Top = x.Top
                    }).ToList()
                };

                var gameAction = new GameAction
                {
                    GameGuid = Game.Guid,
                    PlayerGuid = player.PlayerGuid,
                    ActionType = GameActionType.UpdateList,
                    ActionData = JsonConvert.SerializeObject(updateListParam)

                };
                GameRepository.PushGameAction(gameAction);
            }

            if (Game.StarTime.Add(AbsoluteTimeOut) < DateTime.Now)
            {
                GameRepository.DestroyGame(Game);
            }
        }

        public GameAction ProcessAction(GameAction action)
        {
            Game.LastAction = DateTime.Now;
           
            var player = Game.GetPlayer(action.PlayerGuid);
            switch (action.ActionType)
            {
                case GameActionType.JoinGame:
                    action.ActionResult = JoinGameAction(player, action.PlayerGuid, action.ActionData.ToString());
                    break;
                case GameActionType.LeaveGame:
                    action.ActionResult = LeaveGameAction(player, action.PlayerGuid).ToString();
                    break;
                case GameActionType.LoadDeck:
                    var loadDeckParam = JsonConvert.DeserializeObject<LoadDeckParam>(action.ActionData.ToString());
                    var loaded = LoadDeck(player, loadDeckParam);
                    // start the game if everything is loaded
                    var startaction = StartGame(action);
                    if (startaction != null) { action = startaction;}
                    break;
                case GameActionType.StartGame:
                    break;
                case GameActionType.ShuffleList:
                    var shuffleListParam = JsonConvert.DeserializeObject<ShuffleListParam>(action.ActionData.ToString());
                    action.ActionResult = ShuffleListAction(shuffleListParam);
                    break;
                case GameActionType.UpdateList:
                    var updateListParam = JsonConvert.DeserializeObject<UpdateListParam>(action.ActionData.ToString());
                    action.ActionResult = UpdateListAction(updateListParam);
                    break;
                case GameActionType.DrawCard:
                case GameActionType.DiscardCard:
                case GameActionType.PlayCard:
                case GameActionType.DeckCard:
                case GameActionType.RemoveCard:
                    var moveCardParam = JsonConvert.DeserializeObject<MoveCardParam>(action.ActionData.ToString());
                    action.ActionResult = MoveCardAction(moveCardParam);
                    action.ActionData = moveCardParam;
                    break;
                case GameActionType.ChangeCardState:
                    var changeCardParam = JsonConvert.DeserializeObject<ChangeCardParam>(action.ActionData.ToString());
                    action.ActionResult = ChangeCardState(changeCardParam);
                    break;
                case GameActionType.ChangePlayerStats:
                    var changePlayerStats = JsonConvert.DeserializeObject<ChangePlayerStatsParam>(action.ActionData.ToString());
                    LimitResources(changePlayerStats);
                    LimitVictoryPoints(changePlayerStats);
                    action.ActionData = changePlayerStats;
                    break;
                case GameActionType.ChangeCurtainState:
                    var changeCurtainState = JsonConvert.DeserializeObject<ChangeCurtainStateParam>(action.ActionData.ToString());
                    ChangeCurtainState(changeCurtainState);
                    action.ActionData = changeCurtainState;
                    break;
                case GameActionType.FlipCoin:
                    action.ActionResult = FlipCoin();
                    break;
                case GameActionType.DiceRoll:
                    action.ActionResult = DiceRoll();
                    break;
            }

            return action;

        }

        private void ChangeCurtainState(ChangeCurtainStateParam changeCurtainState)
        {
            var player = Game.Players.FirstOrDefault(x => x.PlayerGuid == changeCurtainState.PlayerGuid);
            if (player != null)
            {
                player.ShowCurtain = changeCurtainState.ShowCurtain;
            }
        }

        private int DiceRoll()
        {
            return Random.Next(1,7);
        }

        private string FlipCoin()
        {
            return (Random.NextDouble() > 0.5)? "Heads" : "Tails";
        }


        private bool ChangeCardState(ChangeCardParam state, bool includeLocation = true)
        {
            var card = Game.Cards.FirstOrDefault(x => x.CardId == state.CardId);
            if (card != null)
            {
                if (state.IsMarked != null) card.IsMarked = state.IsMarked.Value;
                if (state.IsDraggable != null) card.IsDraggable = state.IsDraggable.Value;
                if (state.IsFaceDown != null) card.IsFaceDown = state.IsFaceDown.Value;
                if (state.CounterA != null) card.CounterA = Math.Max(0,state.CounterA.Value);
                if (state.CounterB != null) card.CounterB = Math.Max(0, state.CounterB.Value);
                if (state.IsPeeking != null) card.IsPeeking = state.IsPeeking.Value;
                if (state.IsPointed != null) card.IsPointed = state.IsPointed.Value;

                if (state.Top != null && state.Left != null && includeLocation)
                {
                    UpdateCardLocation(card, state.Top.Value, state.Left.Value);
                }
            }
            return true;
        }

        private void UpdateCardLocation(GameCard card, double top, double left)
        {
            var isCardInPlay = Game.Players.SelectMany(x => x.Play.Cards).Any(x => x.CardId == card.CardId);
            if (isCardInPlay)
            {
                card.Top = top;
                card.Left = left;
            }
            else
            {
                card.Top = 0;
                card.Left = 0;
            }
        }

        private void LimitVictoryPoints(ChangePlayerStatsParam changePlayerStats)
        {
            changePlayerStats.VictoryPoints = Math.Max(0, Math.Min(changePlayerStats.VictoryPoints, 30));
        }

        private void LimitResources(ChangePlayerStatsParam changePlayerStats)
        {
            LimitResource(changePlayerStats.Resources.Black);
            LimitResource(changePlayerStats.Resources.Blue);
            LimitResource(changePlayerStats.Resources.Green);
            LimitResource(changePlayerStats.Resources.Red);
            LimitResource(changePlayerStats.Resources.Yellow);
        }

        private void LimitResource(GameResource resource)
        {
            resource.Available = Math.Max(0, Math.Min(resource.Available, 99));
            resource.Used = Math.Max(0, Math.Min(resource.Used, resource.Available));
        }

        private bool MoveCardAction(MoveCardParam moveParam)
        {
            var sourceList = Game.GetList(moveParam.FromPlayerGuid, moveParam.FromKind);
            var targetList = Game.GetList(moveParam.ToPlayerGuid, moveParam.ToKind);
            if (moveParam.CardId.HasValue)
            {
                var card = sourceList.Cards.FirstOrDefault(x => x.CardId == moveParam.CardId.Value);
                if (card != null)
                {
                    sourceList.Cards.Remove(card);
                    targetList.Cards.Push(card);
                    moveParam.CardState.CardId = card.CardId;
                }
            }
            else
            {
                var card = sourceList.Cards.Pop();
                if (card != null)
                {
                    targetList.Cards.Push(card);
                    moveParam.CardState.CardId = card.CardId;
                }
            }


            if (moveParam.ToKind != ListType.Play)
            {
                moveParam.CardState.Left = 0;
                moveParam.CardState.Top = 0;
            }
            else
            {
                // if moving to play, center on battlefield
                if (!moveParam.CardState.Left.HasValue)
                {
                    moveParam.CardState.Left = CenterLeft;
                }
                if (!moveParam.CardState.Top.HasValue) { 
                    moveParam.CardState.Top = CenterTop;
                }
            }
            ChangeCardState(moveParam.CardState);
            return true;
        }

        private List<GameCard> ShuffleListAction(ShuffleListParam shuffleListParam)
        {
            var list = Game.GetList(shuffleListParam.PlayerGuid, shuffleListParam.Kind).Cards;
            list.Shuffle();
            return list;
        }

        private List<GameCard> UpdateListAction(UpdateListParam updateListParam)
        {
            var list = Game.GetList(updateListParam.PlayerGuid, updateListParam.Kind).Cards;
            list.Clear();
            foreach (var gameCard in updateListParam.Cards)
            {
                var card = Game.Cards.FirstOrDefault(x => x.CardId == gameCard.CardId);
                if (card != null)
                {
                    list.Add(card);
                    ChangeCardState(gameCard, updateListParam.Kind == ListType.Play);
                }
            }
            return list;
        }


        private GameAction StartGame(GameAction action)
        {
            if (!Game.CanJoin && Game.Players.All(x => x.IsDeckLoaded))
            {
                Game.Players.ForEach(x => Game.Cards.AddRange(x.Play.Cards));
                Game.Players.ForEach(x => Game.Cards.AddRange(x.Deck.Cards));
                Game.Players.ForEach(x => Game.Cards.AddRange(x.Graveyard.Cards));
                Game.Players.ForEach(x => Game.Cards.AddRange(x.Hand.Cards));

                Game.IsStarted = true;
                Timer.Start();
                return new GameAction
                {
                    ActionType = GameActionType.StartGame,
                    PlayerGuid = action.PlayerGuid,
                    GameGuid = action.GameGuid,
                    ActionData = null,
                    ActionResult = Game
                };
            }
            return null;
        }

        private bool JoinGameAction(GamePlayer player, Guid playerGuid, string playerName)
        {
            if (player == null && Game.CanJoin)
            {
                var playernumber = Game.Players.Count() + 1;
                var avatar = playernumber == 1 ? "player.webp" : "opponent.webp";
                player = new GamePlayer()
                {
                    PlayerGuid = playerGuid,
                    Name = playerName,
                    VictoryPoints = 15,
                    Avatar = avatar,
                };
                Game.Players.Add(player);
                Game.CanJoin = Game.Players.Count < 2;
                return true;
            }
            return false;
        }

        private bool LeaveGameAction(GamePlayer player, Guid playerGuid)
        {
            return true;
        }

        private bool LoadDeck(GamePlayer player, LoadDeckParam loadDeckParam)
        {
            if(player == null) return false;
            if(player.IsDeckLoaded) return true;

            using (var repository = new Repository())
            {
                repository.Context.Factions.Load();
                repository.Context.CardTypes.Load();
                var deckModel = repository.Context.Decks
                    .Include(x=>x.DeckCards)
                    .ThenInclude(x => x.Card).FirstOrDefault(x => x.Guid == loadDeckParam.DeckGuid);
                if (deckModel == null) return false;

                var cityLeft = CityStartLeft;
                var cityTop = CityStartTop;

                var tokenLeft = TokenStartLeft;
                var tokenTop = TokenStartTop;

                foreach (var deckCard in deckModel.DeckCards)
                {
                    for (int i = 0; i < deckCard.Quantity; i++)
                    {
                        var c = new GameCard()
                        {
                            Name = deckCard.Card.Name,
                            Url = $"/Arcmage/Cards/{deckCard.Card.Guid}/card.webp",
                            IsFaceDown = true,
                            IsDraggable = true,
                            CounterA = 0,
                            CounterB = 0,
                            RuleText = deckCard.Card.RuleText,
                            FlavorText = deckCard.Card.FlavorText,
                            SubType = deckCard.Card.SubType,
                            IsCity = !string.IsNullOrWhiteSpace(deckCard.Card.SubType) && deckCard.Card.SubType.Contains("City"),
                            IsToken = !string.IsNullOrWhiteSpace(deckCard.Card.SubType) && deckCard.Card.SubType.Contains("Token"),
                        };

                        if (c.IsCity)
                        {
                            if (int.TryParse(deckCard.Card.Defense, out var strength))
                            {
                                c.CounterB = strength;
                            }
                            c.IsFaceDown = false;
                            c.Left = cityLeft;
                            c.Top = cityTop;
                            cityLeft += CardWitdh + LayoutGap;
                            player.Play.Cards.Push(c);
                            continue;
                        }

                        if (c.IsToken)
                        {
                            c.IsFaceDown = false;
                            c.Left = tokenLeft;
                            c.Top = TokenStartTop;
                            tokenLeft += TokenGap;
                            tokenTop += TokenGap;
                            player.Play.Cards.Push(c);
                            continue;
                        }
                        player.Deck.Cards.Push(c);



                    }

                }
                player.Deck.Cards.Shuffle();
                player.IsDeckLoaded = true;
                return true;

            }
        }


        public void Dispose()
        {
            Timer.Stop();
            Timer.Dispose();
    }
    }
}
