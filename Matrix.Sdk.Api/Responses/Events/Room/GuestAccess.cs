using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Responses.Events.Room
{
    [DataContract]
    public class GuestAccess : MatrixEvents
    {
        [DataMember(Name = "content")]
        public GuestAccessContent Content { get; set; }
    }

    [DataContract]
    public class GuestAccessContent
    {
        [DataMember(Name = "guest_access", IsRequired = true)]
        public string GuestAccess { get; set; }
    }
}
