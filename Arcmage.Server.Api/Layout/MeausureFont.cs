using System;
using System.Drawing;
using iText.Kernel.Font;


namespace Arcmage.Server.Api.Layout
{
    public static class MeausureFont
    {

        public static Size MeasureString(string text, double fontSize, PdfFont font)
        {
            var width = font.GetWidth(text, (float)fontSize);

            float ascent = font.GetAscent(text, (float)fontSize);
            float descent = font.GetDescent(text, (float)fontSize);

            var height = ascent - descent;
            return new Size((int)Math.Round(width), (int)Math.Round(height));
        }
    }
}
