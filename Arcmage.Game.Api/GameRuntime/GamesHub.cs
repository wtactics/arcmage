using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Arcmage.Game.Api.GameRuntime
{
    public class GamesHub : Hub
    {

        private readonly IGameRepository _gameRepository;

        public GamesHub(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }


        public async Task JoinGame(Guid gameGuid, Guid playerGuid, string playerName)
        {

            var gameAction = _gameRepository.Join(gameGuid, playerGuid, playerName);
            if ( (bool)gameAction.ActionResult)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, gameGuid.ToString());
                _gameRepository.PushGameAction(gameAction);
            }
        }

        public async Task LeaveGame(Guid gameGuid, Guid playerGuid)
        {
            var gameAction = new GameAction()
            {
                GameGuid = gameGuid,
                PlayerGuid = playerGuid,
                ActionType = GameActionType.LeaveGame,
            };
            _gameRepository.PushAction(gameAction);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameGuid.ToString());
        }

        public async Task PushAction(GameAction gameAction)
        {
            _gameRepository.PushAction(gameAction);
        }

        public async Task ProcessAction(GameAction gameAction)
        {
        }

    }
}