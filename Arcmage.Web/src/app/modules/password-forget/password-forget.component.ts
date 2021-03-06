import { Component, OnInit } from "@angular/core";
import { User } from "src/app/models/user";
import { UserApiService } from "src/app/services/api/user-api.service";

@Component({
  selector: "app-password-forget",
  templateUrl: "./password-forget.component.html",
  styleUrls: ["./password-forget.component.scss"]
})
export class PasswordForgetComponent implements OnInit {

  email: string;

  passwordResetErrorMessage: string;

  loading = false;
  passwordResetEmailSent = false;

  constructor( private userApiService: UserApiService) { }

  ngOnInit(): void {
  }

  requestPasswordReset() {
    if (this.email == null || this.email === "") {
      return;
    }
    this.loading = true;
    this.passwordResetErrorMessage = null;
    const user = new User();
    user.email = this.email;

    this.userApiService.requestPasswordReset$(user).subscribe( _ => {
      this.passwordResetEmailSent = true;
      this.loading = false;
    },
    error => {
      this.resetRequestError(error);
    });
  }

  resetRequestError (error: any) {
    this.passwordResetErrorMessage = error.error;
    this.loading = false;
  }

}
