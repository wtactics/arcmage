using System.ComponentModel.DataAnnotations;

namespace Arcmage.DAL.Model
{
    public class CardModel : ModelBase
    {
        public int CardId { get; set; }
        
        // name
        public string Name { get; set; }

        // first line of name if split up
        public string FirstName { get; set; }

        // second line of name if split up
        public string LastName { get; set; }
        
        // artist
        public string Artist { get; set; }

        public string ArtworkLicensor { get; set; }

        public LicenseModel ArtworkLicense { get; set; }
       
        public string RuleText { get; set; }

        public string FlavorText { get; set; }

        public string SubType { get; set; }

        public CardTypeModel Type { get; set; }
        
        public FactionModel Faction { get; set; }

        public string Cost { get; set; }

        public int Loyalty { get; set; }

        public string Attack { get; set; }

        public string Defense { get; set; }
        
        public string Info { get; set; }
        
        public SerieModel Serie { get; set; }

        public RuleSetModel RuleSet { get; set; }

        public StatusModel Status { get; set; }

        // generation input in rootless xml format
        public string LayoutText { get; set; }

        // generation input in markdown format
        public string MarkdownText { get; set; }

        // generation input in pretty print rooted xml
        public string LayoutXml { get; set; }

        public string PngCreationJobId { get; set; }

        public string LanguageCode { get; set; }

        public CardModel MasterCard { get; set; }

        public int? MasterCardId { get; set; }

    }
}
