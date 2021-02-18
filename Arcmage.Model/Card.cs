namespace Arcmage.Model
{
    public class Card: Base
    {
        public int Id { get; set; }

        // name
        public string Name { get; set; }

        // first line of name if split up
        public string FirstName { get; set; }

        // second line of name if split up
        public string LastName { get; set; }

        // artist
        public string Artist { get; set; }

        public string RuleText { get; set; }

        public string FlavorText { get; set; }

        public string SubType { get; set; }

        public CardType Type { get; set; }

        public Faction Faction { get; set; }

        public Status Status { get; set; }

        public string Cost { get; set; }

        public int Loyalty { get; set; }

        public string Attack { get; set; }

        public string Defense { get; set; }

        public string Info { get; set; }

        public Serie Serie { get; set; }

        public RuleSet RuleSet { get; set; }

        // generation input
        public string Artwork { get; set; }

        public string MarkdownText { get; set; }

        // generation results

        public string Svg { get; set; }

        public string Png { get; set; }

        public string Jpeg { get; set; }

        public string Pdf { get; set; }

        //
        
        public string BackSvg { get; set; }

        public string BackPng { get; set; }

        public string BackJpeg { get; set; }

        public string BackPdf { get; set; }


        public bool IsGenerated { get; set; }

        public string BackgroundPng { get; set; }

        public string OverlaySvg { get; set; }

        public Language Language { get; set; }

    }
}
