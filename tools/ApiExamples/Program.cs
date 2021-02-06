using System;
using System.Linq;
using System.Threading.Tasks;
using Arcmage.Client;
using Arcmage.Model;

namespace ApiExamples
{
    class Program
    {
        private static ApiClient ApiClient { get; set; }

        private static CardOptions CardOptions { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Running some examples!");

            Task.Run(async () =>
            {
                await Login();
                await LogSeries();
                await GetCardOptions();
                await SearchCards();

            }).GetAwaiter().GetResult();
            Console.ReadKey();

        }

        private static async Task SearchCards()
        {
            var cardSearchOptions = new CardSearchOptions
            {
                Search = "Archer",
                PageNumber = 1,
                PageSize = 100,
               // Faction = CardOptions.Factions.FirstOrDefault()
            };
            var result = await ApiClient.Search<Card, CardSearchOptions>(cardSearchOptions);
            result.Items.ForEach(x=>Console.WriteLine($"Card: {x.Name}"));
        }

        private static async Task GetCardOptions()
        {
            CardOptions = await ApiClient.Get<CardOptions>();
        }

        private static async Task LogSeries()
        {
            var result = await ApiClient.GetAll<Serie>();
            result.Items.ForEach(x => Console.WriteLine($"Serie : {x.Name}"));
        }

        private static async Task Login()
        {
            var apiUrl = "http://localhost:5000/";
            var login = "";
            var password = "";
            ApiClient = new ApiClient(apiUrl, login, password);
            await ApiClient.Login();
        }
    }
}
