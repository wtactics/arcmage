using System;

namespace Arcmage.Game.Api.GameRuntime
{
    public class ShuffleListParam
    {
        public Guid PlayerGuid { get; set; }

        public ListType Kind { get; set; }
    }
}