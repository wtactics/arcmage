using System;
using System.Linq;
using System.Xml.Linq;

namespace Arcmage.Layout.InputConvertor.XmlRender
{
    public static class XmlRender
    {
        public static string ToMarkdown(string xmlLayout, bool hasXmlRoot = true)
        {
            var result = string.Empty;
            if (!hasXmlRoot) xmlLayout = $"<layout>{xmlLayout}</layout>";
            var document = XDocument.Parse(xmlLayout);
            var paragraphs = document.Root.Elements("p").ToList();
            var counter = 0;
            foreach (var xElement in paragraphs)
            {
                result += ParseParagraph(xElement);
                if (counter != paragraphs.Count - 1)
                {
                    result += Environment.NewLine + Environment.NewLine;
                }
                counter++;
            }

            return result;
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
                        var capitalLetter = xElement.Value.ToUpper();
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
                        paragraphLine += xElement.Value;
                        break;
                    case "b":
                        paragraphLine += $"**{xElement.Value}**";
                        break;
                    case "i":
                        paragraphLine += $"*{xElement.Value}*";
                        break;
                    case "bi":
                        paragraphLine += $"***{xElement.Value}***";
                        break;
                    case "br":
                        paragraphLine += $":{tagName}:";
                        break;
                }
            }

            return InlineBreaks(paragraphLine);
        }

        private static string InlineBreaks(string paragraphLine)
        {
            paragraphLine = paragraphLine.Replace("***:br:***", ":br:");
            paragraphLine = paragraphLine.Replace("**:br:**", ":br:");
            paragraphLine = paragraphLine.Replace("*:br:*", ":br:");
            return paragraphLine;
        }

    }
}
