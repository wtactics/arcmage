using System.ComponentModel.DataAnnotations;

namespace Arcmage.DAL.Model
{
    public class RuleSetModel : ModelBase
    {
        public int RuleSetId { get; set; }

        public string Name { get; set; }
        
        public StatusModel Status { get; set; }
    }
}
