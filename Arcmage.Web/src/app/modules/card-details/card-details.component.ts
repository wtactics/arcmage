import { Component, OnInit, ViewChild, OnDestroy } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { FileUpload } from "primeng/fileupload/fileupload";
import { CardApiService } from "src/app/services/api/card-api.service";
import { Card } from "src/app/models/card";
import { CardOptions } from "src/app/models/card-options";
import { FileUploadApiService } from "src/app/services/api/file-upload-api.service";
import { Observable, interval, Subscription } from "rxjs";
import { startWith, switchMap, delay } from "rxjs/operators";
import { HttpEvent, HttpEventType, HttpParams } from "@angular/common/http";
import { OverlayPanel } from "primeng/overlaypanel/overlaypanel";
import { ConfirmationService, SelectItem } from "primeng/api";
import { ConfigurationService } from "src/app/services/global/config.service";
import { Language } from "src/app/models/language";
import { Ruling } from "src/app/models/ruling";
import { RulingApiService } from "src/app/services/api/ruling-api.service";
import { TranslateService } from "@ngx-translate/core";


@Component({
  selector: "app-card-details",
  templateUrl: "./card-details.component.html",
  styleUrls: ["./card-details.component.scss"]
})
export class CardDetailsComponent implements OnInit, OnDestroy {

  apiUri: string;

  isEditMode = false;
  isEditModeKey = "card-details-isEditMode";

  card: Card;
  cardOptions: CardOptions;
  loyalties: SelectItem[];
  selectedCardInfo: string;

  rulings: Ruling[];
  showRuleCreation: boolean;
  isRuleEditing: boolean;
  newruling: Ruling;

  languages: Language[];

  loading: boolean;
  saving: boolean;

  showUploadProgress = false;
  uploadProgressValue = 0;

  cardGenerationPoll: Subscription;

  @ViewChild("fileUpload", { static: false }) fileUpload: FileUpload;
  uploadRequest: Observable<HttpEvent<any>>;

  constructor(private configurationService: ConfigurationService,
              private route: ActivatedRoute,
              private translateService: TranslateService,
              private cardApiService: CardApiService,
              private rulingApiService: RulingApiService,
              private fileUploadApiService: FileUploadApiService,
              private confirmationService: ConfirmationService) {
    this.apiUri = this.configurationService.configuration.apiUri;
    this.card = new Card();
    this.card.name = "";
    this.card.webp = "";

  }

  ngOnDestroy(): void {
    if (this.cardGenerationPoll) {
      this.cardGenerationPoll.unsubscribe();
    }
  }

  ngOnInit(): void {
    this.loading = true;
    this.newruling = new Ruling();
    this.showRuleCreation = false;


    this.route.paramMap.subscribe(params => {
      const cardGuid = params.get("cardId");

      this.cardApiService.getCardOptions(cardGuid).subscribe(
        options => {
          this.cardOptions = options;
          this.loyalties = options.loyalties.map( x => ({ label: "" + x, value: x }));
          this.cardApiService.get$(cardGuid).subscribe(
            card => {
              this.card = card;
              this.cardApiService.getRulings(cardGuid).subscribe(
                result => {
                  this.rulings = result.items;
                  this.loading = false;
                },
                error => { this.loading = false; }
              );
              this.setDefaults();
            },
            error => { this.loading = false; }
          );
        },
        error => { this.loading = false; }
      );

    });
  }

  searchLanguages(event) {
    this.languages = this.cardOptions.languages.filter(x => x.name.startsWith(event.query));
  }

  setEditMode(editMode: boolean) {
    this.isEditMode = editMode;
    sessionStorage.setItem(this.isEditModeKey, JSON.stringify(this.isEditMode));
  }

  setDefaults(){
    const storedIsEditMode = sessionStorage.getItem(this.isEditModeKey);
    if (storedIsEditMode) {
      this.isEditMode = JSON.parse(storedIsEditMode) as boolean;
    }
  }

