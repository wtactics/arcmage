import { Injectable } from "@angular/core";

import { ActivatedRouteSnapshot, RouterStateSnapshot, Router } from "@angular/router";

import { Observable } from "rxjs";
import { GlobalEventsService } from "../global/global-events.service";

@Injectable()
export class AuthGuardService  {

  constructor(private globalEventsService: GlobalEventsService, private router: Router) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | Observable<boolean> | Promise<boolean> {
    const isAuthenticated = this.globalEventsService.isAuthenticated.getValue();
    let isMe = false;

    if (!isAuthenticated) {
      return false;
    }

    if (route.data.checkUserId) {
      const userId = route.paramMap.get("userId");
      const currentUser = this.globalEventsService.getUser();
      if ( userId && currentUser ) {
        isMe = currentUser.guid === userId;
      }
    }

    const requiredUserRight = route.data.requiredUserRight;
    return isMe || this.globalEventsService.hasRight(requiredUserRight);
  }

}
