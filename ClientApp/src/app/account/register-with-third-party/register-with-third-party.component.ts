import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs';
import { User } from 'src/app/shared/Models/account/User';
import { RegisterWithExternal } from 'src/app/shared/Models/account/registerWithExternal';
import { AccountService } from '../account.service';

@Component({
  selector: 'app-register-with-third-party',
  templateUrl: './register-with-third-party.component.html',
  styleUrls: ['./register-with-third-party.component.css']
})
export class RegisterWithThirdPartyComponent {
  registerForm: FormGroup = new FormGroup({});
  submitted = false;
  provider: string | null = null;
  access_token: string | null = null;
  userId: string | null = null;
  email: string | null = null;
  errorMessages: string[] = [];

  constructor(private accountService: AccountService,
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private formBuilder: FormBuilder) { }

  ngOnInit(): void {
    this.accountService.user$.pipe(take(1)).subscribe({
      next: (user: User | null) => {
        if (user) {
          this.router.navigateByUrl('/');
        } else {
          this.activatedRoute.queryParamMap.subscribe({
            next: (params: any) => {
              console.log(params);
              this.provider = this.activatedRoute.snapshot.paramMap.get('provider');
              this.access_token = params.get('access_token');
              this.userId = params.get('userId');
              this.email = params.get('email');

              if (this.provider && this.access_token && this.userId &&
                (this.provider === 'facebook' || this.provider === 'google')) {

                  this.initializeForm();
              } else {
                this.router.navigateByUrl('/account/register');
              }
            }
          })
        }
      }
    })
  }

  initializeForm() {
    this.registerForm = this.formBuilder.group({
      firstName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(15)]],
      lastName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(15)]]
    });
  }

  register() {
    this.submitted = true;
    this.errorMessages = [];

    if (this.registerForm.valid && this.userId && this.access_token && this.email && this.provider) {
      const firstName = this.registerForm.get('firstName')?.value;
      const lastName = this.registerForm.get('lastName')?.value;


      const model = new RegisterWithExternal(firstName, lastName, this.userId, this.email, this.access_token, this.provider);
      this.accountService.registerWithThirdParty(model).subscribe({
        next: _ => {
          console.log(_);
          this.router.navigateByUrl('/');
        }, error: error => {
          if (error.error.errors) {
            this.errorMessages = error.error.errors;
          } else {
            this.errorMessages.push(error.error);
          }
        }
      })
    }
  }
}
