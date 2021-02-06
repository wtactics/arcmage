using iText.Forms.Xfdf;
using iText.Kernel.Font;

namespace Arcmage.Server.Api.Layout
{
    public static class Styles
    {
    
        public static float CapitalFontSize = 29.7f;

        public static float SymbolLargeFontSize = 35;
        public static float SymbolFontSize = 16;
        public static float FontSize = 11.25f;

        public static float LineSpacing = 1.25f;
        public static float ParagraphSkip = FontSize / 2.0f;

        public static PdfFont CapitalFont = PdfFontFactory.CreateRegisteredFont("nimbusromno9l");
        public static string CapitalFontStyle ="-inkscape-font-specification:NimbusRomNo9L;font-family:NimbusRomNo9L;font-weight:normal;font-style:normal;font-stretch:normal;font-variant:normal;font-size:";

     
        public static PdfFont SymbolFont = PdfFontFactory.CreateRegisteredFont("tactics symbols\0");
        public static string SymbolFontStyle = "-inkscape-font-specification:WTactics Symbols;font-family:WTactics Symbols;font-weight:normal;font-style:normal;font-stretch:normal;font-variant:normal;font-size:";

      

        public static PdfFont NormalFont = PdfFontFactory.CreateRegisteredFont("liberationserif");
        public static string NormalFontStyle = "-inkscape-font-specification:Liberation Serif;font-family:Liberation Serif;font-weight:normal;font-style:normal;font-stretch:normal;font-variant:normal;font-size:";

        public static PdfFont BoldFont = PdfFontFactory.CreateRegisteredFont("liberationserif-bold");
        public static string BoldFontStyle = "-inkscape-font-specification:Liberation Serif Boldfont-family:Liberation Serif;font-weight:bold;font-style:normal;font-stretch:normal;font-variant:normal;font-size:";

        public static PdfFont ItalicFont = PdfFontFactory.CreateRegisteredFont("liberationserif-italic");
        public static string ItalicFontStyle = "-inkscape-font-specification:Liberation Serif Italic;font-family:Liberation Serif;font-weight:normal;font-style:italic;font-stretch:normal;font-variant:normal;font-size:";

        public static PdfFont BoldItalicFont = PdfFontFactory.CreateRegisteredFont("liberationserif-bolditalic");
        public static string BoldItalicFontStyle = "-inkscape-font-specification:Liberation Serif Bold Italic;font-family:Liberation Serif;font-weight:bold;font-style:italic;font-stretch:normal;font-variant:normal;font-size:";

    }
}