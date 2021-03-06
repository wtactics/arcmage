import { Component, OnInit, OnDestroy, ViewChild } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { GameApiService } from "src/app/services/api/game-api.service";
import { Game } from "src/app/models/game";
import { ResultList } from "src/app/models/result-list";
import { Deck } from "src/app/models/deck";
import { DeckSearchOptions } from "src/app/models/deck-search-options";
import { DeckApiService } from "src/app/services/api/deck-api.service";
import { MenuItem } from "primeng/api/menuitem";
import { GlobalEventsService } from "src/app/services/global/global-events.service";
import { Subscription } from "rxjs";
import { User } from "src/app/models/user";
import { Guid } from "guid-typescript";
import { MessageService } from "primeng/api";
import { Clipboard } from "@angular/cdk/clipboard";
import { SlickCarouselComponent } from "ngx-slick-carousel";
import { ConfigurationService } from "src/app/services/global/config.service";
import { LangChangeEvent, TranslateService } from "@ngx-translate/core";

@Component({
  selector: "app-game-invite",
  templateUrl: "./game-invite.component.html",
  styleUrls: ["./game-invite.component.scss"]
})
export class GameInviteComponent implements OnInit, OnDestroy {

  apiUri: string;

  loading: boolean;
  gameExpired: boolean;
  started = false;

  isAuthenticated = false;
  subscription: Subscription = new Subscription();
  user: User;
  userName = "Guest";
  invitedBy = null;

  showCarousel = false;
  slideConfig: any;

  game: Game;

  gameSetupSteps: MenuItem[];
  enterNameMenuItem: MenuItem;
  copyInviteLinkMenuItem: MenuItem;
  selectDeckMenuItem: MenuItem;
  startGameMenuItem: MenuItem;
  activeIndex = 0;

  deckSearchResult: ResultList<Deck>;
  deckSearchOptions: DeckSearchOptions;

  deck: Deck;


  constructor(private configurationService: ConfigurationService,
              private globalEventsService: GlobalEventsService,
              private route: ActivatedRoute,
              private gameApiService: GameApiService,
              private deckApiService: DeckApiService,
              private messageService: MessageService,
              private translateService: TranslateService,
              private clipboard: Clipboard) {
    this.apiUri = this.configurationService.configuration.apiUri;
    this.slideConfig = this.configurationService.configuration.slideConfig;
  }

  ngOnInit(): void {

    this.translateService.onLangChange.subscribe((langChangeEvent: LangChangeEvent) => {
      this.userName = this.translateService.instant("invite.guest");
      this.enterNameMenuItem =  {label: this.translateService.instant("invite.step.enter-name")};
      this.copyInviteLinkMenuItem = {label: this.translateService.instant("invite.step.invite-friend"), visible: true};
      this.selectDeckMenuItem = {label: this.translateService.instant("invite.step.select-deck")};
      this.startGameMenuItem =  {label: this.translateService.instant("invite.step.start-game")};
    });

    this.gameSetupSteps = [
      this.enterNameMenuItem,
      this.selectDeckMenuItem,
      this.startGameMenuItem,
    ];
    this.activeIndex = this.gameSetupSteps.length - 2;

    this.deckSearchOptions = new DeckSearchOptions();
    this.deckSearchOptions.pageNumber = 1;
    this.deckSearchOptions.search = "";
    this.deckSearchOptions.pageSize = 50;
    this.deckSearchOptions.myDecks = false;
    this.deckSearchOptions.excludeDrafts = true;

    this.deckSearchResult = new ResultList<Deck>();
    this.deckSearchResult.items = [];
    this.deckSearchResult.totalItems = 0;


    this.loading = true;
    this.route.paramMap.subscribe(params => {
      const gameGuid = params.get("gameId");
      this.gameApiService.get$(gameGuid).subscribe(
        game => {
          this.game = game;
          this.loading = false;
        },
        error => {
          this.gameExpired = true;
          this.loading = false;
        }
      );
    });

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
          this.userName = this.user.name;
        }
      }));

    this.subscription.add(
      this.route.queryParams
      .subscribe(params => {
        // tslint:disable-next-line:no-string-literal
        this.invitedBy = params["invitedBy"] || null;
        if (!this.invitedBy) {
          this.gameSetupSteps = [
            this.enterNameMenuItem,
            this.copyInviteLinkMenuItem,
            this.selectDeckMenuItem,
            this.startGameMenuItem,
          ];
          this.activeIndex = this.gameSetupSteps.length - 2;
        }
      }));


    this.searchDecks();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
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

  copyInviteLink() {
    const getUrl = window.location;
    const playerName =  this.isAuthenticated && this.user ? encodeURIComponent(this.user.name) : this.userName;
    const baseUrl = this.apiUri + "/#/invite/" + this.game.guid + "?invitedBy=" + playerName;
    this.clipboard.copy(baseUrl);
    this.activeIndex = 1;
    this.messageService.add({
        severity: "success",
        detail: this.translateService.instant("invite.link.copied")
    });
  }

  autoSearchClick(event){
    if (event) {
      this.deckSearchOptions.search = event.query;
    }
    this.deckSearchOptions.pageNumber = 1;
    this.searchDecks();
  }

  searchClick(){
    this.deckSearchOptions.pageNumber = 1;
    this.searchDecks();
  }

  onNameFocus(){
    this.activeIndex = 0;
  }

  onDeckSelectFocus() {
    this.activeIndex = this.gameSetupSteps.length - 2;
  }

  deckSelected(event){
    const deckGuid = this.game.selectedDeck.guid;
    this.showCarousel = false;
    this.deckApiService.get$(deckGuid).subscribe(
      deck => {
        this.deck = deck;
        this.showCarousel = true;
      },
      error => { }
    );
    this.activeIndex = this.gameSetupSteps.length - 1;
  }

  deckCleared(event){
    this.showCarousel = false;
    this.deck = null;
  }

  joinGame (game: Game, deck: Deck){
    const playerGuid = this.isAuthenticated && this.user ? this.user.guid : Guid.create().toString();
    const playerName =  this.isAuthenticated && this.user ? encodeURIComponent(this.user.name) : this.userName;
    const baseUrl = this.apiUri + "/arcmage/Game/index.html?"
      + "gameGuid=" + game.guid + "&"
      + "deckGuid=" + deck.guid + "&"
      + "playerGuid=" + playerGuid + "&"
      + "playerName=" + playerName;

    this.started = true;
    window.open(baseUrl, "_blank");
  }

  trackByGuid(index: number, item: any): string {
    return item.guid;
  }

}
