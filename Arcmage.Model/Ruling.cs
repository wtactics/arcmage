using System;
using System.Collections.Generic;
using System.Text;

namespace Arcmage.Model
{
    public class Ruling : Base
    {
        public int Id { get; set; }

        public Card Card { get; set; }

        public string RuleText { get; set; }

    }
}
