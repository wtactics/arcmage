using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Responses.Events.Room
{
    [DataContract]
    public class Create : MatrixEvents
    {
        [DataMember(Name = "content")]
        public CreateContent Content { get; set; }
    }

    [DataContract]
    public class CreateContent
    {
        [DataMember(Name = "creator")]
        public string Creator { get; set; }
    }
}
