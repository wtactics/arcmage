using System;

namespace Arcmage.Game.Api.GameRuntime
{
    public class GamePlayer
    {
        public Guid PlayerGuid { get; set; }

        public string Name { get; set; }

        public string Avatar { get; set; }

        public int VictoryPoints { get; set; }

        public bool IsDeckLoaded { get; set; }

        public GameResources Resources { get; private set; }

        public Guid DeckGuid { get; set; }

        public GameList Deck { get; private set; }

        public GameList Graveyard { get; private set; }

        public GameList Hand { get; private set; }

        public GameList Play { get; private set; }

        public GameList Removed { get; private set; }

        public bool ShowCurtain { get; set; }
        

        public GamePlayer()
        {
            Resources = new GameResources();

            Deck = new GameList() {Kind = ListType.Deck};
            Graveyard = new GameList() {Kind = ListType.Graveyard};
            Hand = new GameList() {Kind = ListType.Hand};
            Play = new GameList() {Kind = ListType.Play};
            Removed = new GameList() {Kind = ListType.Removed};
        }
    }
}
