import { NgModule } from "@angular/core";
import { Routes, RouterModule, PreloadAllModules } from "@angular/router";

import { UnauthorizedComponent } from "./modules/app/components/unauthorized/unauthorized.component";
import { CardsComponent } from "./modules/cards/cards.component";
import { LoginComponent } from "./modules/login/login.component";
import { RegisterComponent } from "./modules/register/register.component";
import { CardDetailsComponent } from "./modules/card-details/card-details.component";
import { DecksComponent } from "./modules/decks/decks.component";
import { DeckDetailsComponent } from "./modules/deck-details/deck-details.component";
import { GamesComponent } from "./modules/games/games.component";
import { GameInviteComponent } from "./modules/game-invite/game-invite.component";

const routes: Routes = [
  { path: "unauthorized", component: UnauthorizedComponent },
  { path: "cards", component: CardsComponent },
  { path: "cards/:cardId", component: CardDetailsComponent },
  { path: "decks", component: DecksComponent },
  { path: "decks/:deckId", component: DeckDetailsComponent },
  { path: "games", component: GamesComponent },
  { path: "invite/:gameId", component: GameInviteComponent },
  { path: "login", component: LoginComponent },
  { path: "register", component: RegisterComponent },
  { path: "**", redirectTo: "cards" }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {
    preloadingStrategy: PreloadAllModules,
    onSameUrlNavigation: "reload",
    relativeLinkResolution: "legacy"
})],
  exports: [RouterModule]
})
export class AppRoutingModule { }
