
import { Table } from "primeng/table";
import { LazyLoadEvent } from "primeng/api";
import { Component, OnInit, ViewChild, AfterViewInit } from "@angular/core";


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

  loading: boolean;

  isAuthenticated = false;
  subscription: Subscription = new Subscription();

  showDeckCreation: boolean;
  newDeck: Deck;

  deckSearchResult: ResultList<Deck>;
  deckSearchOptions: DeckSearchOptions;
  hideAvancedSearch = false;
  deckOptions: DeckOptions;

  @ViewChild("decksTable") table: Table;

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
      this.deckSearchOptions.pageNumber = 1;
      this.deckSearchOptions.pageSize = this.table.rows;
    }
    else{
      this.deckSearchOptions.pageNumber = Math.floor(event.first / event.rows) + 1;
      this.deckSearchOptions.pageSize = event.rows;

      if (event.sortField) {
        this.deckSearchOptions.orderBy = event.sortField;
        this.deckSearchOptions.reverseOrder = event.sortOrder > 0;
      }
    }
    this.searchDecks();
  }

  searchClick(){
    this.deckSearchOptions.pageNumber = 1;
    this.deckSearchOptions.pageSize = this.table.rows;
    this.searchDecks();
  }

  createDeck() {
    this.newDeck = new Deck();
    this.newDeck.status = this.deckOptions.statuses[0];
    this.showDeckCreation = true;
  }

  toggleAvancedSearch() {
    this.hideAvancedSearch = !this.hideAvancedSearch;
  }
  clear(){
    this.deckSearchOptions.search = "";
    this.deckSearchOptions.status = null;
    this.deckSearchOptions.myDecks = false;
    this.deckSearchOptions.excludeDrafts = true;
    this.deckSearchOptions.pageSize = this.table.rows;
    this.searchDecks();
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
