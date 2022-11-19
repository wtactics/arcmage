import { AfterViewInit, Component, OnDestroy, OnInit } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { ActivatedRoute } from "@angular/router";
import { TranslateService } from "@ngx-translate/core";
import { MessageService } from "primeng/api";
import { Subscription } from "rxjs";
import { SettingsOptions } from "src/app/models/settings-options";
import { User } from "src/app/models/user";
import { UserApiService } from "src/app/services/api/user-api.service";
import { GlobalEventsService } from "src/app/services/global/global-events.service";

@Component({
  selector: "app-settings",
  templateUrl: "./settings.component.html",
  styleUrls: ["./settings.component.scss"]
})
export class SettingsComponent implements OnInit, OnDestroy {

  user: User = new User();
  saveErrorMessage: string;
  settingsOptions: SettingsOptions;
  userId: any;

  loading = false;
  subscription: Subscription = new Subscription();

  constructor(
    private route: ActivatedRoute,
    private globalEventsService: GlobalEventsService,
    private messageService: MessageService,
    private translateService: TranslateService,
    private userApiService: UserApiService,
    private titleService: Title
  ) { }

  get isSavePlayerDisabled(): boolean{
    if (this.user.name == null || this.user.name === "") { return true; }
    return false;
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.loading = true;
      this.userId = params.get("userId");
      this.userApiService.getSettingsOptions$().subscribe( settingsOptions => {
        this.settingsOptions = settingsOptions;
        this.userApiService.get$(this.userId).subscribe(
          user => {
            this.user = user;
            this.loading = false;
            this.titleService.setTitle('Aminduna - ' + this.translateService.instant('menu.settings') + ' - ' + user.name );
          },
          error => {
            this.messageService.add({
              severity: "error",
              detail: this.translateService.instant("settings.user-not-found")
            });
            this.loading = false;
          }
        );
      },
      error => {
        this.messageService.add({
          severity: "error",
          detail: this.translateService.instant("settings.settings-options-error")
        });
        this.loading = false;
      });
    });

  }

  savePlayer() {
    if (this.isSavePlayerDisabled) { return; }

    this.loading = true;
    this.saveErrorMessage = null;
    this.userApiService.update$(this.user.guid, this.user).subscribe( user => {
      this.messageService.add({
        severity: "success",
        detail: this.translateService.instant("settings.saved")
      });
      this.user = user;
      if (this.globalEventsService.getUser()?.guid === this.userId) {
        this.globalEventsService.setUser(user);
      }
      this.loading = false;
    },
    error => {
      this.registerError(error);
    });
  }

  registerError (error: any) {
    this.saveErrorMessage = error.error;
    this.loading = false;
  }

  requestPasswordReset() {
    if (this.user?.email == null || this.user.email === "") {
      return;
    }
    const user = new User();
    user.email = this.user.email;

    this.userApiService.requestPasswordReset$(user).subscribe( _ => {
      this.messageService.add({
        severity: "success",
        detail: this.translateService.instant("settings.password-forget.email-sent")
      });
    },
    error => {
      this.messageService.add({
        severity: "error",
        detail: this.translateService.instant("settings.password-forget.failed")
      });
    });
  }


  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

}
