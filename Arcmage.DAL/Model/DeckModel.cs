using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Arcmage.DAL.Model
{
    public class DeckModel : ModelBase
    {
        public int DeckId { get; set; }

        public string Name { get; set; }

        public string Pdf { get; set; }

        public bool ExportTiles { get; set; }

        public bool GeneratePdf { get; set; }

        public string PdfZipCreationJobId { get; set; }

        public StatusModel Status { get; set; }

        public virtual List<DeckCardModel> DeckCards { get; set; }

        

    }
}
