import { FormsModule } from "@angular/forms";
import { TableModule } from "primeng/table";
import { DialogModule } from "primeng/dialog";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { TriStateCheckboxModule } from "primeng/tristatecheckbox";
import { FileUploadModule} from "primeng/fileupload";
import { DropdownModule } from "primeng/dropdown";
import { ToastModule } from "primeng/toast";
import { ChartModule } from "primeng/chart";
import { MessageService } from "primeng/api";
import { MessagesModule } from "primeng/messages";
import { ProgressBarModule } from "primeng/progressbar";
import { InputTextModule } from "primeng/inputtext";
import { ButtonModule } from "primeng/button";
import { OverlayPanelModule } from "primeng/overlaypanel";
import { SidebarModule } from "primeng/sidebar";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TabViewModule } from "primeng/tabview";
import { MessageModule } from "primeng/message";
import { StepsModule } from "primeng/steps";
import { PanelModule } from "primeng/panel";
import { InputSwitchModule } from "primeng/inputswitch";
import { CheckboxModule } from "primeng/checkbox";
import { AutoCompleteModule } from "primeng/autocomplete";
import { BadgeModule } from "primeng/badge";
import { DividerModule } from "primeng/divider";
import { ConfirmPopupModule } from "primeng/confirmpopup";
import { ConfirmationService } from "primeng/api";

import { SlickCarouselModule } from "ngx-slick-carousel";



import { NgModule, APP_INITIALIZER } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { HttpClientModule, HttpClient } from "@angular/common/http";
import { HTTP_INTERCEPTORS } from "@angular/common/http";
import { LOCALE_ID } from "@angular/core";
import { registerLocaleData, LocationStrategy, HashLocationStrategy } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";
import { ClipboardModule } from "@angular/cdk/clipboard";

import localeNLBE from "@angular/common/locales/nl-BE";
import { TranslateModule, TranslateLoader, TranslateService } from "@ngx-translate/core";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";


import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./modules/app/components/app/app.component";
import { MenuComponent } from "./modules/app/components/menu/menu.component";
import { UnauthorizedComponent } from "./modules/app/components/unauthorized/unauthorized.component";


import { CardsComponent } from "./modules/cards/cards.component";
import { CardDetailsComponent } from "./modules/card-details/card-details.component";
import { DecksComponent } from "./modules/decks/decks.component";

import { GlobalEventsService } from "./services/global/global-events.service";
import { ErrorService } from "./services/global/error.service";
import { DialogService } from "./services/global/dialog.service";
import { ConfigurationService } from "./services/global/config.service";



// import { DefaultInterceptor } from "./services/api/interceptors/default.interceptor";
// import { JsonInterceptor } from "./services/api/interceptors/json.interceptor";
// import { HttpErrorInterceptor } from "./services/api/interceptors/http-error.interceptor";
// import { ContentLanguageInterceptor } from "./services/api/interceptors/content-language.interceptor";
import { JwtInterceptor } from "./services/global/jwt.interceptor";
import { ApiService } from "./services/api/api.service";
import { CardApiService } from "./services/api/card-api.service";
import { DeckApiService } from "./services/api/deck-api.service";
import { DeckCardApiService } from "./services/api/deck-card-api.service";
import { RulingApiService } from "./services/api/ruling-api.service";
import { GameApiService } from "./services/api/game-api.service";


import { SvgIconComponent } from "./modules/components/svg-icon/svg-icon.component";
import { LoginComponent } from "./modules/login/login.component";
import { RegisterComponent } from "./modules/register/register.component";
import { LoginApiService } from "./services/api/login-api.service";
import { UserApiService } from "./services/api/user-api.service";
import { FileUploadApiService } from "./services/api/file-upload-api.service";
import { DeckDetailsComponent } from "./modules/deck-details/deck-details.component";
import { GamesComponent } from "./modules/games/games.component";
import { GameInviteComponent } from "./modules/game-invite/game-invite.component";
import { ConfirmComponent } from "./modules/confirm/confirm.component";
import { PasswordResetComponent } from "./modules/password-reset/password-reset.component";
import { PasswordForgetComponent } from "./modules/password-forget/password-forget.component";
import { SettingsComponent } from "./modules/settings/settings.component";
import { UsersComponent } from "./modules/users/users.component";


registerLocaleData(localeNLBE);

export function HttpLoaderFactory(http: HttpClient, configService: ConfigurationService) {
  const config = configService.configuration;
  return new TranslateHttpLoader(http, "/assets/i18n/", `.json?ts=${Date.now()}`);
}

@NgModule({
  declarations: [
    AppComponent,
    MenuComponent,
    UnauthorizedComponent,
    CardsComponent,
    SvgIconComponent,
    LoginComponent,
    RegisterComponent,
    CardDetailsComponent,
    DecksComponent,
    DeckDetailsComponent,
    GamesComponent,
    GameInviteComponent,
    ConfirmComponent,
    PasswordResetComponent,
    PasswordForgetComponent,
    SettingsComponent,
    UsersComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    FormsModule,
    AppRoutingModule,
    HttpClientModule,
    ToastModule,
    DialogModule,
    InputTextModule,
    ButtonModule,
    OverlayPanelModule,
    SidebarModule,
    ProgressSpinnerModule,
    SlickCarouselModule,
    ConfirmDialogModule,
    TableModule,
    ChartModule,
    MessagesModule,
    MessageModule,
    StepsModule,
    PanelModule,
    InputSwitchModule,
    CheckboxModule,
    TriStateCheckboxModule,
    AutoCompleteModule,
    BadgeModule,
    DividerModule,
    ConfirmPopupModule,
    DropdownModule,
    FileUploadModule,
    ProgressBarModule,
    TabViewModule,
    ClipboardModule,
    TranslateModule.forRoot(
      {
        defaultLanguage: "en",
        loader: {
          provide: TranslateLoader,
          useFactory: HttpLoaderFactory,
          deps: [HttpClient, ConfigurationService]
        }
      }
    ),
  ],
  providers: [
    { provide: LocationStrategy, useClass: HashLocationStrategy },
    {
      provide: APP_INITIALIZER,
      useFactory: (configService: ConfigurationService) => () => configService.loadConfiguration(),
      deps: [ConfigurationService, TranslateService],
      multi: true
    },
    {
      provide: LOCALE_ID, useValue: "en"
    },
    GlobalEventsService,
    ApiService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true
    },
    ErrorService,
    DialogService,
    MessageService,
    CardApiService,
    DeckApiService,
    DeckCardApiService,
    RulingApiService,
    GameApiService,
    LoginApiService,
    UserApiService,
    FileUploadApiService,
    ConfirmationService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
