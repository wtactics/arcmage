namespace Arcmage.Model
{
    public class SearchOptionsBase
    {
        public string Search { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public string OrderBy { get; set; }

        public bool ReverseOrder { get; set; }
        
    }
}
