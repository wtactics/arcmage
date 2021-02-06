using System;

namespace Arcmage.Game.Api.GameRuntime
{
    public class GameAction
    {

        // Needed for identification of the game
        public Guid GameGuid { get; set; }

        // Needed for identification of the player
        public Guid PlayerGuid { get; set; }

        // Kind of action
        public GameActionType ActionType { get; set; }

        // parameters of the call
        public object ActionData { get; set; }

        // result of the call
        public object ActionResult { get; set; }

    }
}
