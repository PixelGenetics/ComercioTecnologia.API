using ComercioTecnologia.Api.Data;
using ComercioTecnologia.Api.Dtos;
using ComercioTecnologia.Api.Models;
using ComercioTecnologia.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComercioTecnologia.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DireccionesController : ControllerBase
{
    private readonly AppDbContext _db;

    public DireccionesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<DireccionRespuesta>>> Listar()
    {
        var uid = User.GetUserId();
        return await _db.Direcciones.AsNoTracking()
            .Where(x => x.UsuarioId == uid)
            .OrderByDescending(x => x.EsPrincipal).ThenBy(x => x.Id)
            .Select(x => new DireccionRespuesta(
                x.Id, x.Alias, x.Calle, x.Ciudad, x.Estado, x.CodigoPostal, x.Pais, x.EsPrincipal))
            .ToListAsync();
    }

    /// <summary>Crea una dirección. La primera del usuario queda como principal.</summary>
    [HttpPost]
    public async Task<ActionResult<DireccionRespuesta>> Crear(DireccionDto dto)
    {
        var uid = User.GetUserId();
        var esPrimera = !await _db.Direcciones.AnyAsync(x => x.UsuarioId == uid);
        var principal = dto.EsPrincipal || esPrimera;

        // Solo puede haber una principal por usuario (índice único parcial)
        if (principal)
        {
            await _db.Direcciones
                .Where(x => x.UsuarioId == uid && x.EsPrincipal)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.EsPrincipal, false));
        }

        var nueva = new Direccion
        {
            UsuarioId = uid,
            Alias = dto.Alias,
            Calle = dto.Calle.Trim(),
            Ciudad = dto.Ciudad.Trim(),
            Estado = dto.Estado,
            CodigoPostal = dto.CodigoPostal,
            Pais = dto.Pais.Trim(),
            EsPrincipal = principal
        };
        _db.Direcciones.Add(nueva);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new DireccionRespuesta(
            nueva.Id, nueva.Alias, nueva.Calle, nueva.Ciudad, nueva.Estado,
            nueva.CodigoPostal, nueva.Pais, nueva.EsPrincipal));
    }
}
