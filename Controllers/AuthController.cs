using ComercioTecnologia.Api.Data;
using ComercioTecnologia.Api.Dtos;
using ComercioTecnologia.Api.Models;
using ComercioTecnologia.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComercioTecnologia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;

    public AuthController(AppDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    /// <summary>Registra un cliente nuevo y devuelve su token.</summary>
    [HttpPost("registro")]
    public async Task<ActionResult<AuthRespuesta>> Registro(RegistroDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();
        if (await _db.Usuarios.AnyAsync(u => u.Correo == correo))
            return Conflict(new ProblemDetails { Title = "El correo ya está registrado.", Status = 409 });

        var usuario = new Usuario
        {
            Nombre = dto.Nombre.Trim(),
            Correo = correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Telefono = dto.Telefono,
            Roles = Rol.Cliente
        };
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, Respuesta(usuario));
    }

    /// <summary>Inicia sesión y devuelve el token JWT.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthRespuesta>> Login(LoginDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);

        if (usuario is null || !usuario.Activo || !PasswordValida(dto.Password, usuario.PasswordHash))
            return Unauthorized(new ProblemDetails { Title = "Credenciales inválidas.", Status = 401 });

        usuario.UltimoAcceso = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Respuesta(usuario);
    }

    /// <summary>Datos del usuario autenticado.</summary>
    [Authorize]
    [HttpGet("perfil")]
    public async Task<ActionResult<UsuarioDto>> Perfil()
    {
        var id = User.GetUserId();
        var u = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return NotFound();
        return new UsuarioDto(u.Id, u.Nombre, u.Correo, Rol.Nombres(u.Roles).ToArray());
    }

    private AuthRespuesta Respuesta(Usuario u)
    {
        var (token, expira) = _tokens.Crear(u);
        return new AuthRespuesta(token, expira,
            new UsuarioDto(u.Id, u.Nombre, u.Correo, Rol.Nombres(u.Roles).ToArray()));
    }

    // Los usuarios de ejemplo del script no tienen hash BCrypt válido: se tratan como credencial incorrecta.
    private static bool PasswordValida(string password, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(password, hash); }
        catch { return false; }
    }
}
