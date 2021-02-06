
import { Table } from "primeng/table";
import { LazyLoadEvent, SelectItem } from "primeng/api";
import { Component, OnInit, ViewChild, AfterViewInit, ViewEncapsulation } from "@angular/core";


import { Card } from "src/app/models/card";
import { CardApiService } from "src/app/services/api/card-api.service";
import { CardSearchOptions } from "src/app/models/card-search-options";
import { ResultList } from "src/app/models/result-list";
import { CardOptions } from "src/app/models/card-options";

import { OverlayPanel } from "primeng/overlaypanel";
import { GlobalEventsService } from "src/app/services/global/global-events.service";
import { Subscription } from "rxjs";
import { Router } from "@angular/router";
import { SlickCarouselComponent } from "ngx-slick-carousel";
import { ConfigurationService } from "src/app/services/global/config.service";

@Component({
  selector: "app-cards",
  templateUrl: "./cards.component.html",
  styleUrls: ["./cards.component.scss"],
})
export class CardsComponent implements OnInit, AfterViewInit {

  apiUri: string;

  loading: boolean;

  isAuthenticated = false;
  subscription: Subscription = new Subscription();

  hideAvancedSearch = true;
  showCardCreation: boolean;
  newCard: Card;

  slideConfig: any;
  showCarousel = false;

  cardSearchResult: ResultList<Card>;
  cardSearchOptions: CardSearchOptions;
  cardOptions: CardOptions;
  loyalties: SelectItem[];

  selectedCard: Card;

  @ViewChild("cardsTable") table: Table;

  constructor(private configurationService: ConfigurationService,
              private globalEventsService: GlobalEventsService,
              private cardApiService: CardApiService,
              private router: Router) {
    this.apiUri = this.configurationService.configuration.apiUri;
    this.slideConfig = this.configurationService.configuration.slideConfig;
  }

  ngAfterViewInit(): void {
  }

  ngOnInit(): void {

    this.newCard = new Card();

    this.cardApiService.getOptions().subscribe(cardOptions => {
      this.cardOptions = cardOptions;
      this.loyalties = cardOptions.loyalties.map( x => ({ label: "" + x, value: x }));
    });

    this.cardSearchOptions = new CardSearchOptions();
    this.cardSearchOptions.pageNumber = 1;
    this.cardSearchOptions.search = "";

    this.cardSearchResult = new ResultList<Card>();
    this.cardSearchResult.items = [];
    this.cardSearchResult.totalItems = 0;

    this.subscription.add(
      this.globalEventsService.isAuthenticated$.subscribe((value) => {
        if (value !== null) {
          this.isAuthenticated = value;
        }
      }));

  }

  searchCards(): void {
    this.showCarousel = false;
    this.cardApiService.search$(this.cardSearchOptions).subscribe(
      (result) => {
        this.cardSearchResult.items = result.items;
        this.cardSearchResult.totalItems = result.totalItems;
        this.showCarousel = true;
      },
      (error) => {
        this.loading = false;
      },
      () => this.loading = false);
  }

  loadData(event: LazyLoadEvent) {

    if (event == null && this.table.lazy) {
      this.cardSearchOptions.pageNumber = 1;
      this.cardSearchOptions.pageSize = this.table.rows;
    }
    else{
      this.cardSearchOptions.pageNumber = Math.floor(event.first / event.rows) + 1;
      this.cardSearchOptions.pageSize = event.rows;

      if (event.sortField) {
        this.cardSearchOptions.orderBy = event.sortField;
        this.cardSearchOptions.reverseOrder = event.sortOrder > 0;
      }
    }
    this.searchCards();
  }

  searchClick(){
    this.cardSearchOptions.pageNumber = 1;
    this.cardSearchOptions.pageSize = this.table.rows;
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
    this.cardSearchOptions.pageSize = this.table.rows;
    this.searchCards();
  }

  toggleAvancedSearch() {
    this.hideAvancedSearch = !this.hideAvancedSearch;
  }

  createCard() {
    this.newCard = new Card();
    this.newCard.serie = this.cardOptions.series[this.cardOptions.series.length - 1];
    this.newCard.ruleSet = this.cardOptions.ruleSets[this.cardOptions.ruleSets.length - 1];
    this.newCard.status = this.cardOptions.statuses[0];
    this.newCard.faction = this.cardOptions.factions[1];
    this.newCard.type = this.cardOptions.cardTypes[1];
    this.newCard.info = "arcmage.org - join us!";
    this.showCardCreation = true;
  }

  saveCard() {

    this.cardApiService.create$(this.newCard).subscribe(
      card => {
        this.showCardCreation = false;
        this.router.navigate(["/cards", card.guid]);
      },
      error => {
        this.showCardCreation = false;
      }

    );
  }

}
