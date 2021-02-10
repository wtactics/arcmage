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
import { SelectItem } from "primeng/api";
import { ConfigurationService } from "src/app/services/global/config.service";
import { Language } from "src/app/models/language";


@Component({
  selector: "app-card-details",
  templateUrl: "./card-details.component.html",
  styleUrls: ["./card-details.component.scss"]
})
export class CardDetailsComponent implements OnInit, OnDestroy {

  apiUri: string;

  card: Card;
  cardOptions: CardOptions;
  loyalties: SelectItem[];
  selectedCardInfo: string;

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
              private cardApiService: CardApiService,
              private fileUploadApiService: FileUploadApiService) {
    this.apiUri = this.configurationService.configuration.apiUri;
    this.card = new Card();
    this.card.name = "";
    this.card.jpeg = "";

  }

  ngOnDestroy(): void {
    if (this.cardGenerationPoll) {
      this.cardGenerationPoll.unsubscribe();
    }
  }

  ngOnInit(): void {
    this.loading = true;
    this.route.paramMap.subscribe(params => {
      const cardGuid = params.get("cardId");

      this.cardApiService.getCardOptions(cardGuid).subscribe(
        options => {
          this.cardOptions = options;
          this.loyalties = options.loyalties.map( x => ({ label: "" + x, value: x }));
          this.cardApiService.get$(cardGuid).subscribe(
            card => {
              this.card = card;
              this.loading = false;
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

  saveCard(forceGeneration: boolean = false) {
    // tslint:disable-next-line:object-literal-key-quotes
    const data = { "forceGeneration": forceGeneration };

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
    this.fileUpload.clear();
  }

  onUploadComplete(): void {
    this.uploadProgressValue = 0;
    this.showUploadProgress = false;
    this.fileUpload.clear();
    this.saveCard(true);
  }

  selectCardInfo(event, infoName: string, overlaypanel: OverlayPanel) {
    this.selectedCardInfo = infoName;
    overlaypanel.toggle(event);
  }

}
