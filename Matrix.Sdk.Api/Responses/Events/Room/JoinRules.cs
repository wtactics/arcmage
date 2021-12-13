using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Responses.Events.Room
{
    [DataContract]
    public class JoinRules : MatrixEvents
    {
        [DataMember(Name = "content")]
        public JoinRulesContent Content { get; set; }
    }

    [DataContract]
    public class JoinRulesContent
    {
        [DataMember(Name = "join_rule")]
        public string JoinRule { get; set; }
    }
}
