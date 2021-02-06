using System.ComponentModel.DataAnnotations;

namespace Arcmage.DAL.Model
{
    public class StatusModel : ModelBase
    {
        public int StatusId { get; set; }

        public string Name { get; set; }
    }
}
