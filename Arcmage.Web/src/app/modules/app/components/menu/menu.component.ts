import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { Subscription } from "rxjs";



import { GlobalEventsService } from "./../../../../services/global/global-events.service";
import { Configuration } from "../../../../services/global/configuration";
import { Router, ActivatedRoute } from "@angular/router";


@Component({
  selector: "app-menu",
  templateUrl: "./menu.component.html",
  styleUrls: ["./menu.component.scss"]
})
export class MenuComponent implements OnInit, OnDestroy {


  public isAuthenticated = false;
  public userName = null;

  public configuration: Configuration;

  private subscription: Subscription = new Subscription();

  returnUrl: string;

  constructor(  private router: Router, private route: ActivatedRoute, private globalEventsService: GlobalEventsService) {
  }

  ngOnInit() {

    this.subscription.add(
      this.globalEventsService.isAuthenticated$.subscribe((value) => {
        if (value !== null) {
          this.isAuthenticated = value;
        }
      }));

    this.subscription.add(
      this.globalEventsService.currentUser$.subscribe((value) => {
        if (value !== null) {
          this.userName = value.name;
        }
      }));

    // tslint:disable-next-line:no-string-literal
    this.returnUrl = this.globalEventsService.getPreviousUrl() || "/";

  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  public logout() {
    this.globalEventsService.logout();
  }

}
