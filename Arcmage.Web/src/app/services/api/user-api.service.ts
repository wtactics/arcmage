import { Injectable } from "@angular/core";
import { ApiService } from './api.service';
import { User } from 'src/app/models/user';
import { Observable } from 'rxjs';

@Injectable()
export class UserApiService extends ApiService<User> {
    getRoute(): string {
        return this.getNamedRoute("User");
    }

    me(): Observable<User> {
        return this.get$("me");
    }
}
