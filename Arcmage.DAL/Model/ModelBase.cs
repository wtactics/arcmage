using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Arcmage.DAL.Model
{
    public class ModelBase
    {
        public Guid Guid { get; set; }

     
        public UserModel Creator { get; set; }

        public int CreatorId { get; set; }

        public DateTime CreateTime { get; set; }

        public UserModel LastModifiedBy { get; set; }

        public int LastModifiedById { get; set; }

        public DateTime LastModifiedTime { get; set; }

    }
}
