using System;
using System.Collections.Generic;

namespace Arcmage.Game.Api.GameRuntime
{
    public class UpdateListParam
    {
        public Guid PlayerGuid { get; set; }

        public ListType Kind { get; set; }

        public List<ChangeCardParam> Cards { get; set; }

        public UpdateListParam()
        {
            Cards = new List<ChangeCardParam>();
        }
    }
}