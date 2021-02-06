import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { Deck } from "src/app/models/deck";
import { Observable } from "rxjs";
import { DeckOptions } from "src/app/models/deck-options ";

@Injectable()
export class DeckApiService extends ApiService<Deck> {
    getRoute(): string {
        return this.getNamedRoute("Deck");
    }

    getSearchRoute(): string {
        return this.getNamedRoute("DeckSearch");
    }

    getOptions(): Observable<DeckOptions> {
        const url = `${this.configuration.apiUri}/${this.getNamedRoute("DeckOptions")}`;
        return this.http.get<DeckOptions>(url);
    }

    getDeckOptions(id: any): Observable<DeckOptions> {
        const url = `${this.configuration.apiUri}/${this.getNamedRoute("DeckOptions")}/${id}`;
        return this.http.get<DeckOptions>(url);
    }
}
