import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { Table } from "primeng/table";
import { LazyLoadEvent, SelectItem } from "primeng/api";
import { OverlayPanel } from "primeng/overlaypanel";

import { DeckApiService } from "src/app/services/api/deck-api.service";
import { Deck } from "src/app/models/deck";

import { Card } from "src/app/models/card";
import { CardApiService } from "src/app/services/api/card-api.service";
import { CardSearchOptions } from "src/app/models/card-search-options";
import { ResultList } from "src/app/models/result-list";
import { CardOptions } from "src/app/models/card-options";
import { DeckCard } from "src/app/models/deck-card";
import { DeckCardApiService } from "src/app/services/api/deck-card-api.service";
import { Observable, interval, Subscription } from "rxjs";
import { startWith, switchMap, delay } from "rxjs/operators";
import { DeckOptions } from "src/app/models/deck-options ";
import { ConfigurationService } from "src/app/services/global/config.service";
import { Language } from "src/app/models/language";
import { Title } from "@angular/platform-browser";

@Component({
  selector: "app-deck-details",
  templateUrl: "./deck-details.component.html",
  styleUrls: ["./deck-details.component.scss"]
})
export class DeckDetailsComponent implements OnInit {

  apiUri: string;

  loading: boolean;
  saving: boolean;
  enableLazyLoad = false;
  numberOfRows = 30;
  firstItem = 0;

  deck: Deck;

  cardSearchResult: ResultList<Card>;
  cardSearchOptions: CardSearchOptions;
  cardOptions: CardOptions;
  loyalties: SelectItem[];
  languages: Language[];
  hideAvancedSearch = false;

  selectedCard: Card;

  deckOptions: DeckOptions;

  deckGenerationPoll: Subscription;

  slideConfig: any;

  showDeckSettings: boolean;

  constructor(private configurationService: ConfigurationService,
              private route: ActivatedRoute,
              private titleService: Title,
              private deckApiService: DeckApiService,
              private deckCardApiService: DeckCardApiService,
              private cardApiService: CardApiService) {
    this.apiUri = this.configurationService.configuration.apiUri;
    this.slideConfig = this.configurationService.configuration.slideConfig;
    this.deck = new Deck();
  }

  ngOnInit(): void {
    this.loading = true;
    this.route.paramMap.subscribe(params => {
      const deckGuid = params.get("deckId");
      this.deckApiService.getDeckOptions(deckGuid).subscribe(deckOptions => this.deckOptions = deckOptions);
      this.deckApiService.get$(deckGuid).subscribe(
        deck => {
          this.deck = deck;
          this.titleService.setTitle('Aminduna - ' + deck.name);
          this.loading = false;
        },
        error => { this.loading = false; }
      );
    });

    this.cardApiService.getOptions().subscribe(cardOptions => {
      this.cardOptions = cardOptions;
      this.loyalties = cardOptions.loyalties.map( x => ({ label: "" + x, value: x }));
      this.languages = cardOptions.languages;
      this.cardSearchOptions.language = this.languages.find(x=>x.languageCode === "en");
      this.cardSearchOptions.status = this.cardOptions.statuses.find(x=>x.guid === "7dedc883-5dd2-5f17-b2a4-eaf04f7ad464");
    });

    this.cardSearchOptions = new CardSearchOptions();
    this.cardSearchOptions.pageNumber = 1;
    this.cardSearchOptions.search = "";

    this.cardSearchResult = new ResultList<Card>();
    this.cardSearchResult.items = [];
    this.cardSearchResult.totalItems = 0;

  }

  searchLanguage($event): void {
    if ($event) {
      this.languages = this.cardOptions.languages.filter( language => {
        if (language && language.name) {
          return language.name.contains($event.query);
        }
        return false;
      });
    }
    else {
      this.languages = this.cardOptions.languages;
    }
  }

  searchCards(): void {
    this.cardApiService.search$(this.cardSearchOptions).subscribe(
      (result) => {
        this.cardSearchResult.items = result.items;
        this.cardSearchResult.totalItems = result.totalItems;
      },
      (error) => {
        this.loading = false;
      },
      () => this.loading = false);
  }

