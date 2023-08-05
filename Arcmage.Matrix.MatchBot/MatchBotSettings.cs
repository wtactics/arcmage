using System.Collections.Generic;

namespace Arcmage.Matrix.MatchBot
{
    public class MatchBotSettings
    {
     
        public string AmindunaApi { get; set; }

        public string HomeServer { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public List<string> RoomIds { get; set; }


        public string ApplicationID { get; set; }
        
        public string ApplicationName { get; set; }

        public string DeviceID { get; set; }

        public string DeviceName { get; set; }

        public string StorageFile { get; set; }

        public MatchBotSettings()
        {
            RoomIds = new List<string>();
        }

    }
}
