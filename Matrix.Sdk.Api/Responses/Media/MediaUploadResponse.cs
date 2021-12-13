using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Responses.Media
{
    [DataContract]
    class MediaUploadResponse
    {
        [DataMember(Name = "content_uri")]
        public string ContentUri { get; set; }
    }
}
