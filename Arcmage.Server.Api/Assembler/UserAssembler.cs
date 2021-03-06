using Arcmage.DAL.Model;
using Arcmage.Model;

namespace Arcmage.Server.Api.Assembler
{
    public static class UserAssembler
    {
        public static User FromDal(this UserModel userModel, bool includeRole = false, bool includeDecks = false, bool includeCards = false)
        {
            if (userModel == null) return null;
            var result = new User
            {
                Id = userModel.UserId,
                Guid = userModel.Guid,
                Name = userModel.Name,
                Email = userModel.Email,
                LastLoginTime = userModel.LastLoginTime,
                CreateTime = userModel.CreateTime,
                Token = userModel.Token,
                IsDisabled = userModel.IsDisabled,
                IsVerified = userModel.IsVerified,
                
            };
            if (includeRole)
            {
                result.Role = userModel.Role.FromDal();
            }

            if (includeDecks)
            {
                userModel.Decks.ForEach(x => result.Decks.Add(x.FromDal()));
            }
            if (includeCards)
            {
                userModel.Cards.ForEach(x=> result.Cards.Add(x.FromDal()));
            }
            return result;
        }

        public static void Patch(this UserModel userModel, User newUser, RoleModel roleModel)
        {
            if (userModel == null) return;
            if (newUser == null) return;
            userModel.Name = newUser.Name;
            userModel.IsVerified = newUser.IsVerified;
            userModel.IsDisabled = newUser.IsDisabled;

            if (roleModel != null) userModel.Role = roleModel;
        }
    }
}
