using Arcmage.DAL.Model;
using Arcmage.Model;

namespace Arcmage.Server.Api.Assembler
{
    public static class RuleSetAssembler
    {
        public static RuleSet FromDal(this RuleSetModel ruleSetModel)
        {
            if (ruleSetModel == null) return null;
            var result = new RuleSet
            {
                Id = ruleSetModel.RuleSetId,
                Name = ruleSetModel.Name,
                Status = ruleSetModel.Status.FromDal()
            };
            return result.SyncBase(ruleSetModel);
        }

        public static void Patch(this RuleSetModel ruleSetModel, RuleSet ruleSet, UserModel user)
        {
            if (ruleSetModel == null) return;
            ruleSetModel.Name = ruleSet.Name;
            ruleSetModel.PatchBase(user);
        }

    }
  
}
