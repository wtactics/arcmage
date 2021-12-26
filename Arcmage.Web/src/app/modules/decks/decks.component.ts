
import { LazyLoadEvent } from "primeng/api";
import { Component, OnInit } from "@angular/core";


import { Deck } from "src/app/models/deck";
import { DeckApiService } from "src/app/services/api/deck-api.service";
import { DeckSearchOptions } from "src/app/models/deck-search-options";
import { ResultList } from "src/app/models/result-list";

import { GlobalEventsService } from "src/app/services/global/global-events.service";
import { Subscription } from "rxjs";
import { Router } from "@angular/router";
import { DeckOptions } from "src/app/models/deck-options ";

@Component({
  selector: "app-decks",
  templateUrl: "./decks.component.html",
  styleUrls: ["./decks.component.scss"]
})
export class DecksComponent implements OnInit {

  deckSearchKey = "decks-deckSearchOptions";
  decksHideAdvancedSearchKey = "decks-hideAdvancedSearch";

  loading: boolean;
  enableLazyLoad = false;
  numberOfRows = 30;
  firstItem = 0;

  isAuthenticated = false;
  subscription: Subscription = new Subscription();

  showDeckCreation: boolean;
  newDeck: Deck;

  deckSearchResult: ResultList<Deck>;
  deckSearchOptions: DeckSearchOptions;
  hideAvancedSearch = false;
  deckOptions: DeckOptions;

  constructor(private globalEventsService: GlobalEventsService, private deckApiService: DeckApiService, private router: Router) { }

  ngOnInit(): void {
    this.newDeck = new Deck();

    this.deckApiService.getOptions().subscribe(deckOptions => this.deckOptions = deckOptions);


    this.deckSearchOptions = new DeckSearchOptions();
    this.deckSearchOptions.pageNumber = 1;
    this.deckSearchOptions.search = "";
    this.deckSearchOptions.myDecks = false;
    this.deckSearchOptions.excludeDrafts = true;

    this.deckSearchResult = new ResultList<Deck>();
    this.deckSearchResult.items = [];
    this.deckSearchResult.totalItems = 0;



    this.subscription.add(
      this.globalEventsService.isAuthenticated$.subscribe((value) => {
        if (value !== null) {
          this.isAuthenticated = value;
        }
      }));

      this.setDefaults();
      this.enableLazyLoad = true;
  }

  searchDecks(): void {

    sessionStorage.setItem(this.deckSearchKey, JSON.stringify(this.deckSearchOptions));

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
    if (this.enableLazyLoad) {
      this.deckSearchOptions.pageNumber = Math.floor(event.first / event.rows) + 1;
      this.deckSearchOptions.pageSize = event.rows;

      if (event.sortField) {
        this.deckSearchOptions.orderBy = event.sortField;
        this.deckSearchOptions.reverseOrder = event.sortOrder > 0;
      }
      this.searchDecks();
    }
  }

  searchClick(){
    this.deckSearchOptions.pageNumber = 1;
    this.deckSearchOptions.pageSize = this.numberOfRows;
    this.searchDecks();
  }

  createDeck() {
    this.newDeck = new Deck();
    this.newDeck.status = this.deckOptions.statuses[0];
    this.showDeckCreation = true;
  }

  clear(){
    this.deckSearchOptions.search = "";
    this.deckSearchOptions.status = null;
    this.deckSearchOptions.myDecks = false;
    this.deckSearchOptions.excludeDrafts = true;
    this.deckSearchOptions.pageSize = this.numberOfRows;
    this.searchDecks();
  }

  setDefaults(){
    const storedDeckSearchOptions = sessionStorage.getItem(this.deckSearchKey);
    if (storedDeckSearchOptions) {
      this.deckSearchOptions = JSON.parse(storedDeckSearchOptions) as DeckSearchOptions;
      this.firstItem = (this.deckSearchOptions.pageNumber-1) * this.deckSearchOptions.pageSize
    }
    const storedHideAdvancedSearch = sessionStorage.getItem(this.decksHideAdvancedSearchKey);
    if (storedHideAdvancedSearch) {
      this.hideAvancedSearch = JSON.parse(storedHideAdvancedSearch) as boolean;
    }
  }

  toggleAvancedSearch() {
    this.hideAvancedSearch = !this.hideAvancedSearch;
    sessionStorage.setItem(this.decksHideAdvancedSearchKey, JSON.stringify(this.hideAvancedSearch));
  }

  saveDeck() {
    this.deckApiService.create$(this.newDeck).subscribe(
      deck => {
        this.showDeckCreation = false;
        this.router.navigate(["/decks", deck.guid]);
      },
      error => {
        this.showDeckCreation = false;
      }

    );
  }


}
