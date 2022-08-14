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
import { ConfirmComponent } from "./modules/confirm/confirm.component";
import { PasswordResetComponent } from "./modules/password-reset/password-reset.component";
import { PasswordForgetComponent } from "./modules/password-forget/password-forget.component";
import { SettingsComponent } from "./modules/settings/settings.component";
import { UsersComponent } from "./modules/users/users.component";
import { SeriesComponent } from "./modules/series/series.component";
import { LicensesComponent } from "./modules/licenses/licenses.component";

import { AuthGuardService } from "./services/auth/auth-guard.service";
import { Rights } from "./models/rights";
import { CreateGameComponent } from "./modules/create-game/create-game.component";



const routes: Routes = [
  { path: "unauthorized", component: UnauthorizedComponent },
  {
    path: "players",
    component: UsersComponent,
    data: { requiredUserRight: Rights.viewPlayer },
    canActivate: [AuthGuardService]
  },
  { path: "cards", component: CardsComponent },
  { path: "cards/:cardId", component: CardDetailsComponent },
  { path: "decks", component: DecksComponent },
  { path: "decks/:deckId", component: DeckDetailsComponent },
  { path: "play", component: CreateGameComponent },
  { path: "games", component: GamesComponent },
  { path: "invite/:gameId", component: GameInviteComponent },
  { path: "login", component: LoginComponent },
  { path: "register", component: RegisterComponent },
  { path: "password-reset", component: PasswordResetComponent },
  { path: "password-forget", component: PasswordForgetComponent },
  { path: "confirm", component: ConfirmComponent },
  {
    path: "settings/:userId",
    component: SettingsComponent,
    data: { requiredUserRight: Rights.viewPlayer, checkUserId: true },
    canActivate: [AuthGuardService]
  },
  {
    path: "admin/series",
    component: SeriesComponent,
    data: { requiredUserRight: Rights.viewSerie},
    canActivate: [AuthGuardService]
  },
  {
    path: "admin/licenses",
    component: LicensesComponent,
    data: { requiredUserRight: Rights.viewLicense},
    canActivate: [AuthGuardService]
  },
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
