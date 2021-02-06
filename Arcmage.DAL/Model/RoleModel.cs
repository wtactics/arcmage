using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Arcmage.DAL.Model
{
    public class RoleModel : ModelBase
    {
        public int RoleId { get; set; }

        public string Name { get; set; }
       
    }
}
