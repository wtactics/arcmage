import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { Subscription } from "rxjs";

import { GlobalEventsService } from "./../../../../services/global/global-events.service";
import { Configuration } from "../../../../services/global/configuration";
import { Router, ActivatedRoute } from "@angular/router";
import { SelectItem } from "primeng/api";
import { User } from "src/app/models/user";


@Component({
  selector: "app-menu",
  templateUrl: "./menu.component.html",
  styleUrls: ["./menu.component.scss"]
})
export class MenuComponent implements OnInit, OnDestroy {


  public isAuthenticated = false;
  public user: User;

  public configuration: Configuration;

  private subscription: Subscription = new Subscription();

  returnUrl: string;
  languages: SelectItem[];

  constructor(    public translate: TranslateService,  private router: Router, private route: ActivatedRoute, private globalEventsService: GlobalEventsService) {
  }

  ngOnInit() {

    this.languages = [{ label: "En", value : "en"}, {label: "Nl", value: "nl"}, {label: "Fr", value: "fr"}, {label: "Es", value: "es"}, {label: "Pt-Br", value: "pt-br"}];

    this.subscription.add(
      this.globalEventsService.isAuthenticated$.subscribe((value) => {
        if (value !== null) {
          this.isAuthenticated = value;
        }
      }));

    this.subscription.add(
      this.globalEventsService.currentUser$.subscribe((user) => {
        if (user !== null) {
          this.user = user;
        }
      }));

    // tslint:disable-next-line:no-string-literal
    this.returnUrl = this.globalEventsService.getPreviousUrl() || "/";

  }

  switchLang(lang: string) {
    this.translate.use(lang);
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  public logout() {
    this.globalEventsService.logout();
  }

}
