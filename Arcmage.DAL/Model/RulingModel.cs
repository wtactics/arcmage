using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcmage.DAL.Model
{
    public class RulingModel : ModelBase
    {
        [Key] public int RulingId { get; set; }

        public CardModel Card { get; set; }

        public string RuleText { get; set; }
    }
}
