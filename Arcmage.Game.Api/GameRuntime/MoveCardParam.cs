using System;

namespace Arcmage.Game.Api.GameRuntime
{
    public class MoveCardParam
    {
        public Guid FromPlayerGuid { get; set; }

        public ListType FromKind { get; set; }

        public Guid ToPlayerGuid { get; set; }

        public ListType ToKind { get; set; }

        public Guid? CardId { get; set; }

        public int? Index { get; set; }

        public ChangeCardParam CardState { get; set; }
        

    }
}