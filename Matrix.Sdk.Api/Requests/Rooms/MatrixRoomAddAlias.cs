using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Requests.Rooms
{
    [DataContract]
    public class MatrixRoomAddAlias
    {
        [DataMember(Name = "room_id")]
        public string RoomID { get; set; }
    }
}
