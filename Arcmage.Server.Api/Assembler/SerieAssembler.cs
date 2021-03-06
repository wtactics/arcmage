using Arcmage.DAL.Model;
using Arcmage.Model;

namespace Arcmage.Server.Api.Assembler
{
    public static class SerieAssembler
    {
        public static Serie FromDal(this SerieModel serieModel, bool includeCards = false)
        {
            if (serieModel == null) return null;
            var result = new Serie
            {
                Id = serieModel.SerieId,
                Name = serieModel.Name,
                Status = serieModel.Status.FromDal()
            };

            if (includeCards)
            {
                serieModel.Cards.ForEach(x=>result.Cards.Add(x.FromDal()));
            }
            return result.SyncBase(serieModel);
        }

        public static void Patch(this SerieModel serieModel, Serie serie, UserModel user)
        {
            if (serieModel == null) return;
            serieModel.Name = serie.Name;
            serieModel.PatchBase(user);
        }

    }
  
}
