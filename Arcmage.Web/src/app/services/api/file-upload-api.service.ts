import { Injectable } from "@angular/core";
import { HttpEvent, HttpRequest, HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Routes } from "src/app/models/routes";
import { Configuration } from "../global/Configuration";
import { ConfigurationService } from "../global/config.service";

@Injectable()
export class FileUploadApiService {

    public configuration: Configuration;

    constructor(public http: HttpClient, private configurationService: ConfigurationService) {
        this.configuration = this.configurationService.configuration;
    }

    getUrl(): string {
        return `${this.configuration.apiUri}/${this.getRoute()}`;
    }

    getRoute(): string {
        return Routes.routeMap.get("FileUpload");
    }

    upload$(id: any, files): Observable<HttpEvent<any>> {
        const url = `${this.getUrl()}/${id}`;

        const formData = new FormData();

        for (let i = 0; i < files.length; i++) {
            formData.append(`file${i}`, files[i], files[i].name);
        }

        return this.http.request(new HttpRequest(
            "POST",
            url,
            formData,
            {
                reportProgress: true
            }));
    }
}
