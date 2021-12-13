using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Responses
{
    [DataContract]
    public class VersionResponse
    {
        [DataMember(Name = "versions")]
        public string[] Versions { get; set; }
    }

}
