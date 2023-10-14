import { Component, OnInit } from '@angular/core';
import { AccountService } from '../account.service';
import { SharedService } from 'src/app/shared/shared.service';
import { ActivatedRoute, Router } from '@angular/router';
import { User } from 'src/app/shared/Models/account/User';
import { take } from 'rxjs';
import { ConfirmEmail } from 'src/app/shared/Models/account/ConfirmEmail';

@Component({
  selector: 'app-confirm-email',
  templateUrl: './confirm-email.component.html',
  styleUrls: ['./confirm-email.component.css']
})
export class ConfirmEmailComponent implements OnInit {
  success = true;

  constructor(
    private accontServer: AccountService,
    private sharedService: SharedService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.accontServer.user$.pipe(take(1)).subscribe({
      next: (user: User | null) => {
        if (user) {
          this.router.navigateByUrl('/');
        } else {
          this.activatedRoute.queryParams.subscribe({
            next: (params: any) => {
              const confirmEmail: ConfirmEmail = {
                token: params.token,
                email: params.email
              };

              this.accontServer.ConfirmEmail(confirmEmail).subscribe({
                next: (response: any) => {
                  this.sharedService.showNotification(
                    true,
                    response.value.title,
                    response.value.message
                  );
                },
                error: (error: any) => {
                  this.success = false;
                  this.sharedService.showNotification(
                    false,
                    'Failed',
                    error.error
                  );
                }
              });
            }
          });
        }
      }
    });
  }

  resendEmailConfirmationLink() {
    this.router.navigateByUrl('/account/send-email/resend-email-confirmation-link');
  }
}
