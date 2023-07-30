using System.Collections.Generic;
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
                // local temp folder to download and then upload the artwork
                var tempFolder = @"c:\temp";
                

                var deckTranslator = new DeckTranslator(apiUrl, login, password, tempFolder);


                var language = "eo";
                var toTranslateDecks = new List<string>()
                {
                    // Set 1 - Brothers in Arms
                    "6776ddb8-3ce0-470b-8d2c-afb26bd29359",
                    // Set 1 - Shadow League
                    "25268444-33ad-42f8-8452-2089659bc91d",
                    // Set 1 - The Uprising
                    "ade9be8e-a414-4da9-a97a-e843a40aa2af",
                    // Set 1 - Toll of Time
                    "0aa069a0-5e59-40dc-a443-692a89e151a0",
                    // Set 1 - Uneasy Alliance
                    "ed84714a-0570-4384-a303-f2ded1400bdd"
                };

                foreach (var translateDeck in toTranslateDecks)
                {
                    // In dryRun mode, we'll not actually create a translated deck nor translated cards,
                    // but just list those who would be created. Change to false to create the cards.
                    await deckTranslator.TranslateDeck(translateDeck, language, dryRun:true);
                    await Task.Delay(60 * 1000);
                }

                // Example resume translate 
                //   - It will not create a new deck, but check if the targetDeck exists
                //     and continue with not yet translated cards
                //   - Cards that are already translated to the given language are not recreated
                //   - Cards that are not yet in the target deck will be added (if need be)
                //
                // var deckGuid = "2e852216-450b-4b2f-add3-e5126197e149";
                // var targetDeckGuid = "89084c1b-f6fc-4c8b-abb8-913e9a3815a7";
                // await deckTranslator.ResumeTranslateDeck(deckGuid, targetDeckGuid, language, dryRun:true);


            }).GetAwaiter().GetResult();

           
        }

       

    }
}
