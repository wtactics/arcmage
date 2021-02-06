using System.Collections.Generic;

namespace Arcmage.Model
{
    public class ResultList<T> 
    {
        public List<T> Items { get; set; }

        public int TotalItems { get; set; }

        public SearchOptionsBase SearchOptions { get; set; }

        public ResultList()
        {
            Items = new List<T>();
        }

        public ResultList(List<T> items)
        {
            Items = items;
            TotalItems = Items.Count;
        }
    }
}
