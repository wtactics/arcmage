import { Component, OnInit, ViewChild, ViewEncapsulation, OnDestroy } from "@angular/core";
import { GlobalEventsService } from "src/app/services/global/global-events.service";
import { Router } from "@angular/router";
import { Game } from "src/app/models/game";
import { Subscription } from "rxjs";
import { ResultList } from "src/app/models/result-list";
import { GameSearchOptions } from "src/app/models/game-search-options";
import { Table } from "primeng/table/table";
import { GameApiService } from "src/app/services/api/game-api.service";
import { LazyLoadEvent } from "primeng/api/public_api";
import { Deck } from "src/app/models/deck";
import { DeckApiService } from "src/app/services/api/deck-api.service";
import { DeckSearchOptions } from "src/app/models/deck-search-options";
import { User } from "src/app/models/user";
import { Guid } from "guid-typescript";
import {StepsModule} from "primeng/steps";
import {MenuItem} from "primeng/api";

@Component({
  selector: "app-games",
  templateUrl: "./games.component.html",
  styleUrls: ["./games.component.scss"],
  encapsulation: ViewEncapsulation.None,
})
export class GamesComponent implements OnInit, OnDestroy {

  loading: boolean;

  isAuthenticated = false;
  subscription: Subscription = new Subscription();
  user: User;

  gameSetupSteps: MenuItem[];
  activeIndex = 0;

  showGameCreation: boolean;
  newGame: Game;

  deckSearchResult: ResultList<Deck>;
  deckSearchOptions: DeckSearchOptions;

  gameSearchResult: ResultList<Game>;
  gameSearchOptions: GameSearchOptions;

  @ViewChild("gamesTable") table: Table;

  constructor(private globalEventsService: GlobalEventsService, private gameApiService: GameApiService, private deckApiService: DeckApiService, private router: Router) { }

  ngOnInit(): void {

    this.gameSetupSteps = [
      {label: "Create a new game"},
      {label: "Invite a friend"},
      {label: "Select a deck"},
      {label: "Start the game"}
    ];

    this.newGame = new Game();

    this.gameSearchOptions = new GameSearchOptions();
    this.gameSearchOptions.pageNumber = 1;
    this.gameSearchOptions.search = "";

    this.gameSearchResult = new ResultList<Game>();
    this.gameSearchResult.items = [];
    this.gameSearchResult.totalItems = 0;

    this.deckSearchOptions = new DeckSearchOptions();
    this.deckSearchOptions.pageNumber = 1;
    this.deckSearchOptions.search = "";
    this.deckSearchOptions.pageSize = 50;

    this.deckSearchResult = new ResultList<Deck>();
    this.deckSearchResult.items = [];
    this.deckSearchResult.totalItems = 0;

    this.subscription.add(
      this.globalEventsService.isAuthenticated$.subscribe((value) => {
        if (value !== null) {
          this.isAuthenticated = value;
        }
      }));

    this.subscription.add(
      this.globalEventsService.currentUser$.subscribe((value) => {
        if (value !== null) {
          this.user = value;
        }
      }));

    this.searchDecks();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  searchGames(): void {
    this.gameApiService.search$(this.gameSearchOptions).subscribe(
      (result) => {
        this.gameSearchResult.items = result.items;
        this.gameSearchResult.totalItems = result.totalItems;
      },
      (error) => {
        this.loading = false;
      },
      () => this.loading = false);
  }

  searchDecks(): void {
    this.deckApiService.search$(this.deckSearchOptions).subscribe(
      (result) => {
        this.deckSearchResult.items = result.items;
        this.deckSearchResult.totalItems = result.totalItems;
      },
      (error) => {
        this.loading = false;
      },
      () => this.loading = false);
  }

  loadData(event: LazyLoadEvent) {

    if (event == null && this.table.lazy) {
      this.gameSearchOptions.pageNumber = 1;
      this.gameSearchOptions.pageSize = this.table.rows;
    }
    else{
      this.gameSearchOptions.pageNumber = Math.floor(event.first / event.rows) + 1;
      this.gameSearchOptions.pageSize = event.rows;

      if (event.sortField) {
        this.gameSearchOptions.orderBy = event.sortField;
        this.gameSearchOptions.reverseOrder = event.sortOrder > 0;
      }
    }
    this.searchGames();
  }

  searchClick(){
    this.gameSearchOptions.pageNumber = 1;
    this.gameSearchOptions.pageSize = this.table.rows;
    this.searchGames();
  }

  createGame() {
    this.newGame = new Game();
    this.showGameCreation = true;
  }

  joinGame (game: Game, deck: Deck){
    const playerGuid = this.isAuthenticated && this.user ? this.user.guid : Guid.create().toString();
    const playerName =  this.isAuthenticated && this.user ? encodeURIComponent(this.user.name) : "Guest";
    const getUrl = window.location;
    const baseUrl = getUrl .protocol + "//" + getUrl.host + "/arcmage/Game/index.html?"
      + "gameGuid=" + game.guid + "&"
      + "deckGuid=" + deck.guid + "&"
      + "playerGuid=" + playerGuid + "&"
      + "playerName=" + playerName;

    window.open(baseUrl, "_blank");
  }

  saveGame() {
    this.gameApiService.create$(this.newGame).subscribe(
      game => {
        this.showGameCreation = false;
        this.router.navigate(["/invite", game.guid]);
      },
      error => {
        this.showGameCreation = false;
      }

    );
  }

}
