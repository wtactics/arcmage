import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { DeckCard } from "src/app/models/deck-card";

@Injectable()
export class DeckCardApiService extends ApiService<DeckCard> {
    getRoute(): string {
        return this.getNamedRoute("DeckCard");
    }
}
