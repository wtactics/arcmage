import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { License } from "src/app/models/license";

@Injectable()
export class LicenseApiService extends ApiService<License> {
    getRoute(): string {
        return this.getNamedRoute("License");
    }

    getSearchRoute(): string {
        return this.getNamedRoute("LicenseSearch");
    }
}
