using System.ComponentModel.DataAnnotations;

namespace Arcmage.DAL.Model
{
    public class DeckCardModel : ModelBase
    {
        [Key]
        public int DeckCardId { get; set; }

        public int Quantity { get; set; }

        public DeckModel Deck { get; set; }

        public CardModel Card { get; set; }

        public string PdfCreationJobId { get; set; }
    }
}
