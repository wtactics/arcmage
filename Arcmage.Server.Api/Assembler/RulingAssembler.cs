using System.IO;
using System.Linq;
using Arcmage.DAL;
using Arcmage.DAL.Model;
using Arcmage.Model;

namespace Arcmage.Server.Api.Assembler
{
    public static class RulingAssembler
    {
        public static Ruling FromDal(this RulingModel rulingModel)
        {
            if (rulingModel == null) return null;
            var result = new Ruling()
            {
                Guid = rulingModel.Guid,
                Id = rulingModel.RulingId,
                RuleText = rulingModel.RuleText,
                Card = rulingModel.Card?.FromDal()
            };
          
            result.SyncBase(rulingModel, true, true);
            return result;
        }

        public static void Patch(this RulingModel rulingModel, Ruling ruling, CardModel cardModel, UserModel user)
        {
            if (rulingModel == null) return;
            rulingModel.RuleText = ruling.RuleText;
            
            if (cardModel != null) rulingModel.Card = cardModel;
            rulingModel.PatchBase(user);
        }
    }
}
