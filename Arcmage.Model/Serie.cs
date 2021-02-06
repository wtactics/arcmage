using System.Collections.Generic;

namespace Arcmage.Model
{
    public class Serie : Base
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Card> Cards { get; set; }

        public Status Status { get; set; }

        public Serie()
        {
            Cards = new List<Card>();
        }
    }
}
