using System;

namespace Arcmage.Matrix.MatchBot
{
    public class Command
    {
        // Command name in lower case and starting with "." 
        public string Name { get; set; }

        // Action with the sender's login and the command's arguments
        public Action<string, string[]> Execute { get; set; }
    }
}
