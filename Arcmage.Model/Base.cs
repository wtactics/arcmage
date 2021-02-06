using System;

namespace Arcmage.Model
{   
    public class Base
    {
        public Guid Guid { get; set; }

        public User Creator { get; set; }

        public DateTime? CreateTime { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime? LastModifiedTime { get; set; }
    }
}