import { LazyLoadEvent, SelectItem } from "primeng/api";
import { Component, OnInit, ViewChild } from "@angular/core";
import { Paginator } from 'primeng/paginator';

import { Card } from "src/app/models/card";
import { CardApiService } from "src/app/services/api/card-api.service";
import { CardSearchOptions } from "src/app/models/card-search-options";
import { ResultList } from "src/app/models/result-list";
import { CardOptions } from "src/app/models/card-options";

import { OverlayPanel } from "primeng/overlaypanel";
import { GlobalEventsService } from "src/app/services/global/global-events.service";
import { Subscription } from "rxjs";
import { Router } from "@angular/router";
import { ConfigurationService } from "src/app/services/global/config.service";
import { Language } from "src/app/models/language";
import { Title } from "@angular/platform-browser";
import { TranslateService } from "@ngx-translate/core";

@Component({
  selector: "app-cards",
  templateUrl: "./cards.component.html",
  styleUrls: ["./cards.component.scss"],
})
export class CardsComponent implements OnInit {


  @ViewChild('paginator', { static: true }) paginator: Paginator

  apiUri: string;

  cardsSearchKey = "cards-cardSearchOptions";
  cardsHideAdvancedSearchKey = "cards-hideAdvancedSearch";
  

  loading: boolean;
  enableLazyLoad = false;
  numberOfRows = 30;
  firstItem = 0;

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

  languages: Language[];

  selectedCard: Card;

  constructor(private configurationService: ConfigurationService,
              private globalEventsService: GlobalEventsService,
              private cardApiService: CardApiService,
              private router: Router,
              private titleService: Title,
              private translateService: TranslateService) {
    this.apiUri = this.configurationService.configuration.apiUri;
    this.slideConfig = this.configurationService.configuration.slideConfig;
  }

  ngOnInit(): void {

    this.newCard = new Card();

    this.cardSearchOptions = new CardSearchOptions();
    this.cardSearchOptions.pageNumber = 1;
    this.cardSearchOptions.search = "";
    this.cardSearchOptions.pageSize = this.numberOfRows;

    this.cardSearchResult = new ResultList<Card>();
    this.cardSearchResult.items = [];
    this.cardSearchResult.totalItems = 0;
    
    this.titleService.setTitle('Aminduna - ' + this.translateService.instant('menu.cards'));
    this.translateService.onLangChange.subscribe(() => {
      this.titleService.setTitle('Aminduna - ' + this.translateService.instant('menu.cards'));
    });

    this.cardApiService.getOptions().subscribe(cardOptions => {
      this.cardOptions = cardOptions;
      this.loyalties = cardOptions.loyalties.map( x => ({ label: "" + x, value: x }));
      this.languages = cardOptions.languages;
      this.cardSearchOptions.language = this.languages.find(x=>x.languageCode === "en");
      this.cardSearchOptions.status = this.cardOptions.statuses.find(x=>x.guid === "7dedc883-5dd2-5f17-b2a4-eaf04f7ad464")   
      this.setDefaults();
      this.searchCards();
      this.enableLazyLoad = true;
    });

   
    this.subscription.add(
      this.globalEventsService.isAuthenticated$.subscribe((value) => {
        if (value !== null) {
          this.isAuthenticated = value;
        }
      }));

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
    this.showCarousel = false;

    sessionStorage.setItem(this.cardsSearchKey, JSON.stringify(this.cardSearchOptions));

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
    if (this.enableLazyLoad) {
      if (event != null) {
        this.cardSearchOptions.pageNumber = Math.floor(event.first / event.rows) + 1;
        this.cardSearchOptions.pageSize = event.rows;
  
        if (event.sortField) {
          this.cardSearchOptions.orderBy = event.sortField;
          this.cardSearchOptions.reverseOrder = event.sortOrder > 0;
        }
      }
      this.searchCards();
    }
  }

  searchClick(){
    this.cardSearchOptions.pageNumber = 1;
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
    this.cardSearchOptions.language = this.languages.find(x=>x.languageCode === "en");
    this.cardSearchOptions.status = this.cardOptions.statuses.find(x=>x.guid === "7dedc883-5dd2-5f17-b2a4-eaf04f7ad464")   
    this.searchCards();
  }

  setDefaults(){
    const storedCardSearchOptions = sessionStorage.getItem(this.cardsSearchKey);
    if (storedCardSearchOptions) {
      this.cardSearchOptions = JSON.parse(storedCardSearchOptions) as CardSearchOptions;
      this.firstItem = (this.cardSearchOptions.pageNumber-1) * this.cardSearchOptions.pageSize
    }
    const storedHideAdvancedSearch = sessionStorage.getItem(this.cardsHideAdvancedSearchKey);
    if (storedHideAdvancedSearch) {
      this.hideAvancedSearch = JSON.parse(storedHideAdvancedSearch) as boolean;
    }
  }

  toggleAvancedSearch() {
    this.hideAvancedSearch = !this.hideAvancedSearch;
    sessionStorage.setItem(this.cardsHideAdvancedSearchKey, JSON.stringify(this.hideAvancedSearch));
  }

  createCard() {
    this.newCard = new Card();
    this.newCard.serie = this.cardOptions.series[this.cardOptions.series.length - 1];
    this.newCard.ruleSet = this.cardOptions.ruleSets[this.cardOptions.ruleSets.length - 1];
    this.newCard.status = this.cardOptions.statuses[0];
    this.newCard.faction = this.cardOptions.factions[1];
    this.newCard.type = this.cardOptions.cardTypes[1];
    this.newCard.language = { name: "English", languageCode : "en"};
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
