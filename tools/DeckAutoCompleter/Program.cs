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



                var deckGuid = "69639e47-3971-43f3-b429-cd96d58268d9";

                var autoCompleter = new AutoCompleter(apiUrl, login, password);

                // Dry run is enabled by default. In dry run mode the auto completer will only show the results
                // for the card's layout and flavour text without applying them.
                await autoCompleter.AutoComplete(deckGuid, true);


            }).GetAwaiter().GetResult();
            Console.ReadKey();
        }
    }
}
