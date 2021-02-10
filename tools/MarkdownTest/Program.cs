using System;
using System.Linq;
using System.Xml.Linq;
using Arcmage.Layout.InputConvertor;

namespace MarkdownTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MarkdownRoundTripTest();
            XmlRoundTripTest();
        }

        public static void MarkdownRoundTripTest()
        {
            Console.WriteLine("Converting markdown to xml");
            Console.WriteLine();
            Console.WriteLine();
        
            var markdown = "***Fatigue - ****I come into play marked.*\\" + "\r\n" +
                           "\r\n" + 
                           ":W:hen ***I*** come into play, put two 1/1 ***Addax\\" + "\r\n" +
                           "tokens*** in target ***city*** or\\" + "\r\n" +
                           "***army*** marked. When ***I*** leave play,\\" + "\r\n" +
                           "sacrifice an ***Addax*** creature.";

            Console.WriteLine(markdown);
            Console.WriteLine();
            Console.WriteLine();

            var xml = LayoutInputConvertor.ToXml(markdown, true);
            Console.WriteLine(xml);

            Console.WriteLine();
            Console.WriteLine();

            var roundtrip = LayoutInputConvertor.ToMarkdown(xml, false);
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

            var xml = @"<p><bi>Fatigue - </bi><i>I come into play marked.</i><br/></p>
<p><c>W</c><n>hen </n><bi>I</bi><n> come into play, put two  1/1 </n><bi>Addax</bi><br/><bi>tokens</bi><n> in target </n><bi>city</bi><n> or</n><br/><bi>army</bi><n> marked. When </n><bi>I</bi><n> leave play,</n><br/><n>sacrifice an </n><bi>Addax</bi><n> creature.</n></p>";

            var document = XDocument.Parse($"<layout>{xml}</layout>", LoadOptions.PreserveWhitespace);
            var paragraphs = document.Root.Elements().ToList().Select(x => x.ToString());
            xml = string.Join(Environment.NewLine, paragraphs);

            Console.WriteLine(xml);
            Console.WriteLine();
            Console.WriteLine();

            var markdown = LayoutInputConvertor.ToMarkdown(xml, false);
            Console.WriteLine(markdown);
            Console.WriteLine();
            Console.WriteLine();

            var roundtrip = LayoutInputConvertor.ToXml(markdown, true);
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
