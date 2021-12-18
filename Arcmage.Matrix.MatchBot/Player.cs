using System;

namespace Arcmage.Matrix.MatchBot
{
    public class Player
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string FormattedName { get; set; }

        public TimeSpan TimeZoneOffset { get; set; }

        public string TimeZone { get; set; }

    }
}
