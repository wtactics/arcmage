using Arcmage.DAL.Model;
using Arcmage.Model;

namespace Arcmage.Server.Api.Assembler
{
    public static class FactionAssembler
    {
        public static Faction FromDal(this FactionModel factionModel)
        {
            if (factionModel == null) return null;
            var result = new Faction
            {
                Id = factionModel.FactionId,
                Name = factionModel.Name
            };
            return result.SyncBase(factionModel);
        }

        public static void Patch(this FactionModel factionModel, Faction faction, UserModel user)
        {
            if (factionModel == null) return;
            factionModel.Name = faction.Name;
            factionModel.PatchBase(user);
        }
    }
}
