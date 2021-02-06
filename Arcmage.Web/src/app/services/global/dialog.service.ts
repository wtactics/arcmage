import { Injectable } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { Observable, Observer } from "rxjs";
import { ConfirmationService } from "primeng/api";

@Injectable()
export class DialogService {

    constructor(
        private confirmationService: ConfirmationService,
        private translateService: TranslateService) {
    }

    confirmCancel(callback: () => void) {
        this.confirmationService.confirm({
            header: this.translateService.instant("dialogs.cancel.title"),
            message: this.translateService.instant("dialogs.cancel.message"),
            icon: "fa fa-trash",
            acceptVisible: true,
            rejectVisible: true,
            accept: () => callback()
        });
    }

    confirmDelete(callback: () => void, header: string = null, message: string = null) {
        this.confirmationService.confirm({
            header: this.translateService.instant(header ? header : "dialogs.delete.title"),
            message: this.translateService.instant(message ? message : "dialogs.delete.message"),
            icon: "fa fa-trash",
            acceptVisible: true,
            rejectVisible: true,
            accept: () => callback()
        });
    }

    confirmDeleteAll(callback: () => void) {
        this.confirmationService.confirm({
            header: this.translateService.instant("dialogs.delete.all.title"),
            message: this.translateService.instant("dialogs.delete.all.message"),
            icon: "fa fa-trash",
            acceptVisible: true,
            rejectVisible: true,
            accept: () => callback()
        });
    }

    prompt(headerKey: string, messageKey: string, accept: () => void, reject: () => void = null) {
        this.confirmationService.confirm({
            header: this.translateService.instant(headerKey),
            message: this.translateService.instant(messageKey),
            icon: "fa fa-question-circle",
            acceptVisible: true,
            rejectVisible: true,
            accept: () => accept(),
            reject: () => reject()
        });
    }

    promptText(headerText: string, messageText: string, accept: () => void, reject: () => void = null) {
        this.confirmationService.confirm({
            header: headerText,
            message: messageText,
            icon: "fa fa-question-circle",
            acceptVisible: true,
            rejectVisible: true,
            accept: () => accept(),
            reject: () => reject()
        });
    }

    showMessage(headerKey: string, messageKey: string) {
        this.confirmationService.confirm({
            header: this.translateService.instant(headerKey),
            message: this.translateService.instant(messageKey),
            icon: "fa fa-info-circle",
            acceptVisible: true,
            rejectVisible: false,
            acceptLabel: this.translateService.instant("form.ok")
        });
    }

    showMessageText(headerText: string, messageText: string) {
        this.confirmationService.confirm({
            header: headerText,
            message: messageText,
            icon: "fa fa-info-circle",
            acceptVisible: true,
            rejectVisible: false,
            acceptLabel: this.translateService.instant("form.ok")
        });
    }

    confirmLeavePage(): Observable<boolean> {
        return Observable.create((observer: Observer<boolean>) => {
            this.confirmationService.confirm({
                header: this.translateService.instant("general.confirmLeavePage.header"),
                message: this.translateService.instant("general.confirmLeavePage.message"),
                icon: "fa fa-question-circle",
                acceptVisible: true,
                rejectVisible: true,
                acceptLabel: this.translateService.instant("dialogs.reject"),
                rejectLabel: this.translateService.instant("dialogs.accept"),
                accept: () => {
                    observer.next(false);
                    observer.complete();
                },
                reject: () => {
                    observer.next(true);
                    observer.complete();
                }
            });
        });
    }

}
