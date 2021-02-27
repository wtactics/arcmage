using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Arcmage.DAL;
using Arcmage.DAL.Utils;
using Arcmage.Layout.InputConvertor;
using Arcmage.Model;
using ImageMagick;
using iText.Kernel.Font;
using Newtonsoft.Json;

namespace Arcmage.Server.Api.Layout
{
    public class CardGenerator
    {
        private static XNamespace NS = "http://www.w3.org/2000/svg";

        private static XNamespace InkscapeNS = "http://www.inkscape.org/namespaces/inkscape";
        private static XNamespace XLinkNS = "http://www.w3.org/1999/xlink";

        private Card Card { get; set; }
        private TemplateInfo Template { get; set; }

        public CardGenerator(Card card)
        {
            Card = card;
            Template = card.Type.TemplateInfo;
        }

        public static void CreatePngJob(Guid cardGuid, string faction, string type)
        {
            var svgFile = Repository.GetSvgFile(cardGuid);
            InkscapeExporter.ExportPng(svgFile, Repository.GetPngFile(cardGuid));

            using (MagickImage image = new MagickImage(Repository.GetPngFile(cardGuid)))
            {
                image.Scale(320, 454);
                // You're done. Save it.
                image.Write(Repository.GetJpegFile(cardGuid));
            }

            var border = Repository.GetPrintBorderFile(faction, type, "png");


            using (MagickImage image = new MagickImage(border))
            {

                image.SetProfile(ColorProfile.SRGB);
                image.SetProfile(ColorProfile.USWebCoatedSWOP);

                // Read the watermark that will be put on top of the image
                using (MagickImage card = new MagickImage(Repository.GetPngFile(cardGuid)))
                {
                    card.SetProfile(ColorProfile.SRGB);
                    card.SetProfile(ColorProfile.USWebCoatedSWOP);
                    card.Scale(1465, 2079);

                    image.Composite(card, Gravity.Center, CompositeOperator.Over);
                }
                // Save the result
                image.Write(Repository.GetPdfFile(cardGuid));
            }

            using (var repository = new Repository())
            {
                var cardModel = repository.Context.Cards.FindByGuid(cardGuid);
                cardModel.PngCreationJobId = null;
                repository.Context.SaveChanges();
            }
        }

        public static void CreateBackPdfJob()
        {
            var border = Repository.GetBackBorderPngFile();
            using (MagickImage image = new MagickImage(border))
            {

                image.SetProfile(ColorProfile.SRGB);
                image.SetProfile(ColorProfile.USWebCoatedSWOP);
              
                // Save the result
                image.Write(Repository.GetBackPdfFile());
            }
        }

        private XElement Overlay { get; set; }

