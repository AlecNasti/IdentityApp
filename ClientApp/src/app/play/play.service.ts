import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { enviroment } from 'src/enviroments/enviroment.development';

@Injectable({
  providedIn: 'root'
})
export class PlayService {

  constructor(private http: HttpClient) { }

  getPlayers() {
    return this.http.get(`${enviroment.appUrl}/api/play/get-players`);
  }

}
