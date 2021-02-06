using Arcmage.DAL.Model;
using Arcmage.Model;

namespace Arcmage.Server.Api.Assembler
{
    public static class DeckCardAssembler
    {
        public static DeckCard FromDal(this DeckCardModel deckCardModel, bool includeDeck = false)
        {
            if (deckCardModel == null) return null;
            var result = new DeckCard
            {
                Quantity = deckCardModel.Quantity,
                Card =  deckCardModel.Card.FromDal(),
            };
            if (includeDeck)
            {
                result.Deck = deckCardModel.Deck.FromDal();
            }

            return result.SyncBase(deckCardModel);
        }

        public static void Patch(this DeckCardModel deckCardModel, DeckCard deckCard, UserModel user)
        {
            if (deckCardModel == null) return;
            deckCardModel.Quantity = deckCard.Quantity;
            deckCardModel.Patch(user);
        }
    }
}
