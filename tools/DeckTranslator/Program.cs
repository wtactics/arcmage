using System.Threading.Tasks;

namespace DeckTranslator
{
    /// <summary>
    /// Copies cards for a given deck that will be translated, some basic data is translated, artwork is uploaded.
    ///  - only french is supported
    ///  - the new cards are created with a name extension (french wip)
    ///  - only the base info is translated, rule text, flavour, layout, ... must be done manually
    ///  - optionally, the translated deck counterpart is also created
    /// </summary>
    class Program
    {

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                // change api url, login, password and temp folder (to download artwork, and upload from there)
                var apiUrl = "https://localhost:5000";
                var login = "";
                var password = "";
                var tempFolder = @"c:\temp";

                // deck id
                var deckGuid = "2e852216-450b-4b2f-add3-e5126197e149";
                var language = "french";

                var deckTranslator = new DeckTranslator(apiUrl, login, password, tempFolder);

                await deckTranslator.TranslateDeck(deckGuid, language, false);


            }).GetAwaiter().GetResult();

           
        }

       

    }
}
