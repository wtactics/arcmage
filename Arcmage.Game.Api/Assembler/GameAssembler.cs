namespace Arcmage.Game.Api.Assembler
{
    public static class GameAssembler
    {
        public static Model.Game FromDal(this GameRuntime.Game game)
        {
            if (game == null) return null;
            var result = new Model.Game()
            {
                Guid = game.Guid,
                Name = game.Name,
                CanJoin = game.CanJoin,
                CreateTime = game.CreateTime
            };
            return result;
        }
    }
}
