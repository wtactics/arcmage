import { Component, OnInit, ViewChild } from "@angular/core";
import { LazyLoadEvent } from "primeng/api";
import { Table } from "primeng/table";
import { Subscription } from "rxjs";
import { ResultList } from "src/app/models/result-list";
import { SearchOptionsBase } from "src/app/models/search-options-base";
import { License } from "src/app/models/license";
import { LicenseApiService } from "src/app/services/api/license-api.service";

@Component({
  selector: "app-licenses",
  templateUrl: "./licenses.component.html",
  styleUrls: ["./licenses.component.scss"]
})
export class LicensesComponent implements OnInit {

  loading: boolean;

  isAuthenticated = false;
  subscription: Subscription = new Subscription();

  isEdit: boolean;
  showEdit: boolean;
  newLicense: License;

  licenseSearchOptions: SearchOptionsBase;
  licenseSearchResult: ResultList<License>;


  @ViewChild("licensesTable") table: Table;

  constructor(private licenseApiService: LicenseApiService) { }

  ngOnInit(): void {
    this.newLicense = new License();

    this.licenseSearchOptions = new SearchOptionsBase();
    this.licenseSearchOptions.pageNumber = 1;
    this.licenseSearchOptions.search = "";

    this.licenseSearchResult = new ResultList<License>();
    this.licenseSearchResult.items = [];
    this.licenseSearchResult.totalItems = 0;

  }

  searchLicenses(): void {
    this.licenseApiService.search$(this.licenseSearchOptions).subscribe(
      (result) => {
        this.licenseSearchResult.items = result.items;
        this.licenseSearchResult.totalItems = result.totalItems;
      },
      (error) => {
        this.loading = false;
      },
      () => this.loading = false);
  }

  loadData(event: LazyLoadEvent) {

    if (event == null && this.table.lazy) {
      this.licenseSearchOptions.pageNumber = 1;
      this.licenseSearchOptions.pageSize = this.table.rows;
    }
    else{
      this.licenseSearchOptions.pageNumber = Math.floor(event.first / event.rows) + 1;
      this.licenseSearchOptions.pageSize = event.rows;

      if (event.sortField) {
        this.licenseSearchOptions.orderBy = event.sortField;
        this.licenseSearchOptions.reverseOrder = event.sortOrder > 0;
      }
    }
    this.searchLicenses();
  }

  createLicense() {
    this.isEdit = false;
    this.newLicense = new License();
    this.showEdit = true;
  }

  showEditLicense(license: License) {
    this.isEdit = true;
    this.newLicense = license;
    this.showEdit = true;
  }

  saveLicense() {

    if (this.isEdit) {
      this.licenseApiService.update$(this.newLicense.guid, this.newLicense).subscribe(
        deck => {
          this.showEdit = false;
        },
        error => {
          this.showEdit = false;
        }
      );
    }
    else {
      this.licenseApiService.create$(this.newLicense).subscribe(
        deck => {
          this.showEdit = false;
          this.licenseSearchOptions.pageNumber = 1;
          this.searchLicenses();
        },
        error => {
          this.showEdit = false;
        }
      );
    }
  }

}
