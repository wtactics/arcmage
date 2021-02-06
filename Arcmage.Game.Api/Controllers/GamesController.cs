using System;
using System.Linq;
using System.Threading.Tasks;
using Arcmage.Game.Api.Assembler;
using Arcmage.Game.Api.GameRuntime;
using Arcmage.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arcmage.Game.Api.Controllers
{
    [Route(Routes.Game)]
    public class GamesController : ControllerBase
    {
        public IGameRepository GameRepository { get; }


        public GamesController(IGameRepository gameRepository)
        {
            GameRepository = gameRepository;
        }

      
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody]  Model.Game game)
        {
            var createdGame = GameRepository.CreateGame(game.Name);
            return Ok(createdGame.FromDal());
        }

        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<IActionResult> Get(Guid id)
        {
            var gameModel = GameRepository.GetGames().FirstOrDefault(x => x.Guid == id);
            if (gameModel != null)
            {
                return Ok(gameModel.FromDal());
            }
            return NotFound();
        }

    }
}
