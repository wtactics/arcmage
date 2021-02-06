namespace Arcmage.Model
{
    public class DeckCard : Base
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        public Card Card { get; set; }

        public Deck Deck { get; set; }
    }
}
