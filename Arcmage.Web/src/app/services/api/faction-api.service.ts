import { Injectable } from "@angular/core";
import { ApiService } from './api.service';
import { Faction } from 'src/app/models/faction';

@Injectable()
export class FactionApiService extends ApiService<Faction> {
    getRoute(): string {
        return this.getNamedRoute("Faction");
    }
}
