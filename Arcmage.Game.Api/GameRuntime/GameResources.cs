namespace Arcmage.Game.Api.GameRuntime
{
    public class GameResources
    {
        public GameResource Black { get;  set; }

        public GameResource Yellow { get;  set; }

        public GameResource Green { get;  set; }

        public GameResource Blue { get;  set; }

        public GameResource Red { get;  set; }

        public GameResources()
        {
            Black = new GameResource();
            Yellow = new GameResource();
            Green = new GameResource();
            Blue = new GameResource();
            Red = new GameResource();
        }
    }
}
