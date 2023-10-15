import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Register } from '../shared/Models/account/register';
import { enviroment } from 'src/enviroments/enviroment.development';
import { Login } from '../shared/Models/account/Login';
import { User } from '../shared/Models/account/User';
import { ReplaySubject, map, of } from 'rxjs';
import { Router } from '@angular/router';
import { ConfirmEmail } from '../shared/Models/account/ConfirmEmail';
import { ResetPassword } from '../shared/Models/account/resetPassword';
import { RegisterWithExternal } from '../shared/Models/account/registerWithExternal';
import { LoginWithExternal } from '../shared/Models/account/loginWithExternal';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private userSource = new ReplaySubject<User | null>(1);
  user$ = this.userSource.asObservable();

  constructor(private http: HttpClient,
    private router: Router) { }

  refreshUser(jwt: string | null) {
    if (jwt === null) {
      this.userSource.next(null);
      return of(undefined);
    }

    let headers = new HttpHeaders();
    headers = headers.set('Authorization', 'Bearer ' + jwt);
    return this.http.get<User>(`${enviroment.appUrl}/api/account/refresh-user-token`, { headers }).pipe(
      map((user: User) => {
        if (user) {
          this.setUser(user);
        }
      })
    )
  }

  login(model: Login) {
    return this.http.post<User>(`${enviroment.appUrl}/api/account/login`, model).pipe(
      map((user: User) => {
        if (user) {
          this.setUser(user);
          //return user; dentro il mapping bisogno ritornare l'oggetto oppure lo perdi e ti ritorna NULL
        }
        //return null; in questo caso utilizzeremo l'oggetto $user senza ritornarlo in quando verrà popolato dal metodo.
      })
    );
  }

  loginWithThirdParty(model: LoginWithExternal) {
    return this.http.post<User>(`${enviroment.appUrl}/api/account/login-with-third-party`, model).pipe(
      map((user: User) => {
        if (user) {
          this.setUser(user);
        }
      })
    )
  }

  logout() {
    localStorage.removeItem(enviroment.userKey);
    this.userSource.next(null);
    this.router.navigateByUrl('/');
  }

  register(model: Register) {
    return this.http.post(`${enviroment.appUrl}/api/account/register`, model);
  }

  registerWithThirdParty(model: RegisterWithExternal) {
    return this.http.post<User>(`${enviroment.appUrl}/api/account/register-with-third-party`, model).pipe(
      map((user: User) => {
        if (user) {
          this.setUser(user);
        }
      })
    );
  }

  getJWT() {
    const key = localStorage.getItem(enviroment.userKey);
    if (key) {
      const user: User = JSON.parse(key);
      return user.jwt
    } else {
      return null
    }
  }

  ConfirmEmail(model: ConfirmEmail){
    return this.http.put(`${enviroment.appUrl}/api/account/confirm-email`, model);
  }

  resendEmailConfirmationLink(email: string){
    return this.http.post(`${enviroment.appUrl}/api/account/resend-email-confirmation-link/${email}`, {});
  }

  forgotUsernameOrPassword(email: string){
    return this.http.post(`${enviroment.appUrl}/api/account/forgot-username-or-password/${email}`, {});
  }

  resetPassword(model: ResetPassword){
    return this.http.put(`${enviroment.appUrl}/api/account/reset-password`, model);
  }

  //salviamo l'utente dentro la memoria locale di angular per poi riutilizzarlo come "observable" quando ne abbiamo bisogno
  private setUser(user: User) {
    localStorage.setItem(enviroment.userKey, JSON.stringify(user));
    this.userSource.next(user);

    //    this.user$.subscribe({
    //       next: response => console.log(response)
    //     }
    //    )
  }
}
