using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly HttpClient _httpClient;

    public AuthService(string secretKey, string issuer, string audience, HttpClient httpClient)
    {
        _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
        _issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        _audience = audience ?? throw new ArgumentNullException(nameof(audience));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public string GenerateJwtToken(string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, email)
            }),
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
