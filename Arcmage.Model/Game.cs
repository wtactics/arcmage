using System;

namespace Arcmage.Model
{
    public class Game
    {
        public Guid Guid { get; set; }

        public string Name { get; set; }

        public bool CanJoin { get; set; }

        public DateTime? CreateTime { get; set; }
    }
}