using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Arcmage.DAL;
using Arcmage.DAL.Model;
using Arcmage.Layout.InputConvertor;

namespace MarkdownTest
{
    class Program
    {
        static void Main(string[] args)
        {

            //PrettyPrintLayoutXml();
            //ToMarkdownLayout();
            //CheckMarkdownRoundTrip();

            MarkdownRoundTripTest();
            //XmlRoundTripTest();
        }

        private static void CheckMarkdownRoundTrip()
        {
            var dir = @"c:\temp\markdowntest";
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir,true);
            }

            Directory.CreateDirectory(dir);

            using (var repository = new Repository())
            {
                var validCounter = 0;
                var models = repository.Context.Set<CardModel>().ToList();
                foreach (var cardModel in models)
                {
                    if (cardModel.CardId == 25)
                    {

                    }
                    var layoutXml = LayoutInputConvertor.ToXml(cardModel.MarkdownText);
                    if (string.Equals(layoutXml, cardModel.LayoutXml))
                    {
                        Console.WriteLine($" Card {cardModel.CardId} : valid");
                        validCounter++;
                    }
                    else
                    {
                        Console.WriteLine($" Card {cardModel.CardId} : invalid");
                        File.WriteAllText(Path.Combine(dir, $"{cardModel.CardId}_original.xml"), cardModel.LayoutXml);
                        File.WriteAllText(Path.Combine(dir, $"{cardModel.CardId}.md"), cardModel.MarkdownText);
                        File.WriteAllText(Path.Combine(dir, $"{cardModel.CardId}_round.xml"), layoutXml);
                    }
                }
                Console.WriteLine($" Valid cards: {validCounter} / {models.Count}");
            }
        }

        private static void ToMarkdownLayout()
        {
            using (var repository = new Repository())
            {
                var models = repository.Context.Set<CardModel>().ToList();
                foreach (var cardModel in models)
                {
                    if (cardModel.CardId == 25)
                    {

                    }
                    cardModel.MarkdownText = LayoutInputConvertor.ToMarkdown(cardModel.LayoutXml);
                }

                repository.Context.SaveChanges();
            }
        }

        private static void PrettyPrintLayoutXml()
        {
            using (var repository = new Repository())
            {
                var models = repository.Context.Set<CardModel>().ToList();
                foreach (var cardModel in models)
                {
                    var xmlLayout = $"<layout>{cardModel.LayoutText}</layout>";
                    var document = XDocument.Parse(xmlLayout);
                    RemoveEmptyElementsByName(document, "n");
                    RemoveEmptyElementsByName(document, "i");
                    RemoveEmptyElementsByName(document, "b");
                    RemoveEmptyElementsByName(document, "bi");

                    cardModel.LayoutXml = document.ToString();
                }

                repository.Context.SaveChanges();
            }
        }

        private static void RemoveEmptyElementsByName(XDocument document, string tagName)
        {
            document.Descendants(tagName)
                .Where(a => a.IsEmpty || a.Value == string.Empty)
                .Remove();
        }

        public static void MarkdownRoundTripTest()
        {
            Console.WriteLine("Converting markdown to xml");
            Console.WriteLine();
            Console.WriteLine();

            var markdown = "\\" + "\r\n" +
                           "\r\n" +
                           ":C: herche n'importe quel ***cimetière*** et met la créature de ton choix dans ***ton armée.*** Cette créature acquiert la capacité\\ ***Mort-vivant.***";


            var markdown2 = "***Fatigue - ****I come into play marked.*\\" + "\r\n" +
                           "\r\n" + 
                           ":W:hen ***I*** come into play, put two 1/1 ***Addax\\" + "\r\n" +
                           "tokens*** in target ***city*** or\\" + "\r\n" +
                           "***army*** marked. When ***I*** leave play,\\" + "\r\n" +
                           "sacrifice an ***Addax*** creature.";

            Console.WriteLine(markdown);
            Console.WriteLine();
            Console.WriteLine();

            var xml = LayoutInputConvertor.ToXml(markdown);
            Console.WriteLine(xml);

            Console.WriteLine();
            Console.WriteLine();

            var roundtrip = LayoutInputConvertor.ToMarkdown(xml);
            Console.WriteLine(roundtrip);
            Console.WriteLine();
            Console.WriteLine();

            if (string.Equals(roundtrip, markdown))
            {
                Console.WriteLine("markdown -> xml -> markdown : valid");
            }

        }

        public static void XmlRoundTripTest()
        {
            Console.WriteLine("Converting to xml to markdown");
            Console.WriteLine();
            Console.WriteLine();

            var xml = @"<layout><p><bi>Fatigue - </bi><i>I come into play marked.</i><br/></p>
<p><c>W</c><n>hen </n><bi>I</bi><n> come into play, put two  1/1 </n><bi>Addax</bi><br/><bi>tokens</bi><n> in target </n><bi>city</bi><n> or</n><br/><bi>army</bi><n> marked. When </n><bi>I</bi><n> leave play,</n><br/><n>sacrifice an </n><bi>Addax</bi><n> creature.</n></p></layout>";

            var document = XDocument.Parse($"<layout>{xml}</layout>", LoadOptions.PreserveWhitespace);
            var paragraphs = document.Root.Elements().ToList().Select(x => x.ToString());
            xml = string.Join(Environment.NewLine, paragraphs);

            Console.WriteLine(xml);
            Console.WriteLine();
            Console.WriteLine();

            var markdown = LayoutInputConvertor.ToMarkdown(xml);
            Console.WriteLine(markdown);
            Console.WriteLine();
            Console.WriteLine();

            var roundtrip = LayoutInputConvertor.ToXml(markdown);
            Console.WriteLine(roundtrip);
            Console.WriteLine();
            Console.WriteLine();
            if (string.Equals(roundtrip, xml))
            {
                Console.WriteLine("xml -> markdown -> xml : valid");
            }

        }
    }
}
