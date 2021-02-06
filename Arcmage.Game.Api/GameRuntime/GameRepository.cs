using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;

namespace Arcmage.Game.Api.GameRuntime
{
    public class GameRepository : IGameRepository
    {
     
        private readonly List<GameController> GameControllers = new List<GameController>();
       

        private readonly object _callLock = new object();

        private IServiceProvider ServiceProvider { get; set; }

        public GameRepository(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public void PushAction(GameAction gameAction)
        {
            lock (_callLock)
            {
                var gameController = GameControllers.FirstOrDefault(x => x.Game.Guid == gameAction.GameGuid);
                if (gameController == null) return;
                if (gameController.Game.Players.Any(x => x.PlayerGuid == gameAction.PlayerGuid))
                {
                    var result = gameController.ProcessAction(gameAction);
                    PushGameAction(result);
                    if(gameAction.ActionType == GameActionType.LeaveGame)
                    {
                        GameControllers.Remove(gameController);
                        gameController.Dispose();
                    }
                }
                
            }
        }

        public void DestroyGame(Game game)
        {
            lock (_callLock)
            {
                var gameController = GameControllers.FirstOrDefault(x => x.Game.Guid == game.Guid);
                if (gameController == null) return;
                GameControllers.Remove(gameController);
                gameController.Dispose();
            }
        }

        public void PushGameAction(GameAction gameAction)
        {
            lock (_callLock)
            {
                var gameController = GameControllers.FirstOrDefault(x => x.Game.Guid == gameAction.GameGuid);
                if (gameController == null) return;
                var gamesHubContext = ServiceProvider.GetService(typeof(IHubContext<GamesHub>)) as IHubContext<GamesHub>;
                gamesHubContext?.Clients.Group(gameAction.GameGuid.ToString()).SendAsync("ProcessAction", gameAction);
            }
        }


        public GameAction Join(Guid gameGuid, Guid playerGuid, string playerName)
        {
            lock (_callLock)
            {
                var gameAction = new GameAction()
                {
                    GameGuid = gameGuid,
                    PlayerGuid = playerGuid,
                    ActionType = GameActionType.JoinGame,
                    ActionData = playerName,
                    ActionResult = false.ToString(),
                };
                var gameController = GameControllers.FirstOrDefault(x => x.Game.Guid == gameGuid);
                if (gameController != null)
                {
                    gameAction = gameController.ProcessAction(gameAction);
                }
                return gameAction;
            }
        }


        public Game CreateGame(string name)
        {
            
            lock (_callLock)
            {
                var utcNow = DateTime.UtcNow;
                if (string.IsNullOrEmpty(name))
                {
                    name = $"Game {utcNow.Year}-{utcNow.Month}-{utcNow.Day} {utcNow.Hour}:{utcNow.Minute}:{utcNow.Second}";
                }
                
                var game = new Game
                {
                    CreateTime = utcNow,
                    Guid = Guid.NewGuid(),
                    Name = name,
                    CanJoin = true,
                };
                var gameController = new GameController(game, this);
                GameControllers.Add(gameController);
                return game;
            }
        }

        public List<Game> GetGames()
        {
            lock (_callLock)
            {
                return new List<Game>(GameControllers.Select(x=> new Game(){
                    Guid = x.Game.Guid,
                    Name = x.Game.Name,
                    CreateTime = x.Game.CreateTime,
                    CanJoin = x.Game.CanJoin,
                }));
            }
        }
    }
}
