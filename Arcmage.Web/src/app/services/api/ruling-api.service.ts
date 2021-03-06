import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { Ruling } from "src/app/models/ruling";

@Injectable()
export class RulingApiService extends ApiService<Ruling> {
    getRoute(): string {
        return this.getNamedRoute("Ruling");
    }
}
