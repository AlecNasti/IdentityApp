import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable, map } from 'rxjs';
import { AccountService } from 'src/app/account/account.service';
import { SharedService } from '../shared.service';
import { User } from '../Models/User';

@Injectable({
  providedIn: 'root'
})
export class AuthorizationGuard {

  constructor(private accountService: AccountService,
    private sharedService: SharedService,
    private router: Router) { }
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> {
    return this.accountService.user$.pipe(
      map((user: User | null) => {
        if (user) {
          return true;
        } else {
          this.sharedService.showNotification(false, 'Restriced Area', 'Please, login to proceed');
          this.router.navigate(['account/login'], { queryParams: { returnUrl: state.url } })
          return false;
        }
      })
    );
  }

}
