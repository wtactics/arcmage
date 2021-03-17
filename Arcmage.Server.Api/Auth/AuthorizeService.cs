using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcmage.DAL.Model;
using Arcmage.Model;

namespace Arcmage.Server.Api.Auth
{
    public static class AuthorizeService
    {
    
        public static bool HashRight(RoleModel role, Right right)
        {
            if (role != null)
            {
                if (role.Guid == PredefinedGuids.ServiceUser) return CheckRight(Rights.ServiceUserRights, right);
                if (role.Guid == PredefinedGuids.Administrator) return CheckRight(Rights.AdministratorRights, right);
                if (role.Guid == PredefinedGuids.Developer) return CheckRight(Rights.DeveloperRights, right);
                if (role.Guid == PredefinedGuids.Contributer) return CheckRight(Rights.ContributerRights, right);
            }
            return CheckRight(Rights.DefaultRights, right);
        }

        public static bool CheckRight(List<Right> rights, Right right)
        {
            return rights.Select(x=>x.Guid).Contains(right.Guid);
        }

        public static List<Right> GetRights(RoleModel role)
        {
            if (role != null)
            {
                if (role.Guid == PredefinedGuids.ServiceUser) return Rights.ServiceUserRights;
                if (role.Guid == PredefinedGuids.Administrator) return Rights.AdministratorRights;
                if (role.Guid == PredefinedGuids.Developer) return Rights.DeveloperRights;
                if (role.Guid == PredefinedGuids.Contributer) return Rights.ContributerRights;
            }
            return Rights.DefaultRights;
        }
    }
}
