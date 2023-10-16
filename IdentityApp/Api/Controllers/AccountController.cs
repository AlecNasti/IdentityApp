using Api.DTOs.Account;
using Api.Helpers;
using Api.Models;
using Api.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JWTService _jwtService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<AccountController> _logger;

        public AccountController(JWTService jwtService,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            EmailService emailService,
            IConfiguration config,
            ILogger<AccountController> logger)
        {
            _jwtService = jwtService;
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("refresh-user-token")]
        public async Task<ActionResult<UserDto>> RefreshUserToken()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);
            return await CreateApplicationUserDto(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            if (user.EmailConfirmed is false)
            {
                return Unauthorized("Please confirm your email");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid username or password");
            }

            return await CreateApplicationUserDto(user);
        }

        [HttpPost("login-with-third-party")]
        public async Task<ActionResult<UserDto>> LoginWithThirdParty(LoginWithExternalDto model)
        {
            if (model.Provider.Equals(SD.Google))
            {
                try
                {
                    if (!GoogleValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unable to login with google");
                    }
                }
                catch (Exception)
                {
                    return Unauthorized("Unable to login with google");
                }
            }
            else
            {
                return BadRequest("Invalid provider");
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == model.UserId && x.Provider == model.Provider);
            if (user == null) return Unauthorized("Unable to find your account");

            return await CreateApplicationUserDto(user);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (await CheckEmailExistsAsync(model.Email))
            {
                return BadRequest($"An existing account is using {model.Email}, email address. Please try with another email address");
            }
            var userToAdd = new User
            {
                FirstName = model.FirstName.ToLower(),
                LastName = model.LastName.ToLower(),
                UserName = model.Email.ToLower(),
                Email = model.Email.ToLower()
            };

            var result = await _userManager.CreateAsync(userToAdd, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            try
            {
                if (await SendConfirmEmailAsync(userToAdd))
                {
                    return Ok(new JsonResult(new { title = "Account Created", message = "Your account has been created, please confirm your email address" }));

                }
                return BadRequest("Failed to send email, Please contact us");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest("Failed to send email, Please contact us");
            }

        }

        [HttpPost("register-with-third-party")]
        public async Task<ActionResult<UserDto>> RegisterWithThirdParty(RegisterWithExternal model)
        {
            if (model.Provider.Equals(SD.Google))
            {
                try
                {
                    if (!GoogleValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unable to register with google");
                    }
                }
                catch (Exception)
                {
                    return Unauthorized("Unable to register with google");
                }
            }
            else
            {
                return BadRequest("Invalid provider");
            }

            try
            {
                var user = await _userManager.FindByNameAsync(model.UserId);
                if (user != null) return BadRequest(string.Format("You have an account already. Please login with your {0}", model.Provider));

                var userToAdd = new User
                {
                    FirstName = model.FirstName.ToLower(),
                    LastName = model.LastName.ToLower(),
                    UserName = model.UserId,
                    Provider = model.Provider,
                    Email = model.email
                };

                var result = await _userManager.CreateAsync(userToAdd);
                if (!result.Succeeded) 
                {
                    return BadRequest(result.Errors);
                }

                if (await SendConfirmEmailAsync(userToAdd))
                {
                    return await CreateApplicationUserDto(userToAdd);
                }
                return BadRequest("Failed to send email, Please contact us");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest("Failed to send email, Please contact us");
            }

        }

        [HttpPut("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized("This email has not been registered yet");
            }

            if (user.EmailConfirmed is true)
            {
                return BadRequest("Your email has been confirmed already. Please login to your account");
            }

            try
            {
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));

                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                if (result.Succeeded)
                {
                    return Ok(new JsonResult(new { title = "Email confirmed", message = "Your email address is confirmed. You can now login" }));
                }

                return BadRequest("Invalid token. Please try again");
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message);
                return BadRequest("Failed to send email, Please contact us");
            }
        }

        [HttpPost("resend-email-confirmation-link/{email}")]
        public async Task<IActionResult> ResendEmailConfirmationLink(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid email");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Unauthorized("This email has not been registered yet");
            }
            if (user.EmailConfirmed is true)
            {
                return BadRequest("Your email has been confirmed already. Please login to your account");
            }

            try
            {
                if (await SendConfirmEmailAsync(user))
                {
                    return Ok(new JsonResult(new { title = "Confirmation link sent", message = "Please confirm your email address" }));

                }

                return BadRequest("Failed to send email, Please contact us");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest("Failed to send email, Please contact us");
            }

        }

        [HttpPost("forgot-username-or-password/{email}")]
        public async Task<IActionResult> ForgotUsernameOrPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid email");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Unauthorized("This email has not been registered yet");
            }
            if (user.EmailConfirmed is false)
            {
                return BadRequest("Please confirm your email, to be able to login");
            }

            try
            {
                if (await SendForgotUsernameOrPasswordEmail(user))
                {
                    return Ok(new JsonResult(new { title = "Forgot username or password email sent", message = "Please check your email" }));
                }
                return BadRequest("Failed to send email. Please contact us");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest("Failed to send email. Please contact us");
            }
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized("This email has not been registered yet");
            }

            if (user.EmailConfirmed is false)
            {
                return BadRequest("Please confirm your email, to be able to reset the password");
            }

            try
            {
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));

                var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
                if (result.Succeeded)
                {
                    return Ok(new JsonResult(new { title = "Password reset success", message = "Your password has been reset" }));
                }

                return BadRequest("Invalid token. Please try again");
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message);
                return BadRequest("Failed to send email, Please contact us");
            }
        }


        #region helper
        private async Task<UserDto> CreateApplicationUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Jwt = await _jwtService.CreateJWT(user)
            };
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }

        private async Task<bool> SendConfirmEmailAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ConfirmEmailPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Hello: {user.FirstName} {user.LastName}</p>" +
                "<p>Please confirm your email address by clicking on the following link.</p>" +
                $"<p><a href=\"{url}\">Click here</a></p>" +
                "<p>Thank you.</p>" +
                $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Confirm your email", body);

            return await _emailService.SendEmailAsync(emailSend);
        }

        private async Task<bool> SendForgotUsernameOrPasswordEmail(User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ResetPasswordPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Hello: {user.FirstName} {user.LastName}</p>" +
                $"<p>Username: {user.UserName}.</p>" +
                "<p>In order to reset your password, please click the following link </p>" +
                $"<p><a href=\"{url}\">Click here</a></p>" +
                "<p>Thank you.</p>" +
                $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Forgot username or password", body);

            return await _emailService.SendEmailAsync(emailSend);
        }

        private async Task<bool> GoogleValidatedAsync(string accessToken, string userId)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(accessToken);

            if (!payload.Audience.Equals(_config["Google:ClientId"]))
            {
                return false;
            }

            if (!payload.Issuer.Equals("accounts.google.com") && !payload.Issuer.Equals("https://accounts.google.com"))
            {
                return false;
            }

            if (payload.ExpirationTimeSeconds == null)
            {
                return false;
            }

            DateTime now = DateTime.Now.ToUniversalTime();
            DateTime expiration = DateTimeOffset.FromUnixTimeSeconds((long)payload.ExpirationTimeSeconds).DateTime;
            if (now > expiration)
            {
                return false;
            }

            if (!payload.Subject.Equals(userId))
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
