namespace Arcmage.DAL.Model
{
    public class LicenseModel : ModelBase
    {
        public int LicenseId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }
    }
}
