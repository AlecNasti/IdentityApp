import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { component } from 'vue/types/umd';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';

const ruotes: Routes = [
  {path: 'login', component: LoginComponent},
  {path: 'register', component: RegisterComponent}
]

@NgModule({
  declarations: [],
  imports: [
    RouterModule.forChild(ruotes) 
  ],
  exports:[
    RouterModule
  ]
})
export class AccountRoutingModule { }
