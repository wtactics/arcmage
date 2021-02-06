import { Injectable } from "@angular/core";
import { ApiService } from './api.service';
import { Login } from 'src/app/models/login';
import { Observable } from 'rxjs';


@Injectable()
export class LoginApiService extends ApiService<Login> {
    getRoute(): string {
        return this.getNamedRoute("Login");
    }

    signIn(email: string, password: string): Observable<string> {
        return this.http.post<string>(this.getUrl(), {'email' :email, 'password': password});
    }
}
