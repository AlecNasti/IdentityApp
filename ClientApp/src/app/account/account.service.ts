import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Register } from '../shared/Models/register';
import { enviroment } from 'src/enviroments/enviroment.development';
import { Login } from '../shared/Models/Login';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private http: HttpClient) { }

  login(model: Login){
    return this.http.post(`${enviroment.appUrl}/api/account/login`, model);
  }
  
  register(model: Register){
    return this.http.post(`${enviroment.appUrl}/api/account/register`, model);
  }
}
