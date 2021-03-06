using Arcmage.DAL.Model;
using Arcmage.Model;

namespace Arcmage.Server.Api.Assembler
{
    public static class StatusAssembler
    {
        public static Status FromDal(this StatusModel statusModel)
        {
            if (statusModel == null) return null;
            var result = new Status()
            {
                Id = statusModel.StatusId,
                Name = statusModel.Name
            };
            return result.SyncBase(statusModel);
        }

        public static void Patch(this StatusModel statusModel, Status status, UserModel user)
        {
            if (statusModel == null) return;
            statusModel.Name = status.Name;
            statusModel.PatchBase(user);
        }
    }
}
