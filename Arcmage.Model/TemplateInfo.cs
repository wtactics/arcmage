namespace Arcmage.Model
{
    public class TemplateInfo : Base
    {
        public int Id { get; set; }
        
        public bool ShowName { get; set; }

        public bool ShowType { get; set; }

        public bool ShowGoldCost { get; set; }

        public bool ShowLoyalty { get; set; }

        public bool ShowText { get; set; }

        public bool ShowAttack { get; set; }

        public bool ShowDefense { get; set; }

        public bool ShowDiscipline { get; set; }

        public bool ShowArt { get; set; }

        public bool ShowInfo { get; set; }

        public double MaxTextBoxWidth { get; set; }

        public double MaxTextBoxHeight { get; set; }
    }
}
