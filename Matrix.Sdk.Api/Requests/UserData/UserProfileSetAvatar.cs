using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Requests.UserData
{
    [DataContract]
    public class UserProfileSetAvatar
    {
        [DataMember(Name = "avatar_url")]
        public string AvatarUrl { get; set; }
    }
}
