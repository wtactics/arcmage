using System.Collections.Generic;

namespace Arcmage.Model
{
    public class Deck : Base
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Zip { get; set; }

        public string Txt { get; set; }

        public bool ExportTiles { get; set; }

        public bool GeneratePdf { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsEditable { get; set; }

        public bool IsGenerated { get; set; }

        public Status Status { get; set; }

        public List<DeckCard> DeckCards { get; set; }

        public int TotalCards { get; set; }

        public Deck()
        {
            DeckCards = new List<DeckCard>();
        }

    }
}
