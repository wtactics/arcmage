using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Requests.UserData
{
    [DataContract]
    public class UserProfileSetDisplayName
    {
        [DataMember(Name = "displayname")]
        public string DisplayName { get; set; }
    }
}
