using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Arcmage.DAL;
using Arcmage.DAL.Utils;
using Arcmage.Model;
using Arcmage.Server.Api.Utils;
using ImageMagick;
using iText.Kernel.Pdf;
using Newtonsoft.Json;

namespace Arcmage.Server.Api.Layout
{
    public static class DeckGenerator
    {

     

        public static void GenerateDeck(Guid deckGuid, bool generatePdf, bool exportTiles, bool generateMissingCards, bool singleDoc = true)
        {
            var jsonFile = Repository.GetDeckJsonFile(deckGuid);
            if (File.Exists(jsonFile))
            {
                var deck = JsonConvert.DeserializeObject<Deck>(File.ReadAllText(jsonFile));
                if (generatePdf)
                {
                    try
                    {
                        GenerateDeckZip(deckGuid, generateMissingCards, singleDoc, deck);
                    }
                    catch
                    {
                        // N.G. Remark: something went wrong, we're not trying to recover here.
                        //              and will just mark the job as finished.
                    }
                }
                if (exportTiles)
                {
                    try
                    {
                        GenerateDeckTiles(deckGuid, generateMissingCards, singleDoc, deck);
                    }
                    catch
                    {
                        // N.G. Remark: something went wrong, we're not trying to recover here.
                        //              and will just mark the job as finished.
                    }
                }
            }
            using (var repository = new Repository())
            {
                var deckModel = repository.Context.Decks.FindByGuid(deckGuid);
                deckModel.PdfZipCreationJobId = null;
                repository.Context.SaveChanges();
            }
        }

        private static void GenerateDeckTiles(Guid deckGuid, bool generateMissingCards, bool singleDoc, Deck deck)
        {
          
            var tilesWidth = 723;
            var tilesHeight = 1024;

            var numberOfCards = deck.DeckCards.Sum(x => x.Quantity) ;
            var numberOfColumns = 6;
            var number0fRows = 5;
            
            var totalTilesWidth = tilesWidth * numberOfColumns;
            var totalTilesHeight = tilesHeight * number0fRows;
            var numberOfCardsPerTile = numberOfColumns * number0fRows - 1; // Last cards is back
            var numberOfTiles = (int) Math.Ceiling((double) numberOfCards / numberOfCardsPerTile);

            var totalCards = 0;
            var cardIndex = 0;
            var tileIndex = 1;

            using (MagickImage backSide = new MagickImage(Repository.GetBackPngFile()))
            {
                
                backSide.SetProfile(ColorProfile.SRGB);
                backSide.SetProfile(ColorProfile.USWebCoatedSWOP);
                backSide.Scale(tilesWidth, tilesHeight);

                var backSideX = (numberOfColumns - 1) * tilesWidth;
                var backSideY = (number0fRows - 1) * tilesHeight;

                var tileImage = new MagickImage(MagickColors.Black, totalTilesWidth, totalTilesHeight);
                tileImage.SetProfile(ColorProfile.SRGB);
                tileImage.SetProfile(ColorProfile.USWebCoatedSWOP);
                tileImage.Composite(backSide, backSideX, backSideY);

                foreach (var deckCard in deck.DeckCards)
                {
                    using (MagickImage card = new MagickImage(Repository.GetPngFile(deckCard.Card.Guid)))
                    {
                        card.SetProfile(ColorProfile.SRGB);
                        card.SetProfile(ColorProfile.USWebCoatedSWOP);
                        card.Scale(tilesWidth, tilesHeight);

                        for (int i = 0; i < deckCard.Quantity; i++)
                        {
                            var row = cardIndex / numberOfColumns;
                            var column = cardIndex % numberOfColumns;

                            var x = column * tilesWidth;
                            var y = row * tilesHeight;

                            tileImage.Composite(card, x, y);

                            cardIndex++;
                            totalCards++;

                            if (cardIndex >= numberOfCardsPerTile || totalCards >= numberOfCards)
                            {

                                var tileFile = Repository.GetDeckTilesFile(deckGuid, tileIndex);
                                if (File.Exists(tileFile))
                                {
                                    File.Delete(tileFile);
                                }

                                // Save the result
                                tileImage.Write(tileFile);
                                tileImage.Dispose();
                            }

                            if (cardIndex >= numberOfCardsPerTile && totalCards < numberOfCards)
                            {
                                cardIndex = 0;
                                tileIndex++;
                            
                                tileImage = new MagickImage(MagickColors.Black, totalTilesWidth, totalTilesHeight);
                                tileImage.SetProfile(ColorProfile.SRGB);
                                tileImage.SetProfile(ColorProfile.USWebCoatedSWOP);
                                tileImage.Composite(backSide, backSideX, backSideY);
                            }
                        }
                    }
                }
            }

        }

        

