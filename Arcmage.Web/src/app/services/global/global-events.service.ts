import { Injectable } from "@angular/core";
import { Observable, BehaviorSubject } from "rxjs";

import { localStorageKeys } from "../../global/localStorage.keys";
import { Router, RouterEvent, NavigationEnd } from "@angular/router";
import { User } from "src/app/models/user";
import { Right } from "src/app/models/right";

@Injectable()
export class GlobalEventsService {

  public isAuthenticated: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(null);
  public isAuthenticated$: Observable<boolean> = this.isAuthenticated.asObservable();

  public currentUser: BehaviorSubject<User> = new BehaviorSubject<User>(null);
  public currentUser$: Observable<User> = this.currentUser.asObservable();

  private previousUrl: string = undefined;
  private currentUrl: string = undefined;

  constructor(private router: Router) {
    this.currentUrl = this.router.url;
    router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.previousUrl = this.currentUrl;
        this.currentUrl = event.url;
      }
    });
  }

  public hasRight(rightName: string): boolean {
    const user = this.getUser();
    if (user && user.rights && user.rights.length !== 0) {
      return user.rights.map(function(x) {return x.name; }).indexOf(rightName) > -1;
    }
    return false;
  }

  public getPreviousUrl() {
    return this.previousUrl;
  }


  setUser(user: User) {
    if (user) {
      this.currentUser.next(user);
      this.isAuthenticated.next(true);
    }
  }

  getUser(): User {
    return this.currentUser.value;
  }

  logout() {
    localStorage.removeItem(localStorageKeys.token);
    this.isAuthenticated.next(false);
    this.currentUser.next(null);
    window.location.reload();
  }

}
