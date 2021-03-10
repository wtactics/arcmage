namespace Arcmage.Model
{
    public class UserSearchOptions : SearchOptionsBase
    {
        public Role Role { get; set; }

        public bool? IsVerified { get; set; }

        public bool? IsDisabled { get; set; }
    }
}
