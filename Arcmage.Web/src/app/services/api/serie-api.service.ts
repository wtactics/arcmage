import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { Serie } from "src/app/models/serie";

@Injectable()
export class SerieApiService extends ApiService<Serie> {
    getRoute(): string {
        return this.getNamedRoute("Serie");
    }

    getSearchRoute(): string {
        return this.getNamedRoute("SeriesSearch");
    }
}
