using ComercioTecnologia.Api.Data;
using ComercioTecnologia.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComercioTecnologia.Api.Controllers;

[ApiController]
[Route("api/metodos-envio")]
public class MetodosEnvioController : ControllerBase
{
    private readonly AppDbContext _db;

    public MetodosEnvioController(AppDbContext db) => _db = db;

    /// <summary>Métodos de envío activos (para el checkout).</summary>
    [HttpGet]
    public async Task<ActionResult<List<MetodoEnvioDto>>> Listar() =>
        await _db.MetodosEnvio.AsNoTracking()
            .Where(m => m.Activo)
            .OrderBy(m => m.CostoBase)
            .Select(m => new MetodoEnvioDto(m.Id, m.Nombre, m.CostoBase, m.TiempoEstimadoDias))
            .ToListAsync();
}