        public Task Generate(bool overlay = true)
        {
            return Task.Run(() =>
            {
                Repository.InitPaths();
                PdfFontFactory.RegisterDirectory(Repository.FontPath);
                var registeredFonts = PdfFontFactory.GetRegisteredFonts();
                var registeredFamilies = PdfFontFactory.GetRegisteredFamilies();

                var templateFile = overlay? Repository.GetOverlayTemplateFile(Card.Faction.Name, Card.Type.Name) :
                    Repository.GetTemplateFile(Card.Faction.Name, Card.Type.Name);
                
                if (!File.Exists(templateFile)) throw new Exception("Template could not be found");

                XDocument svg;
                try
                {
                    // load svg for template
                    svg = XDocument.Load(templateFile);
                    // find the overlay root
                    Overlay = svg.Root.Elements(NS + "g").Single(x => x.Attribute("id").Value == "overlaylayer");
                }
                catch (Exception e)
                {
                    throw new Exception("Template could not be loaded", e);
                }

                // apply card information to svg template
                if (Template.ShowName) SetName();
                if (Template.ShowType) SetType();
                if (Template.ShowGoldCost) SetGoldCost();
                if (Template.ShowLoyalty) SetLoyalty();
                if (Template.ShowText) SetText();
                if (Template.ShowAttack) SetAttack();
                if (Template.ShowDefense) SetDefense();
                if (Template.ShowDiscipline) SetDiscipline();
                if (Template.ShowArt) SetArt();
                if (Template.ShowInfo) SetInfo();

                // write svg
                var svgFile = overlay? Repository.GetOverlaySvgFile(Card.Guid) : Repository.GetSvgFile(Card.Guid);

                File.WriteAllText(svgFile, svg.ToString());

                // write json
                string cardString = JsonConvert.SerializeObject(Card, Formatting.Indented);
                File.WriteAllText(Repository.GetJsonFile(Card.Guid), cardString);

                if (!overlay)
                {
                    var merge = XDocument.Parse("<svg version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" height=\"340.15747\" width=\"244.48819\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"></svg>");
                    var border = XDocument.Load(Repository.GetPrintBorderFile(Card.Faction.Name, Card.Type.Name, "svg"));
                    border.Root.SetAttributeValue("height", "340.15747");
                    border.Root.SetAttributeValue("width", "244.48819");
                    border.Root.SetAttributeValue("y", "0");
                    border.Root.SetAttributeValue("x", "0");
                    merge.Root.Add(border.Root);

                    svg.Root.SetAttributeValue("height", "325.98425");
                    svg.Root.SetAttributeValue("width", "230.31496");
                    svg.Root.SetAttributeValue("y", "7.0866184");
                    svg.Root.SetAttributeValue("x", "7.0866184");
                    merge.Root.Add(svg.Root);
                 
                    
                    File.WriteAllText(Repository.GetPrintSvgFile(Card.Guid), merge.ToString());
                }
              
            });
            
        }

       

        private void SetName()
        {
            if (string.IsNullOrEmpty(Card.LastName) && string.IsNullOrWhiteSpace(Card.FirstName))
            {
                var name = FindById("nametext");
                name?.SetValue(Card.Name ?? string.Empty);
            }
            else
            {
                var firstName = FindById("firstnametext");
                firstName?.SetValue(Card.FirstName ?? string.Empty);
                var lastName = FindById("lastnametext");
                lastName?.SetValue(Card.LastName ?? string.Empty);

                if (firstName == null)
                {
                    var name = FindById("nametext");
                    name?.SetValue(Card.FirstName ?? string.Empty);
                }

            }
        }

        private void SetType()
        {
            var cardType = FindById("cardtypetext");
            cardType?.SetValue(Card.SubType ?? string.Empty);
        }

        private void SetGoldCost()
        {
            var goldcost = FindById("goldcosttext");
            goldcost?.SetValue(Card.Cost ?? string.Empty);
        }

        private void SetLoyalty()
        {
            if (Card.Loyalty > 0)
            {
                var loyalty1 = FindById("L1");
                loyalty1?.SetAttributeValue("style", "display:inline");
            }
            if (Card.Loyalty > 1)
            {
                var loyalty2 = FindById("L2");
                loyalty2?.SetAttributeValue("style", "display:inline");
            }
            if (Card.Loyalty > 2)
            {
                var loyalty3 = FindById("L3");
                loyalty3?.SetAttributeValue("style", "display:inline");
            }
        }

        private void SetAttack()
        {
            var attack = FindById("attacktext");
            attack?.SetValue(Card.Attack ?? string.Empty);
        }

        private void SetDefense()
        {
            var defense = FindById("defensetext");
            defense?.SetValue(Card.Defense ?? string.Empty);
        }

        private void SetDiscipline()
        {
           
        }

        private void SetArt()
        {
            var artFile = Repository.GetArtFile(Card.Guid);
            if( !File.Exists(artFile)) return;
            var bytes = File.ReadAllBytes(artFile);
            var artwork = FindById("artwork");
            artwork?.SetAttributeValue(XLinkNS + "href", "data:image/png;base64," + Convert.ToBase64String(bytes));
        }

        private void SetInfo()
        {
            var info = FindById("infotext");
            info?.SetValue(Card.Info ?? string.Empty);
        }

        #region card text layout

        private XElement CardTextBox { get; set; }

      
        // should start on a new line
        private bool newline;
        // x coordinate for the next new line
        private double newLineStartX;
        // the number of lines to layout next to a capital or a mark, 
        // when it reaches zero the newLineStartX is reset to zero
        private int xcountreset;

        // the absolute x coordinate (in the textbox for a line)
        private double x;

        // the relative y coorindate (int the thexbox for a line) (relative to the normal placement)
        private double dy;

        // the relative y coorindate (int the thexbox for a line) (relative to the normal placement)
        private double dx;

        private double totalHeight;

        // sizes
        private double CapitalFontSize { get; set; }
        private double SymbolLargeFontSize { get; set; }
        private double SymbolFontSize { get; set; }
        private double FontSize { get; set; }
        private double LineSpacing { get; set; }
        private double ParagraphSkip { get; set; }



        private void SetText()
        {
            if (string.IsNullOrWhiteSpace(Card.MarkdownText)) return;
            var layoutXml = LayoutInputConvertor.ToXml(Card.MarkdownText);
            if (string.IsNullOrWhiteSpace(layoutXml)) return;
            var document = XDocument.Parse(layoutXml);

            // reduce text height scale if it doesn't fit with 5% each time
            for (var heightScale = 1.0; heightScale > 0.5; heightScale -= 0.02)
            {
                dy = 0;
                totalHeight = 0;
                CapitalFontSize = Styles.CapitalFontSize * heightScale;
                SymbolLargeFontSize = Styles.SymbolLargeFontSize * heightScale;
                SymbolFontSize = Styles.SymbolFontSize * heightScale;
                FontSize = Styles.FontSize * heightScale;
                LineSpacing = Styles.LineSpacing;
                ParagraphSkip = Styles.ParagraphSkip * heightScale;

                CardTextBox = FindById("cardtext");
                CardTextBox?.RemoveNodes();

             
                foreach (var xElement in document.Root.Elements("p"))
                {
                    ParseParagraph(xElement);
                }
                totalHeight += FontSize * LineSpacing;
                // it fits
                if (totalHeight <= Template.MaxTextBoxHeight) break;

            }

        }
        
       
        public void ParseParagraph(XElement paragraph)
        {
            // start of the paragraph, reset
            x = 0.0;
            xcountreset = 0;
            var line = string.Empty;
            newline = true;
            newLineStartX = 0.0;
            dx = 0.0;

            foreach (var xElement in paragraph.Elements())
            {
                switch (xElement.Name.LocalName)
                {
                    // Capital letter, should only occurr as the first item of a paragraph
                    case "c":
                        line = xElement.Value;
                        dy += (FontSize * LineSpacing);
                        AddTextSpan(CardTextBox, line, Styles.CapitalFontStyle, CapitalFontSize,newline,x,dy);

                        // measure the font's witdh
                        var wordsSize = MeausureFont.MeasureString(line, CapitalFontSize, Styles.CapitalFont);
                        newLineStartX = wordsSize.Width + 2;
                        // start a new line, next to the capital
                        newline = true;
                        // next to the capital
                        x = newLineStartX;
                        // relative move the line one rule up
                        dy = -(FontSize *LineSpacing);
                        // reset the newLineStart after two lines next to the capital
                        xcountreset = 2;
                        break;
                    // Larrge symbol, should only occurr as the first item of a paragraph
                    case "m":
                        line = "M";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "m0":
                        line = "0";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "m1":
                        line = "1";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "m2":
                        line = "2";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "m3":
                        line = "3";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "m4":
                        line = "4";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "m5":
                        line = "5";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "m6":
                        line = "6";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "m7":
                        line = "7";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "m8":
                        line = "8";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "m9":
                        line = "9";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "ma":
                        line = "A";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "mt":
                        line = "T";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "mx":
                        line = "X";
                        AddLargeStartSymbol(line, 5, 2);
                        break;
                    case "mc":
                        line = "M";
                        AddMarkColumnStartSymbol(line, 4, 4);
                        break;
                    case "mi":
                        line = "m";
                        AddLargeStartSymbol(line, 0, 2);
                        break;
                    case "mic":
                        line = "m";
                        AddMarkColumnStartSymbol(line, 0, 2);
                        break;
                    case "mis":
                        line = "m";
                        AddSymbol(line, 0, 5);
                        break;
                    case "g0":
                        line = "k";
                        AddSymbol(line);
                        break;
                    case "g1":
                        line = "b";
                        AddSymbol(line);
                        break;
                    case "g2":
                        line = "c";
                        AddSymbol(line);
                        break;
                    case "g3":
                        line = "d";
                        AddSymbol(line);
                        break;
                    case "g4":
                        line = "e";
                        AddSymbol(line);
                        break;
                    case "g5":
                        line = "f";
                        AddSymbol(line);
                        break;
                    case "g6":
                        line = "g";
                        AddSymbol(line);
                        break;
                    case "g7":
                        line = "h";
                        AddSymbol(line);
                        break;
                    case "g8":
                        line = "i";
                        AddSymbol(line);
                        break;
                    case "g9":
                        line = "j";
                        AddSymbol(line);
                        break;
                    case "gt":
                        line = "t";
                        AddSymbol(line);
                        break;
                    case "gx":
                        line = "x";
                        AddSymbol(line);
                        break;

                    // normal text
                    case "n":
                        line = xElement.Value;
                        LayoutText(line, Styles.NormalFont, Styles.NormalFontStyle, FontSize);
                        break;
                    // normal text
                    case "b":
                        line = xElement.Value;
                        LayoutText(line, Styles.BoldFont, Styles.BoldFontStyle, FontSize);
                        break;
                    case "i":
                        line = xElement.Value;
                        LayoutText(line, Styles.ItalicFont, Styles.ItalicFontStyle, FontSize);
                        break;
                    case "bi":
                        line = xElement.Value;
                        LayoutText(line, Styles.BoldItalicFont, Styles.BoldItalicFontStyle, FontSize);
                        break;
                    case "br":
                        NewLineReset(FontSize);
                        newline = true;
                        break;
                }
            }
            dy += FontSize + ParagraphSkip;
        }

        private void AddSymbol(string line, double xoffset = 0, double xspacing = 0)
        {
            LayoutText(line, Styles.SymbolFont, Styles.SymbolFontStyle, SymbolFontSize, xoffset, xspacing);
        }

        private void AddLargeStartSymbol(string line, double xoffset, double xspacing)
        {
            dy += (FontSize * LineSpacing);
            AddTextSpan(CardTextBox, line, Styles.SymbolFontStyle, SymbolLargeFontSize, newline, x + xoffset, dy);

            // measure the font's witdh
            var symbolSize = MeausureFont.MeasureString(line, SymbolLargeFontSize, Styles.SymbolFont);
            newLineStartX = symbolSize.Width + xspacing;
            // start a new line, next to the capital
            newline = true;
            // next to the capital
            x = newLineStartX;
            // relative move the line one rule up
            dy = -(FontSize * LineSpacing);
            // reset the newLineStart after two lines next to the symbol
            xcountreset = 2;
        }


        private void AddMarkColumnStartSymbol(string line, double xoffset, double xspacing)
        {
           // dy += (Styles.FontSize*Styles.LineSpacing);
            AddTextSpan(CardTextBox, line, Styles.SymbolFontStyle, SymbolFontSize, newline, x + xoffset, dy);

            // measure the font's witdh
            var symbolSize = MeausureFont.MeasureString(line, SymbolFontSize, Styles.SymbolFont);
            newLineStartX = symbolSize.Width + xspacing;
            // start a new line, next to the capital
            newline = true;
            // next to the capital
            x = newLineStartX;
            // reset on same line
            dy = 0;
            // reset the newLineStart after two lines next to the symbol
            xcountreset = 10;
        }

        private void LayoutText(string line, PdfFont font, string fontStyle, double fontSize, double xoffset = 0, double xspacing = 0)
        {
            // we'll keep on processing the line until al is layed out
            while (!string.IsNullOrEmpty(line))
            {
                var lineOverflow = WrapOverflow(line, x, fontSize, font);

                // first line doesn't fit on the row
                if (string.IsNullOrWhiteSpace(lineOverflow.Item1))
                {
                    dy += (fontSize *LineSpacing);
                    if (x == 0)
                    {
                        throw new Exception(string.Format("Word {0} is to long", lineOverflow.Item2));
                    }
                }
                else
                {
                    AddTextSpan(CardTextBox, lineOverflow.Item1, fontStyle, fontSize, newline, x + xoffset, dy, dx);
                    dx = xspacing;
                }

                // check if there is a seccond line
                newline = !string.IsNullOrEmpty(lineOverflow.Item2);
                if (newline)
                {
                    NewLineReset(fontSize);
                }
                else
                {
                    dy = 0;
                    x += MeausureFont.MeasureString(lineOverflow.Item1, fontSize, font).Width + xoffset;
                }
                line = lineOverflow.Item2;
            }
        }

        private void NewLineReset(double fontSize)
        {
            xcountreset--;
            if (xcountreset == 0)
            {
                newLineStartX = 0;
            }
            dy = (fontSize *LineSpacing);
            x = newLineStartX;
        }

        private Tuple<string,string> WrapOverflow(string line, double newLineStartX, double fontSize, PdfFont normalFont)
        {
            var secondLine = string.Empty;
            var firstLine = line;
            var wordsSize = MeausureFont.MeasureString(line, FontSize, Styles.NormalFont);
            while (newLineStartX + wordsSize.Width > Template.MaxTextBoxWidth)
            {
                var split = firstLine.LastIndexOf(" ");
                if (split == -1)
                {
                    // doesn't fit on the row
                    return new Tuple<string, string>(string.Empty, line);
                }
                firstLine = firstLine.Substring(0, split);
                secondLine = line.Substring(split);
                wordsSize = MeausureFont.MeasureString(firstLine, FontSize, Styles.NormalFont);
            }
            return new Tuple<string, string>(firstLine,secondLine);
        }

        public void AddTextSpan(XElement text, string line, string fontStyle, double fontSize, bool isNewLine, double x, double dy, double dx = 0)
        {
            line = ReplaceLeadingTrailingSpaces(line, isNewLine);
            var tspan = new XElement(NS + "tspan", line);
            if (isNewLine)
            {
                tspan.SetAttributeValue("x", string.Format("{0}", x.ToString("F", CultureInfo.InvariantCulture)));
                tspan.SetAttributeValue("dy", string.Format("{0}", dy.ToString("F", CultureInfo.InvariantCulture)));
                totalHeight += dy;
            }
            if (dx != 0)
            {
                tspan.SetAttributeValue("dx", string.Format("{0}", dx.ToString("F", CultureInfo.InvariantCulture)));
            }
            tspan.SetAttributeValue("style", string.Format("{0}{1}px", fontStyle, fontSize.ToString("F", CultureInfo.InvariantCulture)));
            text.Add(tspan);
        }

        public static string ReplaceLeadingTrailingSpaces(string line, bool isNewLine)
        {
            var leading = "";
            while (line.StartsWith(" "))
            {
                
                leading += "\u00A0";
                line = line.Substring(1);
            }
            var trailing = "";
            while (line.EndsWith(" "))
            {
                trailing += "\u00A0";
                line = line.Substring(0, line.Length - 1);
            }
            if (isNewLine) leading = string.Empty;
            return leading + line + trailing;
        }

        #endregion card text layout

        #region helpers

        private XElement FindById(string id)
        {
            return Overlay.Elements().SingleOrDefault(x => x.Attribute("id").Value == id);
        }
        
        #endregion helpers

    }
}
