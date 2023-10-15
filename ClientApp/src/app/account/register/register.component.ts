import { Component, ElementRef, Inject, OnInit, Renderer2, ViewChild } from '@angular/core';
import { AccountService } from '../account.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SharedService } from 'src/app/shared/shared.service';
import { Router } from '@angular/router';
import { take } from 'rxjs';
import { User } from 'src/app/shared/Models/account/User';
import { CredentialResponse } from 'google-one-tap';
import jwtDecode from 'jwt-decode';
import { DOCUMENT } from '@angular/common';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @ViewChild('googleButton', { static: true })
  googleButton: ElementRef = new ElementRef({});
  registerForm: FormGroup = new FormGroup({});
  submitted = false;
  errorMessages: string[] = [];

  constructor(
    private accountService: AccountService,
    private sharedService: SharedService,
    private formBuilder: FormBuilder,
    private router: Router,
    private _rendered2: Renderer2,
    @Inject(DOCUMENT) private _document: Document
  ) {
    this.accountService.user$.pipe(take(1)).subscribe({
      next: (user: User | null) => {
        if (user) {
          this.router.navigateByUrl('/');
        }
      }
    });
  }

  ngOnInit(): void {
    this.inizializeForm();
    this.inizializeGoogleButton();
  }

  ngAfterViewInit() {
    const script1 = this._rendered2.createElement('script');
    script1.src = 'https://accounts.google.com/gsi/client';
    script1.async = 'true';
    script1.defer = 'true';
    this._rendered2.appendChild(this._document.body, script1);
  }

  inizializeForm() {
    this.registerForm = this.formBuilder.group({
      firstName: [
        '',
        [Validators.required, Validators.minLength(3), Validators.maxLength(15)]
      ],
      lastName: [
        '',
        [Validators.required, Validators.minLength(3), Validators.maxLength(15)]
      ],
      email: [
        '',
        [Validators.required, Validators.pattern('^\\S+@\\S+\\.\\S+$')]
      ],
      password: [
        '',
        [Validators.required, Validators.minLength(6), Validators.maxLength(15)]
      ]
    });
  }

  register() {
    this.submitted = true;
    this.errorMessages = [];
    if (this.registerForm.valid) {
      this.accountService.register(this.registerForm.value).subscribe({
        next: (response: any) => {
          this.sharedService.showNotification(
            true,
            response.value.title,
            response.value.message
          );
          this.router.navigateByUrl('/account/login');
        },
        error: error => {
          console.log(error);
          if (error.error.errors) {
            this.errorMessages = error.error.errors;
          } else {
            this.errorMessages.push(error.error);
          }
        }
      });
    }
  }

  private inizializeGoogleButton() {
    (window as any).onGoogleLibraryLoad = () => {
      // @ts-ignore
      google.accounts.id.initialize({
        client_id: '1033808628087-e9jgld45n2fg8ig5rd5269mr7ik3qr8o.apps.googleusercontent.com',
        callback: this.googleCallBack.bind(this),
        auto_select: false,
        cancel_on_tap_outside: true
      });
      // @ts-ignore
      google.accounts.id.renderButton(
        this.googleButton.nativeElement,
        {size: 'medium', shape: 'rectangular', text: 'signup_with', logo_alignment: 'center'}
      );
    };
  }

  private async googleCallBack(response: CredentialResponse) {
    const decodedToken: any = jwtDecode(response.credential);
    this.router.navigateByUrl(`/account/register/third-party/google?access_token=${response.credential}&userId=${decodedToken.sub}&email=${decodedToken.email}`);
  }
}
