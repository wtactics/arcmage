import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { Card } from "src/app/models/card";
import { CardOptions } from "src/app/models/card-options";
import { Observable } from "rxjs";

@Injectable()
export class CardApiService extends ApiService<Card> {
    getRoute(): string {
        return this.getNamedRoute("Card");
    }

    getSearchRoute(): string {
        return this.getNamedRoute("CardSearch");
    }

    getOptions(): Observable<CardOptions> {
        const url = `${this.configuration.apiUri}/${this.getNamedRoute("CardOptions")}`;
        return this.http.get<CardOptions>(url);
    }

    getCardOptions(id: any): Observable<CardOptions> {
        const url = `${this.configuration.apiUri}/${this.getNamedRoute("CardOptions")}/${id}`;
        return this.http.get<CardOptions>(url);
    }

}
