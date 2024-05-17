using BlazorAut.Data;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    //private readonly ILocalStorageService _localStorage;
    private readonly ApplicationDbContext _context;
    private readonly string _secretKey;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    //public CustomAuthenticationStateProvider(ILocalStorageService localStorage, string secretKey)
    //{
    //    _localStorage = localStorage;
    //    _secretKey = secretKey;
    //}
    public CustomAuthenticationStateProvider(IServiceScopeFactory serviceScopeFactory, ApplicationDbContext context, string secretKey)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _context = context;
        _secretKey = secretKey;
    }



    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var scopedContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var identity = new ClaimsIdentity();
            var token = await scopedContext.UserTokens.OrderByDescending(t => t.CreatedAt).FirstOrDefaultAsync();

            if (token != null && token.Expiration > DateTime.UtcNow)
            {
                var claims = ParseClaimsFromJwt(token.Token).ToList();

                // Найдем пользователя по UserId, связанному с токеном
                var user = await scopedContext.Users.SingleOrDefaultAsync(u => u.Id == token.UserId);
                if (user != null)
                {
                    // Добавим claim с именем пользователя
                    claims.Add(new Claim(ClaimTypes.Name, user.Email));
                    identity = new ClaimsIdentity(claims, "jwt");
                }
            }

            var userPrincipal = new ClaimsPrincipal(identity);
            return await Task.FromResult(new AuthenticationState(userPrincipal));
        }
    }



    public async Task MarkUserAsAuthenticated(string email)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            // Create a new user
            user = new User
            {
                Email = email,
                Tokens = new List<UserToken>()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // Generate JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, email)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Save or update the token in the database
        var userToken = await _context.UserTokens.SingleOrDefaultAsync(ut => ut.UserId == user.Id);

        if (userToken == null)
        {
            userToken = new UserToken
            {
                UserId = user.Id,
                Token = tokenString,
                Expiration = tokenDescriptor.Expires.Value,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserTokens.Add(userToken);
        }
        else
        {
            userToken.Token = tokenString;
            userToken.Expiration = tokenDescriptor.Expires.Value;
            userToken.CreatedAt = DateTime.UtcNow;

            _context.UserTokens.Update(userToken);
        }

        await _context.SaveChangesAsync();

        // Notify the authentication state provider
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    public async Task LogoutAsync()
    {
        var token = await _context.UserTokens.OrderByDescending(t => t.CreatedAt).FirstOrDefaultAsync();
        if (token != null)
        {
            _context.UserTokens.Remove(token);
            await _context.SaveChangesAsync();
        }

        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }


    private string GenerateJwtToken(string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, email) }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;
        return jsonToken?.Claims;
    }
}
