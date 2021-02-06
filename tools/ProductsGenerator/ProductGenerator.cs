using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcmage.Client;
using Arcmage.Model;

namespace ProductsGenerator
{
    class ProductGenerator
    {
        private static Dictionary<string, Dictionary<string, string>> Languages { get; set; }

        private static Dictionary<string, string> CurrentLanguage { get; set; }

        private string OutputFile { get; set; }


        private ApiClient ApiClient { get; set; }


        static ProductGenerator()
        {

            Languages = new Dictionary<string, Dictionary<string, string>>()
            {
                { "french", new Dictionary<string, string>
                {
                    {"Info", "arcmage.org - rejoignez-nous!"},
                    {"Event", "Evénement"},
                    {"Magic", "Magie"},
                    {"Enchantment", "Enchantment"},
                    {"City", "Ville"},
                }},
            };
        }

        public ProductGenerator(string api, string login, string password, string outputFile)
        {
          
            OutputFile = outputFile;
            ApiClient = new ApiClient(api, login, password);
        }



        public async Task GenerateProductsCsv()
        {
            await ApiClient.Login();

            var records = new List<string>();

            var header = "ID,Type,SKU,Name,Published,\"Is featured?\",\"Visibility in catalog\",\"Short description\",\"Description\",\"Tax status\",\"In stock?\",\"Backorders allowed?\",\"Sold individually?\",\"Weight (g)\",\"Length (cm)\",\"Width (cm)\",\"Height (cm)\",\"Allow customer reviews?\",\"Regular price\",Categories,Tags,Cross-sells,Position";
            records.Add(header);

            var releaseCandidateGuid = Guid.Parse("7DEDC883-5DD2-5F17-B2A4-EAF04F7AD464");

            var options = await ApiClient.Get<CardOptions>();

           
            var cardSearchOptions = new CardSearchOptions
            {
                PageNumber = 1,
                PageSize = 1,
                Status = options.Statuses.FirstOrDefault(x => x.Guid == releaseCandidateGuid)
            };
            var result = await ApiClient.Search<Card, CardSearchOptions>(cardSearchOptions);

            var totalItems = result.TotalItems;
            var processedItems = 0;

            var pageNumber = 1;
            cardSearchOptions.PageSize = 50;

            while (processedItems < totalItems)
            {
                cardSearchOptions.PageNumber = pageNumber;
                result = await ApiClient.Search<Card, CardSearchOptions>(cardSearchOptions);
                foreach (var card in result.Items)
                {
                    processedItems++;
                    var record = CreateCardRecord(card);
                    records.Add(record);
                }
                pageNumber++;
                await Task.Delay(1000);
            }

            File.WriteAllLines(OutputFile, records, Encoding.UTF8);

           

        }

        private static string CreateCardRecord(Card card)
        {
            var id = "";
            var type = "simple";
            var SKU = card.Guid.ToString();
            var name = Escape(card.Name);
            var published = "1";
            var isFeatured = "0";
            var visibleInCatalog = "visible";
            var shortDescription = Escape(card.Name);
            var description = Escape("Rule Text:\n" + card.RuleText + "\n\n" + "Flavor Text:\n" + card.FlavorText);
            var taxStatus = "taxable";
            var inStock = "1";
            var backOrdersAllowed = "0";
            var soldIndividually = "0";
            var weight = "0.1";
            var length = "9.0";
            var width = "6.4";
            var height = "0";
            var allowCustomersReviews = "0";
            var regularPrice = "0.15";
            var categories = "Cards";
            var tags = GetCardTags(card);
            var crossSels = Escape("id:1008, id:1007, id:1004");
            var position = "0";

            var record = $"{id},{type},{SKU},{name},{published},{isFeatured},{visibleInCatalog},{shortDescription},{description},{taxStatus},{inStock},{backOrdersAllowed},{soldIndividually},{weight},{length},{width},{height},{allowCustomersReviews},{regularPrice},{categories},{tags},{crossSels},{position}";
            return record;
        }

        private static string GetCardTags(Card card)
        {
            return Escape(card.Faction.Name + "," + card.Type.Name);
        }

        private static string Escape(string str)
        {
            var parts = str.Split('"');
            str = string.Join("\"\"", parts);
            return $"\"{str}\"";
        }
    }
}
