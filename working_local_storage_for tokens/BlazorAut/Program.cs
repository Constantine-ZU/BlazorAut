using BlazorAut.Data;
using BlazorAut.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddBlazoredLocalStorage();

// Add DbContext configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AppSettingsService
builder.Services.AddScoped<AppSettingsService>();

// Load app settings from the database
var serviceProvider = builder.Services.BuildServiceProvider();
var appSettingsService = serviceProvider.GetRequiredService<AppSettingsService>();
var appSettings = appSettingsService.GetAppSettingsAsync().Result;

var secretKey = appSettings["JwtSecretKey"];
var issuer = appSettings["JwtIssuer"];
var audience = appSettings["JwtAudience"];
var smtpServer = appSettings["SmtpServer"];
var smtpPort = int.Parse(appSettings["SmtpPort"]);
var smtpUser = appSettings["SmtpUser"];
var smtpPass = appSettings["SmtpPass"];
var key = Encoding.UTF8.GetBytes(secretKey);

// Configure JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });



builder.Services.AddScoped<JwtService>(provider => new JwtService(secretKey, issuer, audience));
builder.Services.AddScoped<IEmailService>(provider => new EmailService(smtpServer, smtpPort, smtpUser, smtpPass));
builder.Services.AddScoped<AuthenticationStateProvider>(provider => new CustomAuthenticationStateProvider(provider.GetRequiredService<ILocalStorageService>(), secretKey));
builder.Services.AddAuthorizationCore();
builder.Services.AddHttpContextAccessor();
builder.Services.AddBlazoredSessionStorage(); // Add this line

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
