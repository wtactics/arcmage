using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Arcmage.Client;
using Arcmage.Model;

namespace DeckAutoCompleter
{
    class AutoCompleter
    {
        private ApiClient ApiClient { get; set; }

        public AutoCompleter(string api, string login, string password)
        {
            ApiClient = new ApiClient(api, login, password);
        }

        public async Task AutoComplete(string deckGuid)
        {
            await ApiClient.Login();

            var deck = await ApiClient.GetByGuid<Deck>(deckGuid);
            if (deck == null) throw new Exception("Deck not found");

            var total = deck.DeckCards.Count;
            foreach (var deckCard in deck.DeckCards.OrderBy(x => x.Card.Name))
            {
                var card = await ApiClient.GetByGuid<Card>(deckCard.Card.Guid);
                await Task.Delay(1000);
                await AutoComplete(card);
                await Task.Delay(1000);
       
            }
        }

        private async Task AutoComplete(Card card)
        {
            var layoutText = card.LayoutText;

            var ruleText = string.Empty;
            var flavourText = string.Empty;


            var flavorIndex = layoutText.IndexOf("<p><i>\"", StringComparison.InvariantCulture);
            if (flavorIndex != -1)
            {
                
                flavourText = layoutText.Substring(flavorIndex);
                var flavourDocument = XDocument.Parse($"<root>{flavourText}</root>");
                flavourText = string.Empty;
                foreach (var xElement in flavourDocument.Root.Elements("p"))
                {
                    var para = ParseParagraph(xElement).Trim();
                    if (!string.IsNullOrWhiteSpace(para))
                    {
                        flavourText += para.Trim().Trim('"').Trim() + "\n";
                    }
                }

                layoutText = layoutText.Substring(0, flavorIndex);
            }

            

            var ruleDocument = XDocument.Parse($"<root>{layoutText}</root>");
            foreach (var xElement in ruleDocument.Root.Elements("p"))
            {
                var para = ParseParagraph(xElement).Trim();
                if (!string.IsNullOrWhiteSpace(para))
                {
                    ruleText += para + "\n";
                }
                
            }

            Console.WriteLine(
                $"Card  : {card.Name}\n" + 
                $"Rule  : {ruleText}" +
                $"Flavor: {flavourText}");
            Console.WriteLine("---");

            card.RuleText = ruleText;
            if (!string.IsNullOrWhiteSpace(flavourText)) card.FlavorText = flavourText;

            await ApiClient.Update(card);

        }

        public string ParseParagraph(XElement paragraph)
        {
            var para = string.Empty;
          
            foreach (var xElement in paragraph.Elements())
            {
                // start of the paragraph, reset
                var line = string.Empty;
                switch (xElement.Name.LocalName)
                {
                    // Capital letter, should only occur as the first item of a paragraph
                    case "c":
                        line = xElement.Value;
                        break;
                    // Large symbol, should only occur as the first item of a paragraph
                    case "m":
                        line = "M,";
                        break;
                    case "m0":
                        line = "M0,";
                        break;
                    case "m1":
                        line = "M1,";
                        break;
                    case "m2":
                        line = "M2,";
                        break;
                    case "m3":
                        line = "M3,";
                        break;
                    case "m4":
                        line = "M4,";
                        break;
                    case "m5":
                        line = "M5,";
                        break;
                    case "m6":
                        line = "M6,";
                        break;
                    case "m7":
                        line = "M7,";
                        break;
                    case "m8":
                        line = "M8,";
                        break;
                    case "m9":
                        line = "M9,";
                        break;
                    case "ma":
                        line = "A,";
                        break;
                    case "mt":
                        line = "T,";
                        break;
                    case "mx":
                        line = "X,";
                        break;
                    case "mc":
                        line = "M,";
                        break;
                    case "mi":
                        line = "M,";
                        break;
                    case "mic":
                        line = "M,";
                        break;
                    case "mis":
                        line = "M,";
                        break;
                    case "g0":
                        line = "0";
                        break;
                    case "g1":
                        line = "1";
                        break;
                    case "g2":
                        line = "2";
                        break;
                    case "g3":
                        line = "3";
                        break;
                    case "g4":
                        line = "4";
                        break;
                    case "g5":
                        line = "5";
                        break;
                    case "g6":
                        line = "6";
                        break;
                    case "g7":
                        line = "7";
                        break;
                    case "g8":
                        line = "8";
                        break;
                    case "g9":
                        line = "9";
                        break;
                    case "gt":
                        line = "T";
                        break;
                    case "gx":
                        line = "X";
                        break;

                    // normal text
                    case "n":
                        line = xElement.Value;
                        break;
                    // normal text
                    case "b":
                        line = xElement.Value;
                        break;
                    case "i":
                        line = xElement.Value;
                        break;
                    case "bi":
                        line = xElement.Value;
                        break;
                    case "br":
                        break;
                }

                para += line;
            }

            return para;
        }
    }
}
