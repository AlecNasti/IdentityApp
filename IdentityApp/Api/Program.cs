using Api.Data;
using Api.Helpers;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ContextSeedService>();

builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    options.SignIn.RequireConfirmedEmail = true;

})
    .AddRoles<IdentityRole>() //aggiungi ruoli
    .AddRoleManager<RoleManager<IdentityRole>>() //usa il manager per creare ruoli
    .AddEntityFrameworkStores<Context>() // aggiungere il contesto
    .AddSignInManager<SignInManager<User>>() // usare il manager per il log in
    .AddUserManager<UserManager<User>>() // usare il manager per creare utenti
    .AddDefaultTokenProviders(); // creare token per la conferma dell'email

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidateIssuer = true,
            ValidateAudience = false
        };
    });

builder.Services.AddCors();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
        .Where(x => x.Value.Errors.Count > 0)
        .SelectMany(x => x.Value.Errors)
        .Select(x => x.ErrorMessage).ToArray();

        var toReturn = new
        {
            Errors = errors
        };

        return new BadRequestObjectResult(toReturn);
    };
});

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy(SD.AdminPolicy, policy => policy.RequireRole(SD.AdminRole));
    opt.AddPolicy(SD.ManagerPolicy, policy => policy.RequireRole(SD.ManagerRole));
    opt.AddPolicy(SD.PlayerPolicy, policy => policy.RequireRole(SD.PlayerRole));

    opt.AddPolicy(SD.AdminOrManagerPolicy, policy => policy.RequireRole(SD.AdminRole, SD.ManagerRole));
    opt.AddPolicy(SD.AdminAndManagerPolicy, policy => policy.RequireRole(SD.AdminRole).RequireRole(SD.ManagerRole));
    opt.AddPolicy(SD.AllRolePolicy, policy => policy.RequireRole(SD.AdminRole, SD.ManagerRole, SD.PlayerRole));

    opt.AddPolicy(SD.AdminEmailPolicy, policy => policy.RequireClaim(ClaimTypes.Email, SD.ClaimAdminEmail));
    opt.AddPolicy(SD.PowerSurnamePolicy, policy => policy.RequireClaim(ClaimTypes.Surname, SD.ClaimPowerSurname));
    opt.AddPolicy(SD.ManagerEmailAndPowerSurnamePolicy, policy => policy.RequireClaim(ClaimTypes.Surname, SD.ClaimPowerSurname)
    .RequireClaim(ClaimTypes.Email, SD.ClaimManagerEmail));
    opt.AddPolicy("VipPolicy", policy => policy.RequireAssertion(context => SD.VIPPolicy(context)));
});

var app = builder.Build();

app.UseCors(opt =>
{
    opt.AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins(builder.Configuration["JWT:ClientUrl"]);
});


if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#region context seed
using var scope = app.Services.CreateScope();
try
{
    var contextSeedService = scope.ServiceProvider.GetService<ContextSeedService>();
    await contextSeedService.InizializeContextAsync();
}
catch (Exception ex)
{
    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
    logger.LogError(ex.Message, "failed to start and seed the database");
}
#endregion

app.Run();
