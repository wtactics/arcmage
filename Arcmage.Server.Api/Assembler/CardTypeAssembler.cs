using Arcmage.DAL.Model;
using Arcmage.Model;

namespace Arcmage.Server.Api.Assembler
{
    public static class CardTypeAssembler
    {
        public static CardType FromDal(this CardTypeModel cardTypeModel, bool includeTemplateInfo = false)
        {
            if (cardTypeModel == null) return null;
            var result = new CardType()
            {
                Id = cardTypeModel.CardTypeId,
                Name = cardTypeModel.Name
            };
            if (includeTemplateInfo)
            {
                result.TemplateInfo = cardTypeModel.TemplateInfo.FromDal();
            }
            return result.SyncBase(cardTypeModel);
        }

        public static void Patch(this CardTypeModel cardTypeModel, CardType cardType, TemplateInfoModel templateInfoModel, UserModel user)
        {
            if (cardTypeModel == null) return;
            cardTypeModel.Name = cardType.Name;
            cardTypeModel.TemplateInfo = templateInfoModel;
            cardTypeModel.Patch(user);
        }
    }
}
