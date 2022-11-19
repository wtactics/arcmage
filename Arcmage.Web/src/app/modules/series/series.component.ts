import { Component, OnInit, ViewChild } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { TranslateService } from "@ngx-translate/core";
import { LazyLoadEvent } from "primeng/api";
import { Table } from "primeng/table";
import { Subscription } from "rxjs";
import { ResultList } from "src/app/models/result-list";
import { SearchOptionsBase } from "src/app/models/search-options-base";
import { Serie } from "src/app/models/serie";
import { SerieApiService } from "src/app/services/api/serie-api.service";

@Component({
  selector: "app-series",
  templateUrl: "./series.component.html",
  styleUrls: ["./series.component.scss"]
})
export class SeriesComponent implements OnInit {

  loading: boolean;

  isAuthenticated = false;
  subscription: Subscription = new Subscription();

  isEdit: boolean;
  showEdit: boolean;
  newSerie: Serie;

  serieSearchOptions: SearchOptionsBase;
  serieSearchResult: ResultList<Serie>;


  @ViewChild("seriesTable") table: Table;

  constructor(
    private serieApiService: SerieApiService,
    private titleService: Title,
    private translateService: TranslateService
  ) { }

  ngOnInit(): void {
    this.newSerie = new Serie();

    this.serieSearchOptions = new SearchOptionsBase();
    this.serieSearchOptions.pageNumber = 1;
    this.serieSearchOptions.search = "";

    this.serieSearchResult = new ResultList<Serie>();
    this.serieSearchResult.items = [];
    this.serieSearchResult.totalItems = 0;

    this.titleService.setTitle('Aminduna - ' + this.translateService.instant('menu.series'));
    this.translateService.onLangChange.subscribe(() => {
      this.titleService.setTitle('Aminduna - ' + this.translateService.instant('menu.series'));
    });

  }

  searchSeries(): void {
    this.serieApiService.search$(this.serieSearchOptions).subscribe(
      (result) => {
        this.serieSearchResult.items = result.items;
        this.serieSearchResult.totalItems = result.totalItems;
      },
      (error) => {
        this.loading = false;
      },
      () => this.loading = false);
  }

  loadData(event: LazyLoadEvent) {

    if (event == null && this.table.lazy) {
      this.serieSearchOptions.pageNumber = 1;
      this.serieSearchOptions.pageSize = this.table.rows;
    }
    else{
      this.serieSearchOptions.pageNumber = Math.floor(event.first / event.rows) + 1;
      this.serieSearchOptions.pageSize = event.rows;

      if (event.sortField) {
        this.serieSearchOptions.orderBy = event.sortField;
        this.serieSearchOptions.reverseOrder = event.sortOrder > 0;
      }
    }
    this.searchSeries();
  }

  createSerie() {
    this.isEdit = false;
    this.newSerie = new Serie();
    this.showEdit = true;
  }

  showEditSerie(serie: Serie) {
    this.isEdit = true;
    this.newSerie = serie;
    this.showEdit = true;
  }

  saveSerie() {

    if (this.isEdit) {
      this.serieApiService.update$(this.newSerie.guid, this.newSerie).subscribe(
        deck => {
          this.showEdit = false;
        },
        error => {
          this.showEdit = false;
        }
      );
    }
    else {
      this.serieApiService.create$(this.newSerie).subscribe(
        deck => {
          this.showEdit = false;
          this.serieSearchOptions.pageNumber = 1;
          this.searchSeries();
        },
        error => {
          this.showEdit = false;
        }
      );
    }
  }

}
