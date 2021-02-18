using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Arcmage.Layout.InputConvertor.XmlRender
{
    public static class XmlRender
    {
        public static string ToMarkdown(string xmlLayout)
        {
            var document = XDocument.Parse(xmlLayout, LoadOptions.PreserveWhitespace);
            var paragraphs = document.Root.Elements("p").Select(ParseParagraph).ToList();
            // Separate paragraphs with blank lines, using window's style line endings.
            return string.Join("\r\n\r\n", paragraphs); ;
        }

        private static string ParseParagraph(XElement paragraph)
        {
            // start of the paragraph, reset
            var paragraphLine = string.Empty;

            foreach (var xElement in paragraph.Elements())
            {
                var tagName = xElement.Name.LocalName;
                switch (tagName)
                {
                    // Capital letter, should only occur as the first item of a paragraph
                    case "c":
                        var capitalLetter = TextValue(xElement).ToUpper().First();
                        paragraphLine += $":{capitalLetter}:";
                        break;
                    // Large symbol, should only occur as the first item of a paragraph
                    case "m":
                    case "m0":
                    case "m1":
                    case "m2":
                    case "m3":
                    case "m4":
                    case "m5":
                    case "m6":
                    case "m7":
                    case "m8":
                    case "m9":
                    case "ma":
                    case "mt":
                    case "mx":
                    case "mc":
                    case "mi":
                    case "mic":
                    case "mis":
                    case "g0":
                    case "g1":
                    case "g2":
                    case "g3":
                    case "g4":
                    case "g5":
                    case "g6":
                    case "g7":
                    case "g8":
                    case "g9":
                    case "gt":
                    case "gx":
                        paragraphLine += $":{tagName}:";
                        break;
                    // normal text
                    case "n":
                        paragraphLine += TextValue(xElement);
                        break;
                    case "b":
                        paragraphLine += $"**{TextValue(xElement)}**";
                        break;
                    case "i":
                        paragraphLine += $"*{TextValue(xElement)}*";
                        break;
                    case "bi":
                        paragraphLine += $"***{TextValue(xElement)}***";
                        break;
                    case "br":
                        paragraphLine += $":{tagName}:";
                        break;
                }
            }
            paragraphLine = InlineBreaks(paragraphLine);
          
            // Support empty paragraph
            if (string.IsNullOrEmpty(paragraphLine))
            {
                paragraphLine = "\\";
            }

            return paragraphLine;
        }

        public static string TextValue(XElement xe)
        {
            var text = xe.Nodes().OfType<XText>().Aggregate(new StringBuilder(), (s, c) => s.Append(c), s => s.ToString());
            return text;
        }

       private static string InlineBreaks(string paragraphLine)
       { 
           // Support breaks within bold, italic of bold-italic 
           Regex.Replace(paragraphLine, @"^(\*{1,3}):br:\1$", ":br:");
           // Translate break :br: syntax to backslash + enter syntax 
           return paragraphLine.Replace(":br:", "\\\r\n");
       }
    }
}
