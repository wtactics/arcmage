using System;
using Arcmage.DAL.Model;
using Arcmage.Model;

namespace Arcmage.Server.Api.Assembler
{
    public static class BaseAssembler
    {
        public static T SyncBase<T>(this T t, ModelBase baseModel, bool includeCreator = false, bool includeLastModified = false) where T : Base
        {
            if (t == null) return default(T);
            t.Guid = baseModel.Guid;
            if (includeCreator)
            {
                t.CreateTime = baseModel.CreateTime;
                if (baseModel.Creator != null) t.Creator = baseModel.Creator.FromDal();
            }
            if (includeLastModified)
            {
                t.LastModifiedTime = baseModel.LastModifiedTime;
                if (baseModel.LastModifiedBy != null) t.LastModifiedBy = baseModel.LastModifiedBy.FromDal();
            }
            return t;
        }

        public static void Patch(this ModelBase baseModel, UserModel user)
        {
            if (baseModel == null) return;
            baseModel.LastModifiedTime = DateTime.UtcNow;
            baseModel.LastModifiedBy = user;
        }
    }
}
