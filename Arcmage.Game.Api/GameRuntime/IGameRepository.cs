using System;
using System.Collections.Generic;

namespace Arcmage.Game.Api.GameRuntime
{
    public interface IGameRepository
    {
       
        void PushAction(GameAction gameAction);
        void DestroyGame(Game game);
        void PushGameAction(GameAction gameAction);
        GameAction Join(Guid gameGuid, Guid playerGuid, string playerName);
        Game CreateGame(string name);
        List<Game> GetGames();
    }
}