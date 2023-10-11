import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Register } from '../shared/Models/register';
import { enviroment } from 'src/enviroments/enviroment.development';
import { Login } from '../shared/Models/Login';
import { User } from '../shared/Models/User';
import { ReplaySubject, map, of } from 'rxjs';
import { Router } from '@angular/router';

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

  logout() {
    localStorage.removeItem(enviroment.userKey);
    this.userSource.next(null);
    this.router.navigateByUrl('/');
  }

  register(model: Register) {
    return this.http.post(`${enviroment.appUrl}/api/account/register`, model);
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
