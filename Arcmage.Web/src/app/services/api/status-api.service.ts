import { Injectable } from "@angular/core";
import { ApiService } from './api.service';
import { Status } from 'src/app/models/status';

@Injectable()
export class StatusApiService extends ApiService<Status> {
    getRoute(): string {
        return this.getNamedRoute("Status");
    }
}
