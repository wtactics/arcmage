// This net5 Matrix.Sdk is a fork from https://github.com/VRocker/MatrixAPI

// Following changes were made 
// - Added option to send matrix formatted text (html) 

// The Matrix.Sdk is under Apache License, Version 2.0, January 2004, http://www.apache.org/licenses/

using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Requests.Rooms.Message
{
    [DataContract]
    public class MatrixRoomMessageText : MatrixRoomMessageBase
    {
        public MatrixRoomMessageText()
            : base()
        {
            base.MessageType = "m.text";
        }

        [DataMember(Name = "body")]
        public string Body { get; set; }


        [DataMember(Name = "formatted_body")]
        public string FormattedBody { get; set; }

        [DataMember(Name = "format")]
        public string Format { get; set; }
    }
}
