using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Arcmage.DAL.Model
{
    public class SerieModel : ModelBase
    {
        public int SerieId { get; set; }

        public string Name { get; set; }
    
        public virtual List<CardModel> Cards { get; set; }

        public StatusModel Status { get; set; }

    }
}
