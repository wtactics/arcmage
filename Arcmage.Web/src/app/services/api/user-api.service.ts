import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { User } from "src/app/models/user";
import { Observable } from "rxjs";
import { SettingsOptions } from "src/app/models/settings-options";

@Injectable()
export class UserApiService extends ApiService<User> {
    getRoute(): string {
        return this.getNamedRoute("User");
    }

    me(): Observable<User> {
        return this.get$("me");
    }

    verify$(): Observable<User> {
        const url = `${this.getUrl()}/email/verify`;
        return this.http.get<User>(url);
    }

    requestPasswordReset$(user: User): Observable<any> {
        const url = `${this.getUrl()}/request/password/reset`;
        return this.http.post<User>(url, user);
    }

    passwordReset$(user: User): Observable<User> {
        const url = `${this.getUrl()}/password/reset`;
        return this.http.post<User>(url, user);
    }

    getSettingsOptions$(): Observable<SettingsOptions> {
        const url = `${this.configuration.apiUri}/${this.getNamedRoute("SettingsOptions")}`;
        return this.http.get<SettingsOptions>(url);
    }

}
