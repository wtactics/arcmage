using System;
using System.Threading.Tasks;

namespace DeckAutoCompleter
{
    /// <summary>
    /// Tries to update the rule text and flavor text fields of the cards based on the layout text. (of a given deck)
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                // change api url, login, password
                var apiUrl = "https://localhost:5000";
                var login = "";
                var password = "";
                var deckGuid = "e7da7224-a734-4bc2-b6a7-5a9727fd5bf1";

                var autoCompleter = new AutoCompleter(apiUrl, login, password);

                await autoCompleter.AutoComplete(deckGuid);


            }).GetAwaiter().GetResult();
            Console.ReadKey();
        }
    }
}
