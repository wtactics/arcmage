﻿import { Component, OnInit } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

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
  loginForm: FormGroup;
  loading = false;
  submitted = false;
  returnUrl: string;
  loginErrorMessage: string;

  constructor(
      private formBuilder: FormBuilder,
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
      this.loginForm = this.formBuilder.group({
          username: ["", Validators.required],
          password: ["", Validators.required]
      });

      // get return url from route parameters or default to '/'
      // tslint:disable-next-line:no-string-literal
      this.returnUrl = this.globalEventsService.getPreviousUrl() || "/";
  }

  // convenience getter for easy access to form fields
  get f() { return this.loginForm.controls; }

  onSubmit() {
      this.submitted = true;
      // stop here if form is invalid
      if (this.loginForm.invalid) { return; }

      this.loading = true;
      const pass = md5(this.f.password.value);

      this.login(this.f.username.value, pass);
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
