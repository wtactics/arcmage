using Arcmage.DAL.Model;
using Arcmage.Model;

namespace Arcmage.Server.Api.Assembler
{
    public static class RoleAssembler
    {
        public static Role FromDal(this RoleModel roleModel)
        {
            if (roleModel == null) return null;
            var result = new Role
            {
                Id = roleModel.RoleId,
                Name = roleModel.Name,
            };
            return result.SyncBase(roleModel);
        }
    }
}
