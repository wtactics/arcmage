import { Component, OnInit } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { ActivatedRoute } from "@angular/router";
import { TranslateService } from "@ngx-translate/core";
import { localStorageKeys } from "src/app/global/localStorage.keys";
import { User } from "src/app/models/user";
import { UserApiService } from "src/app/services/api/user-api.service";
import { GlobalEventsService } from "src/app/services/global/global-events.service";
import { md5 } from "src/app/services/global/md5";

@Component({
  selector: "app-password-reset",
  templateUrl: "./password-reset.component.html",
  styleUrls: ["./password-reset.component.scss"]
})
export class PasswordResetComponent implements OnInit {

  resetErrorMessage: string;
  loading: boolean;
  resetFailed: boolean;
  resetSuccess: boolean;
  user: User;

  constructor(
    private route: ActivatedRoute, 
    private globalEventsService: GlobalEventsService, 
    private userApiService: UserApiService,
    private titleService: Title,
    private translateService: TranslateService) { }

  ngOnInit(): void {

    this.titleService.setTitle('Aminduna');
    this.translateService.onLangChange.subscribe(() => {
      this.titleService.setTitle('Aminduna');
    });

    const token: string = this.route.snapshot.queryParamMap.get("token");
    if (token) {

      // storing verify token
      localStorage.setItem(localStorageKeys.token, token);
      this.userApiService.me().subscribe(
        user => {
          this.user = user;
        },
        error => { this.resetFailed = true; }
      );
    }
    else {
      this.resetFailed = true;
    }
  }

  get isPasswordResetDisabled(): boolean{
    if (this.user.password == null || this.user.password === "") { return true; }
    if (this.user.password2 == null || this.user.password2 === "") { return true; }
    if (this.user.password !== this.user.password2) { return true; }
    return false;
  }

  submitPasswordReset() {
    if (this.isPasswordResetDisabled) { return; }

    this.user.password = md5(this.user.password);
    this.user.password2 = md5(this.user.password2);
    this.loading = true;

    this.userApiService.passwordReset$(this.user).subscribe( newUser => {
      localStorage.setItem(localStorageKeys.token, newUser.token);
      this.globalEventsService.setUser(newUser);
      this.resetSuccess = true;
      this.loading = false;
    },
    error => {
      this.resetError(error);
    });
  }

  resetError (error: any) {
    this.resetErrorMessage = error.error;
    this.loading = false;
  }

}
