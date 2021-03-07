import { Injectable } from "@angular/core";
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from "@angular/common/http";
import { Observable } from "rxjs";

import { GlobalEventsService } from "./global-events.service";
import { localStorageKeys } from "src/app/global/localStorage.keys";
import { ConfigurationService } from "./config.service";

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    constructor(private globalEventsService: GlobalEventsService, private configurationService: ConfigurationService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // add auth header with jwt if user is logged in and request is to the api url
        const currentUser = this.globalEventsService.getUser();
        const token =  localStorage.getItem(localStorageKeys.token);
        const apiUri = this.configurationService?.configuration?.apiUri;

        if (apiUri){
            const isLoggedIn = token;
            const isApiUrl = request.url.startsWith(apiUri);
            if (isLoggedIn && isApiUrl) {
                request = request.clone({
                    setHeaders: {
                        Authorization: `Bearer ${token}`
                    }
                });
            }
        }
        return next.handle(request);
    }
}