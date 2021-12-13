using System.Runtime.Serialization;

namespace Matrix.Sdk.Api.Requests.Session
{
    [DataContract]
    public class MatrixLoginPassword : MatrixLogin
    {
        public MatrixLoginPassword(string _user, string _pass)
        {
            _type = "m.login.password";

            User = _user;
            Password = _pass;
        }

        [DataMember(Name = "user")]
        public string User { get; set; }
        [DataMember(Name ="password")]
        public string Password { get; set; }
    }
}