  loadData(event: LazyLoadEvent) {

    if (this.enableLazyLoad) {
      this.cardSearchOptions.pageNumber = Math.floor(event.first / event.rows) + 1;
      this.cardSearchOptions.pageSize = event.rows;

      if (event.sortField) {
        this.cardSearchOptions.orderBy = event.sortField;
        this.cardSearchOptions.reverseOrder = event.sortOrder > 0;
      }
      this.searchCards();
    }
  }

  searchClick(){
    this.enableLazyLoad = true;
    this.cardSearchOptions.pageNumber = 1;
    this.cardSearchOptions.pageSize = this.numberOfRows;
    this.searchCards();
  }

  selectCard(event, card: Card, overlaypanel: OverlayPanel) {
    this.selectedCard = card;
    overlaypanel.toggle(event);
  }

  clear(){
    this.cardSearchOptions.search = "";
    this.cardSearchOptions.faction = null;
    this.cardSearchOptions.cardType = null;
    this.cardSearchOptions.loyalty = null;
    this.cardSearchOptions.cost = null;
    this.cardSearchOptions.serie = null;
    this.cardSearchOptions.status = null;
    this.cardSearchOptions.pageSize = this.numberOfRows;
    this.cardSearchOptions.language = this.languages.find(x=>x.languageCode === "en");
    this.cardSearchOptions.status = this.cardOptions.statuses.find(x=>x.guid === "7dedc883-5dd2-5f17-b2a4-eaf04f7ad464");
    this.searchCards();
  }

  toggleAvancedSearch() {
    this.hideAvancedSearch = !this.hideAvancedSearch;
  }

  increaseDeckCard (card: Card) {
    let deckCard = this.deck.deckCards.find(dc => dc.card.guid === card.guid);

    if (!deckCard) {
        deckCard = new DeckCard();
        deckCard.card = card;
        deckCard.quantity = 0;
        this.deck.deckCards.push(deckCard);
    }
    deckCard.quantity++;
    this.saveDeckCard(deckCard);
  }

  saveDeckCard (deckCard: DeckCard) {
    this.loading = true;
    deckCard.deck = new Deck();
    deckCard.deck.guid = this.deck.guid;

    this.deckCardApiService.create$(deckCard).subscribe(
      (dc) => {
        if (deckCard.quantity <= 0){
          const index: number = this.deck.deckCards.indexOf(deckCard);
          if (index !== -1) {
            this.deck.deckCards.splice(index, 1);
          }
        }
        this.loading = false;
      },
      (error) => {
        this.loading = false;
        const er = error;
      });
  }

  decreaseDeckCard (card: Card) {
    const deckCard = this.deck.deckCards.find(dc => dc.card.guid === card.guid);

    if (deckCard) {
        deckCard.quantity--;
        this.saveDeckCard(deckCard);
    }
  }

  editDeckSettings(){
    this.showDeckSettings = true;
  }

  saveDeck() {
    const deckUpdate = new Deck();
    deckUpdate.guid = this.deck.guid;
    deckUpdate.name = this.deck.name;
    deckUpdate.exportTiles = this.deck.exportTiles;
    deckUpdate.generatePdf = this.deck.generatePdf;
    deckUpdate.status = this.deck.status;

    this.saving = true;

    this.deck.isGenerated = false;
    if (this.deckGenerationPoll) {
      this.deckGenerationPoll.unsubscribe();
    }

    this.deckApiService.update$(this.deck.guid, deckUpdate).pipe(delay(500)).subscribe(
      deck => {
        this.deck = deck;

        this.deckGenerationPoll = interval(2 * 60 * 1000).pipe(
          startWith(0),
          switchMap(() =>  this.deckApiService.get$(this.deck.guid))).subscribe(
            (generatedDeck) => {
              if (generatedDeck.isGenerated) {
                  this.deck.isGenerated = deck.isGenerated;
                  this.deckGenerationPoll.unsubscribe();
              }
            },
            error => { this.deckGenerationPoll.unsubscribe(); }
          );
        this.showDeckSettings = false;
        this.saving = false;
      },
      error => {
        this.saving = false;
        this.showDeckSettings = false;
      }

    );

  }

}
