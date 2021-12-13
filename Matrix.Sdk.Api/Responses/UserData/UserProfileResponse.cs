using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Responses.UserData
{
    [DataContract]
    public class UserProfileResponse
    {
        [DataMember(Name = "avatar_url")]
        public string AvatarUrl { get; set; }
        [DataMember(Name = "displayname")]
        public string DisplayName { get; set; }
    }
}
