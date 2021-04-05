using Arcmage.DAL.Model;
using Arcmage.Model;

namespace Arcmage.Server.Api.Assembler
{
    public static class LicenseAssembler
    {
        public static License FromDal(this LicenseModel licenseModel)
        {
            if (licenseModel == null) return null;
            var result = new License
            {
                Id = licenseModel.LicenseId,
                Name = licenseModel.Name,
                Description = licenseModel.Description,
                Url = licenseModel.Url
            };
            return result.SyncBase(licenseModel);
        }

        public static void Patch(this LicenseModel licenseModel, License license, UserModel user)
        {
            if (licenseModel == null) return;
            licenseModel.Name = license.Name;
            licenseModel.Description = license.Description;
            licenseModel.Url = license.Url;
            licenseModel.PatchBase(user);
        }
    }
}
