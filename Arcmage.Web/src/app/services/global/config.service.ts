import { Injectable } from "@angular/core";
import { HttpClient, HttpBackend } from "@angular/common/http";
import { Configuration } from "./configuration";
import { environment } from "../../../environments/environment";

@Injectable({
    providedIn: "root"
})
export class ConfigurationService {

    private httpClient: HttpClient;

    public configuration: Configuration;

    constructor(handler: HttpBackend) {
        // avoid interceptors
        this.httpClient = new HttpClient(handler);
    }

    public async loadConfiguration(): Promise<any> {
        return this.httpClient.get<Configuration>(environment.configUrl)
            .toPromise()
            .then(configuration => {
                this.configuration = configuration;
            });
    }

}
