import { Component, OnInit, ViewChild } from "@angular/core";
import { Subscription } from "rxjs";
import { ResultList } from "src/app/models/result-list";
import { SettingsOptions } from "src/app/models/settings-options";
import { User } from "src/app/models/user";
import { UserSearchOptions } from "src/app/models/user-search-options";
import { UserApiService } from "src/app/services/api/user-api.service";
import { Table } from "primeng/table";
import { LazyLoadEvent, SelectItem } from "primeng/api";

import { GlobalEventsService } from "src/app/services/global/global-events.service";

@Component({
  selector: "app-users",
  templateUrl: "./users.component.html",
  styleUrls: ["./users.component.scss"]
})
export class UsersComponent implements OnInit {

  public isAuthenticated = false;
  loading: boolean;

  private subscription: Subscription = new Subscription();

  settingsOptions: SettingsOptions;
  userSearchResult: ResultList<User>;
  userSearchOptions: UserSearchOptions;

  hideAvancedSearch = true;
  @ViewChild("usersTable") table: Table;

  constructor(private globalEventsService: GlobalEventsService, private userApiService: UserApiService) {
  }

  ngOnInit() {

    this.subscription.add(
      this.globalEventsService.isAuthenticated$.subscribe((value) => {
        if (value !== null) {
          this.isAuthenticated = value;
        }
      }));

    this.userApiService.getSettingsOptions$().subscribe( settingsOptions => {
      this.settingsOptions = settingsOptions;
    });

    this.userSearchOptions = new UserSearchOptions();
    this.userSearchOptions.pageNumber = 1;
    this.userSearchOptions.search = "";

    this.userSearchResult = new ResultList<User>();
    this.userSearchResult.items = [];
    this.userSearchResult.totalItems = 0;
  }

  searchUsers(): void {
    this.userApiService.search$(this.userSearchOptions).subscribe(
      (result) => {
        this.userSearchResult.items = result.items;
        this.userSearchResult.totalItems = result.totalItems;
      },
      (error) => {
        this.loading = false;
      },
      () => this.loading = false);
  }

  loadData(event: LazyLoadEvent) {

    if (event == null && this.table.lazy) {
      this.userSearchOptions.pageNumber = 1;
      this.userSearchOptions.pageSize = this.table.rows;
    }
    else{
      this.userSearchOptions.pageNumber = Math.floor(event.first / event.rows) + 1;
      this.userSearchOptions.pageSize = event.rows;

      if (event.sortField) {
        this.userSearchOptions.orderBy = event.sortField;
        this.userSearchOptions.reverseOrder = event.sortOrder > 0;
      }
    }
    this.searchUsers();
  }

  searchClick(){
    this.userSearchOptions.pageNumber = 1;
    this.userSearchOptions.pageSize = this.table.rows;
    this.searchUsers();
  }

 

  clear(){
    this.userSearchOptions.search = "";
    this.userSearchOptions.role = null;
    this.userSearchOptions.pageSize = this.table.rows;
    this.searchUsers();
  }

  toggleAvancedSearch() {
    this.hideAvancedSearch = !this.hideAvancedSearch;
  }

}
