using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Responses.Events
{
    [DataContract]
    public class Direct : MatrixEvents
    {
        [DataMember(Name = "content")]
        public Dictionary<string, List<string>> Content { get; set; }
    }
}
