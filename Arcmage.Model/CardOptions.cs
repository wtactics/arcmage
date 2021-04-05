using System.Collections.Generic;

namespace Arcmage.Model
{
    public class CardOptions
    {
        public bool IsEditable { get; set; }

        public bool IsStatusChangedAllowed { get; set; }

        public bool IsRulingEditable { get; set; }

        public List<CardType> CardTypes { get; set; }

        public List<Faction> Factions { get; set; }

        public List<Serie> Series { get; set; }

        public List<RuleSet> RuleSets { get; set; }

        public List<Status> Statuses { get; set; }

        public List<License> ArtworkLicenses { get; set; }

        public List<int> Loyalties { get; set; }

        public List<Language> Languages { get; set; }

        public CardOptions()
        {
            Loyalties = new List<int> {0,1,2,3};
            Languages = new List<Language>();
        }

    }
}
