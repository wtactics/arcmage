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
                { "fr", new Dictionary<string, string>
                {
                    {"Info", "arcmage.org - rejoignez-nous!"},
                    {"Event", "Évènement"},
                    {"Magic", "Magie"},
                    {"Enchantment", "Enchantement"},
                    {"City", "Cité"},
                }},
                // esperanto
                { "eo", new Dictionary<string, string>
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


       
        public async Task TranslateDeck(string guid, string language, bool dryRun = true)
        {

            if (dryRun)
            {
                Console.WriteLine($"Deck translation runs in dry run mode, no decks or cards will be created.");
            }

            if (!Languages.ContainsKey(language))
            {
                Console.WriteLine($"Language ({language}) not supported");
                return;
            }

            await ApiClient.Login();

            CurrentLanguage = Languages[language];

            var deck = await ApiClient.GetByGuid<Deck>(guid);
            if (deck == null)
            {
                Console.WriteLine($"Deck ({guid}) not found");
                return;
            }

            Console.WriteLine($"Translating deck {deck.Name} to {language}");

            var newDeck = new Deck()
            {
                Name = $"{deck.Name} ({language})",
                Status = new Status() { Guid = PredefinedGuids.Draft }
            };
            Console.WriteLine($"Creating deck {newDeck.Name}");
            if (!dryRun)
            {
                newDeck = await ApiClient.Create(newDeck);
            }
            

            var counter = 1;
            var total = deck.DeckCards.Count;
            foreach (var deckCard in deck.DeckCards.OrderBy(x => x.Card.Name))
            {
                var card = await ApiClient.GetByGuid<Card>(deckCard.Card.Guid);

                // Search of the card has already been translated into the given language
                var cardSearchOptions = new CardSearchOptions
                {
                    PageNumber = 1,
                    PageSize = 10,
                    Language = new Language() { LanguageCode = language },
                    MasterCard = card,
                };
                var result = await ApiClient.Search<Card, CardSearchOptions>(cardSearchOptions);
                var newCard = result.Items?.FirstOrDefault();
                await Task.Delay(1000);
                
                
                if (newCard != null)
                {
                    Console.WriteLine($"Progress {counter++}/{total} : Card {card.Name} already translated to {language} ");
                }
                else
                {
                    // We haven't found a translation of the given card in the given language, create it now
                    Console.WriteLine($"Progress {counter++}/{total} : Creating card {card.Name} ({language} wip)");
                    if (!dryRun)
                    {
                        newCard = await CreateCard(card, language);
                        await Task.Delay(30 * 1000);
                    }
                }
            

                if (newCard != null && !dryRun)
                {
                    //  add it to the newly created deck.
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


        public async Task ResumeTranslateDeck(string sourceGuid, string destinationGuid, string language, bool dryRun = true)
        {

            if (dryRun)
            {
                Console.WriteLine($"Deck translation runs in dry run mode, no decks or cards will be created.");
            }

            if (!Languages.ContainsKey(language))
            {
                Console.WriteLine($"Language ({language}) not supported");
                return;
            }

            await ApiClient.Login();

            CurrentLanguage = Languages[language];

            var sourceDeck = await ApiClient.GetByGuid<Deck>(sourceGuid);
            if (sourceDeck == null)
            {
                Console.WriteLine($"Source deck ({sourceGuid}) not found");
                return;
            }

            Console.WriteLine($"Resume translating deck {sourceDeck.Name} to {language}");

            var newDeck = await ApiClient.GetByGuid<Deck>(destinationGuid);
            if (newDeck == null)
            {
                Console.WriteLine($"Destination deck ({destinationGuid}) not found");
                return;
            }

            var counter = 1;
            var total = sourceDeck.DeckCards.Count;
            foreach (var deckCard in sourceDeck.DeckCards.OrderBy(x => x.Card.Name))
            {
                var card = await ApiClient.GetByGuid<Card>(deckCard.Card.Guid);

                // Search of the card has already been translated into the given language
                var cardSearchOptions = new CardSearchOptions
                {
                    PageNumber = 1,
                    PageSize = 10,
                    Language = new Language() { LanguageCode = language },
                    MasterCard = card,
                };
                var result = await ApiClient.Search<Card, CardSearchOptions>(cardSearchOptions);
                var newCard = result.Items?.FirstOrDefault();
                await Task.Delay(1000);


                if (newCard != null)
                {
                    Console.WriteLine($"Progress {counter++}/{total} : Card {card.Name} already translated to {language} ");
                }
                else
                {
                    // We haven't found a translation of the given card in the given language, create it now
                    Console.WriteLine($"Progress {counter++}/{total} : Creating card {card.Name} ({language} wip)");
                    if (!dryRun)
                    {
                        newCard = await CreateCard(card, language);
                        await Task.Delay(30 * 1000);
                    }
                }


                if (newCard != null && !dryRun)
                {

                    if (newDeck.DeckCards.Exists(x => x.Card.Guid == newCard.Guid))
                    {
                        Console.WriteLine("Card already added to the deck");
                    }
                    else
                    {
                        //  add it to the newly created deck.
                        var newDeckCard = new DeckCard()
                        {
                            Card = newCard,
                            Deck = newDeck,
                            Quantity = deckCard.Quantity
                        };
                        await ApiClient.Create(newDeckCard);
                        newDeck.DeckCards.Add(newDeckCard);
                        await Task.Delay(1000);
                    }


                   
                }
            }
        }


        private async Task<Card> CreateCard(Card card, string language)
        {
            await ApiClient.DownloadFile(card.Artwork, TempFile);
            var newCard = new Card() {Name = $"{card.Name} ({language} wip)" };
            TranslateCardFrench(newCard, card);
            newCard.Language = new Language() { LanguageCode = language };
            newCard = await ApiClient.Create(newCard);
            await Task.Delay(1000);
            await ApiClient.UploadFile(newCard.Guid.ToString(), TempFile);
            await Task.Delay(1000);

            // copy markdown text and force the update to regenerate the card
            newCard.MarkdownText = card.MarkdownText;
            await ApiClient.Update(newCard);
            await Task.Delay(1000);

            return newCard;
        }

        private static void TranslateCardFrench(Card newCard, Card card)
        {
            newCard.Serie = card.Serie;
            newCard.RuleSet = card.RuleSet;
            // N.G. Remark: we'll leave the card in draft mode
            newCard.Status = new Status() { Guid = PredefinedGuids.Draft };
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
            newCard.Artist = card.Artist;
            newCard.ArtworkLicense = card.ArtworkLicense;
            newCard.ArtworkLicensor = card.ArtworkLicensor;
            // add the link to the original card
            newCard.MasterCard = card;
            // we're not copying the markdown text here
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
