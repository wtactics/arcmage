using System.IO;
using Arcmage.DAL;
using Arcmage.DAL.Model;
using Arcmage.Layout.InputConvertor;
using Arcmage.Model;
using Arcmage.Server.Api.Utils;

namespace Arcmage.Server.Api.Assembler
{
    public static class CardAssembler
    {
        public static Card FromDal(this CardModel cardModel)
        {
            if (cardModel == null) return null;
            var result = new Card();
            result.Id = cardModel.CardId;
            result.Name = cardModel.Name;
            result.FirstName = cardModel.FirstName;
            result.LastName = cardModel.LastName;
            result.Artist = cardModel.Artist;
            result.RuleText = cardModel.RuleText;
            result.FlavorText = cardModel.FlavorText;
            result.SubType = cardModel.SubType;
            result.Cost = cardModel.Cost;
            result.Loyalty = cardModel.Loyalty;
            result.Attack = cardModel.Attack;
            result.Defense = cardModel.Defense;
            result.Info = cardModel.Info;
            result.MarkdownText = cardModel.MarkdownText;
            result.Language = Languages.GetLanguage(cardModel.LanguageCode);
            
            if (cardModel.Type != null) result.Type = cardModel.Type.FromDal();
            if (cardModel.Faction != null) result.Faction = cardModel.Faction.FromDal();
            if (cardModel.Serie != null) result.Serie = cardModel.Serie.FromDal();
            if (cardModel.RuleSet != null) result.RuleSet = cardModel.RuleSet.FromDal();
            if (cardModel.Status != null) result.Status = cardModel.Status.FromDal();

            result.SyncBase(cardModel,true, true);

            result.IsGenerated = File.Exists(Repository.GetPngFile(cardModel.Guid)) && string.IsNullOrEmpty(cardModel.PngCreationJobId);

            result.Artwork = $"/api/Cards/{cardModel.Guid}/export?format=Art&modified={result.LastModifiedTime.Value.Ticks}";

            result.Svg = $"/api/Cards/{cardModel.Guid}/export?format=Svg&modified={result.LastModifiedTime.Value.Ticks}";
            result.Png = $"/api/Cards/{cardModel.Guid}/export?format=Png&modified={result.LastModifiedTime.Value.Ticks}";
            result.Jpeg = $"/Arcmage/Cards/{cardModel.Guid}/card.jpg";
            result.Pdf = $"/api/Cards/{cardModel.Guid}/export?format=Pdf&modified={result.LastModifiedTime.Value.Ticks}";

            result.BackPng = $"/api/Cards/{cardModel.Guid}/export?format=BackPng";
            result.BackJpeg = $"/api/Cards/{cardModel.Guid}/export?format=BackJpeg";
            result.BackPdf = $"/api/Cards/{cardModel.Guid}/export?format=BackPdf";
            result.BackSvg = $"/api/Cards/{cardModel.Guid}/export?format=BackSvg";

            result.OverlaySvg = $"/api/Cards/{cardModel.Guid}/export?format=OverlaySvg&modified={result.LastModifiedTime.Value.Ticks}";
            // background only changes when type or faction changes
            if (cardModel.Faction != null && result.Type != null) result.BackgroundPng = $"/api/Cards/{cardModel.Guid}/export?format=BackgroundPng&faction={result.Faction.Guid}&type={result.Type.Guid}";
            return result;
        }

        public static void Patch(this CardModel cardModel, Card card, 
            SerieModel serieModel, FactionModel factionModel, CardTypeModel cardTypeModel, StatusModel statusModel, RuleSetModel ruleSetModel, UserModel user)
        {
            if (cardModel == null) return;
            if (card == null) return;
            cardModel.Name = card.Name;
            cardModel.FirstName = card.FirstName;
            cardModel.LastName = card.LastName;
            cardModel.Artist = card.Artist;
            cardModel.RuleText = card.RuleText;
            cardModel.FlavorText = card.FlavorText;
            cardModel.SubType = card.SubType;
            cardModel.Cost = card.Cost;
            cardModel.Loyalty = card.Loyalty;
            cardModel.Attack = card.Attack;
            cardModel.Defense = card.Defense;
            cardModel.Info = card.Info;
            cardModel.MarkdownText = card.MarkdownText;
            cardModel.LanguageCode = card.Language?.LanguageCode?? "en";
           
            
            if (serieModel != null) cardModel.Serie = serieModel;
            if (factionModel != null) cardModel.Faction = factionModel;
            if (cardTypeModel != null) cardModel.Type = cardTypeModel;
            if (statusModel != null) cardModel.Status = statusModel;
            if (ruleSetModel != null) cardModel.RuleSet = ruleSetModel;
            cardModel.Patch(user);
        }

    }

}
