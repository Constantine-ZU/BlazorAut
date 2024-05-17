using Microsoft.AspNetCore.Components.Authorization;
using Blazored.SessionStorage;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlazorAut.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ISessionStorageService _sessionStorage;
        private readonly string _secretKey;

        public CustomAuthenticationStateProvider(ISessionStorageService sessionStorage, string secretKey)
        {
            _sessionStorage = sessionStorage;
            _secretKey = secretKey;
            Console.WriteLine($"CustomAuthenticationStateProvider initialized with secret key: {_secretKey}");
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // This method needs to be called during or after the OnAfterRenderAsync lifecycle method
            var token = await _sessionStorage.GetItemAsync<string>("authToken");
            Console.WriteLine($"GetAuthenticationStateAsync called. Token: {token}");

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token is null or empty. Returning anonymous user.");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            }, out _);

            var identity = (ClaimsIdentity)principal.Identity;
            Console.WriteLine($"User authenticated: {identity.Name}");
            return new AuthenticationState(principal);
        }

        public async Task MarkUserAsAuthenticated(string email)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, email) };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            await _sessionStorage.SetItemAsync("authToken", tokenString);
            Console.WriteLine($"MarkUserAsAuthenticated called for email: {email}");
            Console.WriteLine($"Generated JWT token: {tokenString}");

            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwtAuthType"));
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(authenticatedUser)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _sessionStorage.RemoveItemAsync("authToken");
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
        }
    }
}
