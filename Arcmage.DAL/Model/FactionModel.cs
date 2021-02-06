using System.ComponentModel.DataAnnotations;

namespace Arcmage.DAL.Model
{
    public class FactionModel : ModelBase
    {
        public int FactionId { get; set; }

        public string Name { get; set; }
    }
}
