using System;
using System.Collections.Generic;
using System.Text;

namespace Arcmage.Model
{
    public class SettingsOptions
    {
        public bool IsPlayerAdmin { get; set; }

        public List<Role> Roles { get; set; }

    }
}
