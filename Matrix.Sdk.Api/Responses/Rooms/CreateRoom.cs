using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Responses.Rooms
{
    [DataContract]
    public class CreateRoom
    {
        [DataMember(Name = "room_id")]
        public string RoomID { get; set; }
    }
}
