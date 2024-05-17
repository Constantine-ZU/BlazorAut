using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly string _secretKey;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage, string secretKey)
    {
        _localStorage = localStorage;
        _secretKey = secretKey;
    }

   


    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            }, out _);
            return new AuthenticationState(principal);
        }
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public async Task MarkUserAsAuthenticated(string email)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, email)
        };
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        await _localStorage.SetItemAsync("authToken", tokenString);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("authToken");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
