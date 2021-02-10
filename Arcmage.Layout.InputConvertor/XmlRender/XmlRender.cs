using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Arcmage.Layout.InputConvertor.XmlRender
{
    public static class XmlRender
    {
        public static string ToMarkdown(string xmlLayout, bool hasXmlRoot = true)
        {
            if (!hasXmlRoot) xmlLayout = $"<layout>{xmlLayout}</layout>";
            var document = XDocument.Parse(xmlLayout, LoadOptions.PreserveWhitespace);
            var paragraphs = document.Root.Elements("p").Select(ParseParagraph).ToList();
            return string.Join("\n\n", paragraphs).Replace("\n", "\r\n");
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
                    // Capital letter, should only occurr as the first item of a paragraph
                    case "c":
                        var capitalLetter = ShallowValue(xElement).ToUpper().First();
                        paragraphLine += $":{capitalLetter}:";
                        break;
                    // Larrge symbol, should only occurr as the first item of a paragraph
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
                        paragraphLine += ShallowValue(xElement);
                        break;
                    case "b":
                        paragraphLine += $"**{ShallowValue(xElement)}**";
                        break;
                    case "i":
                        paragraphLine += $"*{ShallowValue(xElement)}*";
                        break;
                    case "bi":
                        paragraphLine += $"***{ShallowValue(xElement)}***";
                        break;
                    case "br":
                        paragraphLine += $":{tagName}:";
                        break;
                }
            }
            paragraphLine = InlineBreaks(paragraphLine);
            return RemoveNullChars(paragraphLine);
        }

        public static string ShallowValue(XElement xe)
        {

            
            return xe
                .Nodes()
                .OfType<XText>()
                .Aggregate(new StringBuilder(),
                    (s, c) => s.Append(c),
                    s => s.ToString());
        }

       private static string InlineBreaks(string paragraphLine)
       {
           paragraphLine = paragraphLine.Replace("***:br:***", ":br:");
           paragraphLine = paragraphLine.Replace("**:br:**", ":br:");
           paragraphLine = paragraphLine.Replace("*:br:*", ":br:");
           paragraphLine = paragraphLine.Replace(":br:", "\\\n");
           paragraphLine = paragraphLine.TrimEnd('\r', '\n');

           return paragraphLine;
       }

       private static string RemoveNullChars(string text)
       {
           return text.Replace("\0", string.Empty);
       }

    }
}
