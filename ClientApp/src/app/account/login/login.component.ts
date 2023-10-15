import {
  Component,
  ElementRef,
  Inject,
  OnInit,
  Renderer2,
  ViewChild
} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../account.service';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs';
import { User } from 'src/app/shared/Models/account/User';
import { DOCUMENT } from '@angular/common';
import { CredentialResponse } from 'google-one-tap';
import jwtDecode from 'jwt-decode';
import { LoginWithExternal } from 'src/app/shared/Models/account/loginWithExternal';
import { SharedService } from 'src/app/shared/shared.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  @ViewChild('googleButton', { static: true })
  googleButton: ElementRef = new ElementRef({});
  loginForm: FormGroup = new FormGroup({});
  submitted = false;
  errorMessages: string[] = [];
  returnUrl: string | null = null;

  constructor(
    private accountService: AccountService,
    private formBuilder: FormBuilder,
    private router: Router,
    private activatedRouter: ActivatedRoute,
    private _rendered2: Renderer2,
    private sharedService: SharedService,
    @Inject(DOCUMENT) private _document: Document
  ) {
    this.accountService.user$.pipe(take(1)).subscribe({
      next: (user: User | null) => {
        if (user) {
          this.router.navigateByUrl('/');
        } else {
          this.activatedRouter.queryParamMap.subscribe({
            next: (params: any) => {
              if (params) {
                this.returnUrl = params.get('returnUrl');
              }
            }
          });
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
    this.loginForm = this.formBuilder.group({
      userName: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  login() {
    this.submitted = true;
    this.errorMessages = [];

    if (this.loginForm.valid) {
      this.accountService.login(this.loginForm.value).subscribe({
        next: (response: any) => {
          if (this.returnUrl) {
            this.router.navigateByUrl(this.returnUrl);
          } else {
            this.router.navigateByUrl('/');
          }
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

  resendEmailConfirmationLink() {
    this.router.navigateByUrl(
      '/account/send-email/resend-email-confirmation-link'
    );
  }

  private inizializeGoogleButton() {
    (window as any).onGoogleLibraryLoad = () => {
      // @ts-ignore
      google.accounts.id.initialize({
        client_id:
          '1033808628087-e9jgld45n2fg8ig5rd5269mr7ik3qr8o.apps.googleusercontent.com',
        callback: this.googleCallBack.bind(this),
        auto_select: false,
        cancel_on_tap_outside: true
      });
      // @ts-ignore
      google.accounts.id.renderButton(this.googleButton.nativeElement, {
        size: 'medium',
        shape: 'rectangular',
        text: 'signin_with',
        logo_alignment: 'center'
      });
    };
  }

  private async googleCallBack(response: CredentialResponse) {
    const decodedToken: any = jwtDecode(response.credential);
    this.accountService
      .loginWithThirdParty(
        new LoginWithExternal(response.credential, decodedToken.sub, 'google')
      )
      .subscribe({
        next: _ => {
          if (this.returnUrl) {
            this.router.navigateByUrl(this.returnUrl);
          } else {
            this.router.navigateByUrl('/');
          }
        },
        error: error => {
          this.sharedService.showNotification(false, 'Failed', error.error);
        }
      });
  }
}
