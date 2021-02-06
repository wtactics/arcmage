using System.ComponentModel.DataAnnotations;

namespace Arcmage.DAL.Model
{
    public class CardTypeModel : ModelBase
    {
        public int CardTypeId { get; set; }

        public string Name { get; set; }

        public TemplateInfoModel TemplateInfo { get; set; }

    }
}
