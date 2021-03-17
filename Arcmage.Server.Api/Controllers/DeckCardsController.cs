using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Arcmage.DAL;
using Arcmage.DAL.Utils;
using Arcmage.Model;
using Arcmage.Server.Api.Assembler;
using Arcmage.Server.Api.Auth;
using Arcmage.Server.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arcmage.Server.Api.Controllers
{
  
    [Route(Routes.DeckCards)]
    public class DeckCardsController : ControllerBase
    {

        [Authorize]
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] DeckCard deckCard)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.EditDeck))
                {
                    return Forbid();
                }

                if (deckCard.Card == null)
                {
                    return BadRequest("The card is required.");
                }
                if (deckCard.Deck == null)
                {
                    return BadRequest("The deck is required.");
                }

                var deckModel = await repository.Context.Decks.FindByGuidAsync(deckCard.Deck.Guid);
                if (deckModel == null)
                {
                    return BadRequest("The deck is not found.");
                }

                var cardModel = await repository.Context.Cards.FindByGuidAsync(deckCard.Card.Guid);
                if (cardModel == null)
                {
                    return BadRequest("The card is not found.");
                }

                await repository.Context.Entry(deckModel).Reference(x => x.Creator).LoadAsync();
                var isMyDeck = repository.ServiceUser?.Guid == deckModel.Creator.Guid;

                if(!isMyDeck && !AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.AllowOthersDeckEdit))
                {
                    return Forbid("The specified deck is not yours.");
                }

                var deckCardModel = await repository.Context.DeckCards.Include(x => x.Deck).Include(x => x.Card).FirstOrDefaultAsync(x => x.Card.CardId == cardModel.CardId && x.Deck.DeckId == deckModel.DeckId);
                if (deckCardModel == null)
                {
                    if (deckCard.Quantity > 0)
                    {
                        deckCardModel = repository.CreateDeckCard(deckModel, cardModel, deckCard.Quantity, Guid.NewGuid());
                    }

                }
                else
                {
                    if (deckCard.Quantity <= 0)
                    {
                        deckModel.DeckCards.Remove(deckCardModel);
                        repository.Context.DeckCards.Remove(deckCardModel);
                        await repository.Context.SaveChangesAsync();
                    }
                    else
                    {
                        deckCardModel.Quantity = deckCard.Quantity;
                        await repository.Context.SaveChangesAsync();
                    }
                }
                return Ok(deckCardModel.FromDal());
            }
        }
      
    }
}
