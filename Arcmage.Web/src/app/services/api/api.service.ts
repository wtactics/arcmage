// Auto-Generation: disable
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { HttpClient, HttpParams} from "@angular/common/http";

import { ConfigurationService } from "../global/config.service";
import { Configuration } from "../global/configuration";
import { Routes } from "src/app/models/routes";
import { ResultList } from "src/app/models/result-list";


@Injectable()
export class ApiService<T> {

    public configuration: Configuration;

    constructor(public http: HttpClient, private configurationService: ConfigurationService) {
        this.configuration = this.configurationService.configuration;
    }

    getBaseUrl(): string {
        return `${this.configuration.apiUri}`;
    }

    getUrl(): string {
        return `${this.getBaseUrl()}/${this.getRoute()}`;
    }

    getRoute(): string {
        return "";
    }

    getSearchRoute(): string{
        return "";
    }

    getNamedRoute(name: string): string {
        return Routes.routeMap.get(name);
    }

    search$(options: any): Observable<ResultList<T>> {
        const url = `${this.getBaseUrl()}/${this.getSearchRoute()}`;
        return this.http.post<ResultList<T>>(url, options);
    }

    get$(id: any): Observable<T> {
        const url = `${this.getUrl()}/${id}`;
        return this.http.get<T>(url);
    }

    create$(model: T): Observable<T> {
        return this.http.post<T>(this.getUrl(), model);
    }

    update$(id: any, model: T, params: HttpParams = new HttpParams()): Observable<T> {
        const url = `${this.getUrl()}/${id}`;
        return this.http.patch<T>(url, model, {params});
    }

    delete$(id: any): Observable<T> {
        const url = `${this.getUrl()}/${id}`;

        return this.http.delete<T>(url);
    }

}