  saveCard(forceGeneration: boolean = false) {

    const params = new HttpParams().set("forceGeneration", forceGeneration ? "true" : "false" );

    this.saving = true;

    this.card.isGenerated = false;
    if (this.cardGenerationPoll) {
      this.cardGenerationPoll.unsubscribe();
    }

    this.cardApiService.update$(this.card.guid, this.card, params).pipe(delay(500)).subscribe(
      card => {
        this.card = card;

        this.cardGenerationPoll = interval(2 * 60 * 1000).pipe(
          startWith(0),
          switchMap(() =>  this.cardApiService.get$(this.card.guid))).subscribe(
            (generatedCard) => {
              if (generatedCard.isGenerated) {
                  this.card.isGenerated = generatedCard.isGenerated;
                  this.cardGenerationPoll.unsubscribe();
              }
            },
            error => { this.cardGenerationPoll.unsubscribe(); }
          );

        this.saving = false;
      },
      error => {
        this.saving = false;
      }

    );
  }

  upload(event) {
    this.uploadRequest = this.fileUploadApiService.upload$(this.card.guid, event.files);

    if (this.uploadRequest) {
      this.showUploadProgress = true;
      this.uploadProgressValue = 0;

      this.uploadRequest
        .subscribe(
          (e) => {
            if (e.type === HttpEventType.UploadProgress) {
              this.reportProgress(e.loaded, e.total);
            }
          },
          (error) => { this.onUploadError(error); },
          () => this.onUploadComplete());
    }
  }

  reportProgress(loaded: number, total: number) {
    if (total === 0) {
      this.uploadProgressValue = 0;
    }
    else {
      this.uploadProgressValue = Math.round(loaded / total * 10000) / 100;
    }
  }

  onUploadError(error: any): void {
    this.uploadProgressValue = 0;
    this.showUploadProgress = false;
  }

  onUploadComplete(): void {
    this.uploadProgressValue = 0;
    this.showUploadProgress = false;
    this.saveCard(true);
  }

  selectCardInfo(event, infoName: string, overlaypanel: OverlayPanel) {
    this.selectedCardInfo = infoName;
    overlaypanel.toggle(event);
  }

  showEditRuling(ruling): void {
    this.newruling = ruling;
    this.newruling.card = this.card;
    this.isRuleEditing = true;
    this.showRuleCreation = true;
  }



  deleteRuling(event: Event, ruling) {
    this.confirmationService.confirm({
        target: event.target,
        acceptLabel: this.translateService.instant("global.delete"),
        rejectLabel: this.translateService.instant("global.cancel"),
        message: this.translateService.instant("rulings.delete"),
        icon: "pi pi-exclamation-triangle",
        accept: () => {
          this.rulingApiService.delete$(ruling.guid).subscribe(
            _ => {
              this.cardApiService.getRulings(this.card.guid).subscribe(
                result => {
                  this.rulings = result.items;
                  this.showRuleCreation = false;
                },
              );
            },
            error => {
              this.cardApiService.getRulings(this.card.guid).subscribe(
                result => {
                  this.rulings = result.items;
                  this.showRuleCreation = false;
                },
              );
            }
          );
        },
        reject: () => {
        }
    });
}

  showAddRuling(): void {
    this.isRuleEditing = false;
    this.newruling = new Ruling();
    this.newruling.card = this.card;
    this.showRuleCreation = true;
  }

  SaveRuling(): void {
    if (this.isRuleEditing) {
      this.rulingApiService.update$(this.newruling.guid, this.newruling).subscribe(
        ruling => {
          this.cardApiService.getRulings(this.card.guid).subscribe(
            result => {
              this.rulings = result.items;
              this.showRuleCreation = false;
            },
            error => {
              this.showRuleCreation = false;
            }
          );
        },
        error => {
          this.showRuleCreation = false;
        }
      );
    }
    else {
      this.rulingApiService.create$(this.newruling).subscribe(
        ruling => {
          this.cardApiService.getRulings(this.card.guid).subscribe(
            result => {
              this.rulings = result.items;
              this.showRuleCreation = false;
            },
            error => {
              this.showRuleCreation = false;
            }
          );
        },
        error => {
          this.showRuleCreation = false;
        }
      );
    }
  }

}
