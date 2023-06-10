using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arcmage.Client;
using Arcmage.Model;

namespace DeckTranslator
{
    public class DeckTranslator
    {
        private static Dictionary<string, Dictionary<string, string>> Languages { get; set; }

        private static Dictionary<string, string> CurrentLanguage { get; set; }

        private string TempFile { get; set; }

      
        private ApiClient ApiClient { get; set; }


        static DeckTranslator()
        {
            
            Languages = new Dictionary<string, Dictionary<string, string>>()
            {
                { "french", new Dictionary<string, string>
                {
                    {"Info", "arcmage.org - rejoignez-nous!"},
                    {"Event", "Évènement"},
                    {"Magic", "Magie"},
                    {"Enchantment", "Enchantement"},
                    {"City", "Cité"},
                }},
                { "esperanto", new Dictionary<string, string>
                {
                    {"Info", "arcmage.org - aligi nin!"},
                    {"Event", "Okazaĵo"},
                    {"Magic", "Magio"},
                    {"Enchantment", "Ensorĉo"},
                    {"City", "Urbo"},
                }},
            };
        }
     
        public DeckTranslator(string api, string login, string password, string tempFolder)
        {
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            TempFile = Path.Combine(tempFolder, "temp_art.png");
            ApiClient = new ApiClient(api,login,password);
        }


       
        public async Task TranslateDeck(string guid, string language, bool createCards)
        {
            await ApiClient.Login();

            if (!Languages.ContainsKey(language)) throw new Exception("Language not supported");
            CurrentLanguage = Languages[language];

            var deck = await ApiClient.GetByGuid<Deck>(guid);
            if(deck == null) throw new Exception("Deck not found");

            var newDeck = new Deck() { Name = $"{deck.Name} ({language})" };
            newDeck = await ApiClient.Create(newDeck);

            var counter = 1;
            var total = deck.DeckCards.Count;
            foreach (var deckCard in deck.DeckCards.OrderBy(x => x.Card.Name))
            {
                var card = await ApiClient.GetByGuid<Card>(deckCard.Card.Guid);
                Card newCard;
                if (createCards)
                {
                    Console.WriteLine($"Creating card {counter++}/{total} : {card.Name} ({language} wip)");
                    newCard = await CreateCard(card, language);
                    await Task.Delay(30 * 1000);
                }
                else
                {
                    var cardSearchOptions = new CardSearchOptions
                    {
                        Search = $"{card.Name} ({language}",
                        PageNumber = 1,
                        PageSize = 10
                    };
                    var result = await ApiClient.Search<Card, CardSearchOptions>(cardSearchOptions);
                    newCard = result.Items?.FirstOrDefault();
                    await Task.Delay(1000);
                }

                if (newCard != null)
                {
                    var newDeckCard = new DeckCard()
                    {
                        Card = newCard,
                        Deck = newDeck,
                        Quantity = deckCard.Quantity
                    };
                    await ApiClient.Create(newDeckCard);
                    await Task.Delay(1000);
                }



            }
        }

        private async Task<Card> CreateCard(Card card, string language)
        {
            await ApiClient.DownloadFile(card.Artwork, TempFile);
            var newCard = new Card() {Name = $"{card.Name} ({language} wip)" };
            newCard = await ApiClient.Create(newCard);


            await Task.Delay(1000);
            await ApiClient.UploadFile(newCard.Guid.ToString(), TempFile);
            await Task.Delay(1000);
            TranslateCardFrench(newCard, card);
            // fetch the full card type before updating
            var options = await ApiClient.GetByGuid<CardOptions>(newCard.Guid.ToString());
            newCard.Type = options.CardTypes.FirstOrDefault(x => x.Guid == newCard.Type.Guid);
            // add the link to the original card
            newCard.MasterCard = card;
            await ApiClient.Update(newCard);
            await Task.Delay(1000);
            return newCard;
        }

        private static void TranslateCardFrench(Card newCard, Card card)
        {
            newCard.Serie = card.Serie;
            newCard.RuleSet = card.RuleSet;
            // N.G. Remark: we'll leave the card in draft mode
            // newCard.Status = card.Status;
            newCard.Faction = card.Faction;
            newCard.Type = card.Type;
            newCard.SubType = GetTranslation(card.SubType);
            newCard.Cost = card.Cost;
            newCard.Loyalty = card.Loyalty;
            newCard.RuleText = card.RuleText;
            newCard.FlavorText = card.FlavorText;
            newCard.Attack = card.Attack;
            newCard.Defense = card.Defense;
            newCard.Info = GetTranslation("Info");
            newCard.FirstName = card.FirstName;
            newCard.LastName = card.LastName;
            newCard.MarkdownText = card.MarkdownText;
            newCard.Artist = card.Artist;

        }

        private static string GetTranslation(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return key;
            key = key.Trim();
            if (!CurrentLanguage.ContainsKey(key)) return key;
            return CurrentLanguage[key];
        }
      
    }
}
