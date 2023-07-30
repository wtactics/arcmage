using System.IO;
using System.Linq;
using Arcmage.DAL;
using Arcmage.DAL.Model;
using Arcmage.Model;
using Arcmage.Server.Api.Utils;

namespace Arcmage.Server.Api.Assembler
{
    public static class DeckAssembler
    {
        public static Deck FromDal(this DeckModel deckModel, bool includeCards = false)
        {
            if (deckModel == null) return null;
            var result = new Deck()
            {
                Guid = deckModel.Guid,
                Id = deckModel.DeckId,
                Name = deckModel.Name,
            };
            if (includeCards)
            {
                deckModel.DeckCards.OrderBy(x=>x.Card.Name).ToList().ForEach(x => result.DeckCards.Add(x.FromDal()));
            }
            result.SyncBase(deckModel, true, true);

            result.Zip = $"/api/Decks/{deckModel.Guid}/export?format=Zip&modified={result.LastModifiedTime.Value.Ticks}";
            result.Txt = $"/arcmage/Decks/{deckModel.Guid}/deck.txt";
            result.IsAvailable = File.Exists(Repository.GetDeckFile(deckModel.Guid));
            result.ExportTiles = deckModel.ExportTiles;
            result.GeneratePdf = deckModel.GeneratePdf;
            result.Language = Languages.GetLanguage(deckModel.LanguageCode);

            if (deckModel.DeckCards != null && deckModel.DeckCards.Any())
            {
                result.TotalCards =  deckModel.DeckCards.Sum(x => x.Quantity);
            }
            result.IsGenerated = File.Exists(Repository.GetDeckZipFile(deckModel.Guid)) && string.IsNullOrEmpty(deckModel.PdfZipCreationJobId);

            if (deckModel.Status != null) result.Status = deckModel.Status.FromDal();

            return result;
        }

        public static void Patch(this DeckModel deckModel, Deck deck, StatusModel statusModel, UserModel user)
        {
            if (deckModel == null) return;
            deckModel.Name = deck.Name;
            deckModel.LanguageCode = deck.Language?.LanguageCode ?? "en";
            deckModel.ExportTiles = deck.ExportTiles;
            deckModel.GeneratePdf = deck.GeneratePdf;
            
            if (statusModel != null) deckModel.Status = statusModel;
            deckModel.PatchBase(user);
        }
    }
}
