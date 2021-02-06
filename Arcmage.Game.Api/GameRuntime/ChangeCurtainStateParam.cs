using System;

namespace Arcmage.Game.Api.GameRuntime
{
    public class ChangeCurtainStateParam
    {
        public Guid PlayerGuid { get; set; }

        public bool ShowCurtain { get; set; }
    }
}