        private static void GenerateDeckZip(Guid deckGuid, bool generateMissingCards, bool singleDoc, Deck deck)
        {
            using (FileStream zipToOpen = new FileStream(Repository.GetDeckZipFile(deckGuid), FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                {
                    var licfiles = Directory.GetFiles(Repository.LicPath);
                    foreach (var licfile in licfiles)
                    {
                        var licName = Path.GetFileName(licfile);
                        archive.CreateEntryFromFile(licfile, licName);
                    }

                    foreach (var deckCard in deck.DeckCards)
                    {
                        var cardPdf = Repository.GetPdfFile(deckCard.Card.Guid);
                        if (!File.Exists(cardPdf))
                        {
                            if (generateMissingCards)
                            {
                                CardGenerator.CreatePngJob(deckCard.Card.Guid, deckCard.Card.Faction.Name,
                                    deckCard.Card.Type.Name);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (!singleDoc)
                        {
                            var entryName = SanitizeName(deckCard.Card.Name);

                            for (var i = 0; i < deckCard.Quantity; i++)
                            {
                                archive.CreateEntryFromFile(cardPdf, $"{entryName}_{i + 1}.pdf");
                            }
                        }
                    }

                    var cardBackPdfFile = Repository.GetBackPdfFile();
                    if (!File.Exists(cardBackPdfFile))
                    {
                        if (generateMissingCards)
                        {
                            CardGenerator.CreateBackPdfJob();
                        }
                    }

                    if (!singleDoc)
                    {
                        if (File.Exists(cardBackPdfFile))
                        {
                            archive.CreateEntryFromFile(cardBackPdfFile, $"back.pdf");
                        }
                    }

                    var deckFormatFile = Repository.GetDeckFormatFile(deckGuid);
                    if (File.Exists(deckFormatFile))
                    {
                        archive.CreateEntryFromFile(deckFormatFile, $"deck.txt");
                    }

                    if (singleDoc)
                    {
                        var deckpdf = Repository.GetDeckFile(deck.Guid);
                        using (FileStream stream = new FileStream(deckpdf, FileMode.Create))
                        {
                            using (PdfDocument pdf = new PdfDocument(new PdfWriter(stream)))
                            {
                                var documentInfo = pdf.GetDocumentInfo();
                                documentInfo.SetAuthor($"{deck.Creator.Name}");
                                documentInfo.SetCreator($"{deck.Creator.Name} - arcmage.org");
                                documentInfo.SetTitle($"{deck.Name} - arcmage.org");
                            
                         
                                foreach (var deckCard in deck.DeckCards)
                                {
                                    var cardPdf = Repository.GetPdfFile(deckCard.Card.Guid);
                                    using (var cardDocument = new PdfDocument(new PdfReader(cardPdf)))
                                    {
                                        for (var i = 0; i < deckCard.Quantity; i++)
                                        {
                                            cardDocument.CopyPagesTo(1, 1, pdf);
                                        }
                                    }
                                }

                                using (var backDocument = new PdfDocument(new PdfReader(cardBackPdfFile)))
                                {
                                    backDocument.CopyPagesTo(1, 1, pdf);
                                }
                            }

                            archive.CreateEntryFromFile(deckpdf, $"deck.pdf");
                        }
                    }
                }
            }
        }

        private static readonly char [] Invalids = System.IO.Path.GetInvalidFileNameChars();

        private static string SanitizeName(string name)
        {
            return string.Join("_", name.Split(Invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }


    }


}
