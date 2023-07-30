namespace Arcmage.Model
{
    public class DeckSearchOptions : SearchOptionsBase
    {
        public bool? ExportTiles { get; set; }

        public Status Status { get; set; }

        public Language Language { get; set; }

        public bool? MyDecks { get; set; }

        public bool? ExcludeDrafts { get; set; }
    }
}
