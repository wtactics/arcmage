using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Requests.Rooms.Message
{
    [DataContract]
    public class MatrixRoomMessageBase
    {
        [DataMember(Name = "msgtype")]
        public string MessageType { get; set; }
    }
}
