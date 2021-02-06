using System.Collections.Generic;

namespace Arcmage.Game.Api.GameRuntime
{
    public class GameList
    {
        public List<GameCard> Cards { get; private set; }

        public ListType Kind { get; set; }

        public GameList()
        {
            Cards = new List<GameCard>();
        }
    }
}