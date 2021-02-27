﻿import { Component, OnInit } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";

import { localStorageKeys } from "../../global/localStorage.keys";

import { GlobalEventsService } from "src/app/services/global/global-events.service";
import { LoginApiService } from "src/app/services/api/login-api.service";
import { UserApiService } from "src/app/services/api/user-api.service";
import { md5 } from "src/app/services/global/md5";



@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.sass"]
})
export class LoginComponent implements OnInit {

  username: string;
  password: string;

  returnUrl: string;
  loginErrorMessage: string;

  loading = false;

  constructor(
      private route: ActivatedRoute,
      private router: Router,
      private globalEventsService: GlobalEventsService,
      private loginApiService: LoginApiService,
      private userApiService: UserApiService
  ) {
      // redirect to home if already logged in
      if (this.globalEventsService.getUser()) {
          this.router.navigate(["/"]);
      }
  }

  ngOnInit() {
      // get return url from route parameters or default to '/'
      // tslint:disable-next-line:no-string-literal
      this.returnUrl = this.globalEventsService.getPreviousUrl() || "/";
  }


  submitLogin() {
    if (this.username == null || this.username === "" || this.password == null || this.password === "") {
      return;
    }
    this.loading = true;
    const pass = md5(this.password);
    this.login(this.username, pass);
  }

  login(email: string, password: string) {
    this.loginErrorMessage = null;
    this.loginApiService.signIn(email, password).subscribe( token => {
      localStorage.setItem(localStorageKeys.token, token);
      this.userApiService.me().subscribe(
        user => {
          this.globalEventsService.setUser(user);
          this.router.navigate([this.returnUrl]);
        },
        error => this.loginError(error));
    },
    error => {
      this.loginError(error);
    });
  }

  loginError (error: any) {
    this.loginErrorMessage = error.error.message;
    this.loading = false;
  }

}
