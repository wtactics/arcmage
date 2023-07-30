using System;
using System.Collections.Generic;
using System.Text;

namespace Arcmage.Model
{
    public class DeckOptions
    {

        public bool IsEditable { get; set; }

        public bool IsStatusChangedAllowed { get; set; }

        public List<Status> Statuses { get; set; }

        public List<Language> Languages { get; set; }

        public DeckOptions()
        {
            Languages = new List<Language>();
        }
    }
}
