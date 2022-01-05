using System;

namespace Arcmage.Game.Api.GameRuntime
{
    public class GameCard
    {
        
        public Guid CardId { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public bool IsFaceDown { get; set; }

        public bool IsMarked { get; set; }

        public bool IsDraggable { get; set; }

        public double Top { get; set; }

        public double Left { get; set; }

        public int CounterA { get; set; }

        public int CounterB { get; set; }

        public string RuleText { get; set; }

        public string FlavorText { get; set; }

        public string SubType { get; set; }

        public bool IsCity { get; set; }

        public bool IsToken { get; set; }

        public bool IsPeeking { get; set; }

        public bool IsPointed { get; set; }

        public GameCard()
        {
            CardId = Guid.NewGuid();
        }
    }
}
