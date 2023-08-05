using System;
using System.Threading.Tasks;

namespace Arcmage.Matrix.MatchBot
{
    public class Command
    {
        // Command name in lower case and starting with "!" 
        public string Name { get; set; }

        // Action with to be executed with the RoomId, the sender and the command arguments
        public Action<string, string, string[]> Execute { get; set; }
    }
}
