import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { Game } from "src/app/models/game";

@Injectable()
export class GameApiService extends ApiService<Game> {

    getBaseUrl(): string {
        return `${this.configuration.gameApiUri}`;
    }

    getRoute(): string {
        return this.getNamedRoute("Game");
    }

    getSearchRoute(): string {
        return this.getNamedRoute("GameSearch");
    }
}
