namespace Arcmage.Model
{
    public class CardType : Base
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public TemplateInfo TemplateInfo { get; set; }
    }
}
