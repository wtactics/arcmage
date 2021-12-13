using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Requests.Rooms.Message
{
    [DataContract]
    public class MatrixRoomMessageEmote : MatrixRoomMessageBase
    {
        public MatrixRoomMessageEmote()
            : base()
        {
            base.MessageType = "m.emote";
        }
    }
}
