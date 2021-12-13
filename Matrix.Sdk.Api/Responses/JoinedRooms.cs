using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Responses
{
    [DataContract]
    public class JoinedRooms
    {
        [DataMember(Name = "joined_rooms")]
        public List<string> Rooms { get; set; }
    }
}
