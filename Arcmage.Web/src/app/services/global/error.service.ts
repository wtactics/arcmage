import { Injectable } from "@angular/core";
import { MessageService } from "primeng/api";
import { TranslateService } from "@ngx-translate/core";
import { Observable, Subject } from "rxjs";

@Injectable()
export class ErrorService {

    public httpError: Subject<number> = new Subject<number>();
    public httpError$: Observable<number> = this.httpError.asObservable();

    public errorMessage: Subject<string> = new Subject<string>();
    public errorMessage$: Observable<string> = this.errorMessage.asObservable();

    private lastErrorCode = -1;
    private lastErrorTime = -1;

    constructor(
        private messageService: MessageService,
        private translateService: TranslateService) {
    }

    showHttpError(httpStatusCode: number, message: any) {
        const now = new Date().getTime();

        if (this.lastErrorCode >= 0 && this.lastErrorTime > 0) {
            // don't show the same error messages in a timespan of 2 seconds
            if (httpStatusCode === this.lastErrorCode && (now - this.lastErrorTime) <= 2000) {
                return;
            }
        }

        this.lastErrorCode = httpStatusCode;
        this.lastErrorTime = now;

        const key = `apiError.${httpStatusCode}`;
        let errorMessage = this.translateService.instant(key);

        if (message && typeof message === "string") {
            // check if we can translate the provided message
            const detailedMessage = this.translateService.instant(message.toString());
            if (detailedMessage !== message) {
                errorMessage = detailedMessage;
            }
        }

        // server down and we don't have error translations yet
        if (httpStatusCode === 0 && key === errorMessage) {
            errorMessage = "Er is momenteel geen connectie met de server.";
        }

        this.httpError.next(httpStatusCode);
        this.errorMessage.next(errorMessage);

        this.messageService.add({
            severity: "error",
            detail: errorMessage
        });
    }

    clear() {
        this.httpError.next(null);
    }
}
