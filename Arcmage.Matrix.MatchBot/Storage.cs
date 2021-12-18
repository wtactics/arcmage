using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Arcmage.Matrix.MatchBot
{
    public class Storage
    {
        // List of last 10 processed message ids
        public List<string> ProcessedMessageIds { get; set; }
        
        // Dictionary of player id's and player info objects
        public Dictionary<string, Player> Players { get; set; }

        public Storage()
        {
            ProcessedMessageIds = new List<string>();
            Players = new Dictionary<string,Player>();
        }

        public static Storage Load(string storageFile)
        {
            
            if (File.Exists(storageFile))
            {
                var processedMessagesContent = File.ReadAllText(storageFile);
                return JsonConvert.DeserializeObject<Storage>(processedMessagesContent);
            }
            return new Storage();
        }
 
        public void Save(string storageFile)
        {
            var processedMessagesContent = JsonConvert.SerializeObject(this);
            File.WriteAllText(storageFile, processedMessagesContent);
        }

    }
}
