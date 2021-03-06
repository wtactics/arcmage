import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { localStorageKeys } from "src/app/global/localStorage.keys";
import { UserApiService } from "src/app/services/api/user-api.service";
import { GlobalEventsService } from "src/app/services/global/global-events.service";

@Component({
  selector: "app-confirm",
  templateUrl: "./confirm.component.html",
  styleUrls: ["./confirm.component.scss"]
})
export class ConfirmComponent implements OnInit {

  verificationSuccess: boolean;
  verificationFailed: boolean;
  userName: string;

  constructor(private route: ActivatedRoute, private globalEventsService: GlobalEventsService, private userApiService: UserApiService) { }

  ngOnInit(): void {
    const token: string = this.route.snapshot.queryParamMap.get("token");
    if (token) {

      // storing verify token
      localStorage.setItem(localStorageKeys.token, token);

      // verify
      this.userApiService.verify$().subscribe( user => {
        localStorage.setItem(localStorageKeys.token, user.token);
        this.userName = user.name;
        this.globalEventsService.setUser(user);
        this.verificationSuccess = true;
      },
      error => {
        localStorage.removeItem(localStorageKeys.token);
        this.verificationFailed = true;
      });
    }
    else {
      this.verificationFailed = true;
    }
  }

}
