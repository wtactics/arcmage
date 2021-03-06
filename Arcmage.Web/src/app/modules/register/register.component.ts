import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { User } from "src/app/models/user";
import { LoginApiService } from "src/app/services/api/login-api.service";
import { UserApiService } from "src/app/services/api/user-api.service";
import { GlobalEventsService } from "src/app/services/global/global-events.service";

import { localStorageKeys } from "../../global/localStorage.keys";
import { md5 } from "src/app/services/global/md5";

@Component({
  selector: "app-register",
  templateUrl: "./register.component.html",
  styleUrls: ["./register.component.sass"]
})
export class RegisterComponent implements OnInit {

  user: User;

  returnUrl: string;
  registerErrorMessage: string;

  loading = false;
  showEmailValidation = false;

  constructor(
    private router: Router,
    private globalEventsService: GlobalEventsService,
    private loginApiService: LoginApiService,
    private userApiService: UserApiService) {
      // redirect to home if already logged in
      if (this.globalEventsService.getUser()) {
        this.router.navigate(["/"]);
    }
  }

  get isRegistrationDisabled(): boolean{
    if (this.user.name == null || this.user.name === "") { return true; }
    if (this.user.email == null || this.user.email === "") { return true; }
    if (this.user.password == null || this.user.password === "") { return true; }
    if (this.user.password2 == null || this.user.password2 === "") { return true; }
    if (this.user.password !== this.user.password2) { return true; }
    return false;
  }

  ngOnInit(): void {
    this.user = new User();
    this.showEmailValidation = false;
     // get return url from route parameters or default to '/'
    // tslint:disable-next-line:no-string-literal
    this.returnUrl = this.globalEventsService.getPreviousUrl() || "/";
  }

  submitRegistration() {
    if (this.isRegistrationDisabled) { return; }


    this.loading = true;
    const newUser = new User();
    newUser.name = this.user.name;
    newUser.email = this.user.email;
    newUser.password = md5(this.user.password);
    newUser.password2 = md5(this.user.password2);
    this.register(newUser);
  }

  // Register and login
  register(user: User) {
    this.registerErrorMessage = null;
    this.userApiService.create$(user).subscribe( newUser => {
      this.showEmailValidation = true;
    },
    error => {
      this.registerError(error);
    });
  }

  registerError (error: any) {
    this.registerErrorMessage = error.error;
    this.loading = false;
  }

}
