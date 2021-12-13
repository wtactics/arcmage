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
        
        public List<string> ProcessedMessageIds { get; set; }
        
        public List<string> Players { get; set; }

        public Storage()
        {
            ProcessedMessageIds = new List<string>();
            Players = new List<string>();
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
