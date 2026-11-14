using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ComercioTecnologia.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace ComercioTecnologia.Api.Services;

public class TokenService
{
    private readonly IConfiguration _cfg;

    public TokenService(IConfiguration cfg) => _cfg = cfg;

    public (string Token, DateTime Expira) Crear(Usuario u)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new(ClaimTypes.Name, u.Nombre),
            new(ClaimTypes.Email, u.Correo)
        };
        claims.AddRange(Rol.Nombres(u.Roles).Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var horas = _cfg.GetValue<int>("Jwt:HorasExpiracion", 8);
        var expira = DateTime.UtcNow.AddHours(horas);

        var token = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Audience"],
            claims: claims,
            expires: expira,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
