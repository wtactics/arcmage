import { Component, OnInit } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { localStorageKeys } from "./../../../../global/localStorage.keys";
import { UserApiService } from "src/app/services/api/user-api.service";
import { GlobalEventsService } from "src/app/services/global/global-events.service";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"]
})
export class AppComponent implements OnInit {


  constructor(private translateService: TranslateService, private userApiService: UserApiService, private globalEventsService: GlobalEventsService) {

    const defaultLang = "en";
    this.translateService.setDefaultLang(defaultLang);

    const initialLanguage = localStorage.getItem(localStorageKeys.language) || defaultLang;
    this.translateService.use(initialLanguage);

    this.translateService.onLangChange.subscribe(() => {
      localStorage.setItem(localStorageKeys.language, this.translateService.currentLang);
    });

  }

  loginError (error: any) {

  }

  ngOnInit(): void {
    if (localStorage.getItem(localStorageKeys.token)){
      this.userApiService.me().subscribe(
        user => {
          this.globalEventsService.setUser(user);
        },
        error => this.loginError(error));
    }
  }
}
