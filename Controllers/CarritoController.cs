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
public class CarritoController : ControllerBase
{
    private const string Lista = "Carrito";
    private readonly AppDbContext _db;

    public CarritoController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<CarritoDto>> Obtener() =>
        await ObtenerCarrito(User.GetUserId());

    /// <summary>Agrega una variante (suma a la cantidad si ya estaba en el carrito).</summary>
    [HttpPost]
    public async Task<ActionResult<CarritoDto>> Agregar(ItemCarritoDto dto)
    {
        var uid = User.GetUserId();

        var existe = await _db.VariantesProducto.AsNoTracking()
            .AnyAsync(v => v.Id == dto.VarianteId && v.Activo && v.Producto.Activo);
        if (!existe)
            return NotFound(Problema("La variante no existe o no está disponible.", 404));

        var item = await _db.ItemsUsuario.FirstOrDefaultAsync(i =>
            i.UsuarioId == uid && i.Lista == Lista && i.VarianteId == dto.VarianteId);

        var nueva = (item?.Cantidad ?? 0) + dto.Cantidad;
        var stock = await StockTotal(dto.VarianteId);
        if (nueva > stock)
            return Conflict(Problema($"Stock insuficiente. Disponible: {stock}.", 409));

        if (item is null)
        {
            _db.ItemsUsuario.Add(new ItemUsuario
            {
                UsuarioId = uid, Lista = Lista, VarianteId = dto.VarianteId, Cantidad = nueva
            });
        }
        else
        {
            item.Cantidad = nueva;
            item.ActualizadoEn = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();

        return await ObtenerCarrito(uid);
    }

    /// <summary>Cambia la cantidad de una variante que ya está en el carrito.</summary>
    [HttpPut("{varianteId:int}")]
    public async Task<ActionResult<CarritoDto>> Actualizar(int varianteId, CantidadDto dto)
    {
        var uid = User.GetUserId();
        var item = await _db.ItemsUsuario.FirstOrDefaultAsync(i =>
            i.UsuarioId == uid && i.Lista == Lista && i.VarianteId == varianteId);
        if (item is null)
            return NotFound(Problema("La variante no está en el carrito.", 404));

        var stock = await StockTotal(varianteId);
        if (dto.Cantidad > stock)
            return Conflict(Problema($"Stock insuficiente. Disponible: {stock}.", 409));

        item.Cantidad = dto.Cantidad;
        item.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await ObtenerCarrito(uid);
    }

    [HttpDelete("{varianteId:int}")]
    public async Task<ActionResult<CarritoDto>> Quitar(int varianteId)
    {
        var uid = User.GetUserId();
        await _db.ItemsUsuario
            .Where(i => i.UsuarioId == uid && i.Lista == Lista && i.VarianteId == varianteId)
            .ExecuteDeleteAsync();
        return await ObtenerCarrito(uid);
    }

    [HttpDelete]
    public async Task<IActionResult> Vaciar()
    {
        var uid = User.GetUserId();
        await _db.ItemsUsuario.Where(i => i.UsuarioId == uid && i.Lista == Lista).ExecuteDeleteAsync();
        return NoContent();
    }

    // ---------- helpers ----------
    private async Task<int> StockTotal(int varianteId)
    {
        var total = await _db.InventarioStock
            .Where(s => s.VarianteId == varianteId)
            .SumAsync(s => (int?)s.Cantidad);
        return total ?? 0;
    }

    private async Task<CarritoDto> ObtenerCarrito(int uid)
    {
        var items = await _db.ItemsUsuario.AsNoTracking()
            .Where(i => i.UsuarioId == uid && i.Lista == Lista)
            .OrderBy(i => i.ActualizadoEn)
            .Select(i => new CarritoItemDto(
                i.VarianteId,
                i.Variante.Sku,
                i.Variante.Producto.Nombre,
                i.Variante.NombreVariante,
                i.Variante.Precio,
                i.Cantidad,
                i.Variante.Precio * i.Cantidad,
                i.Variante.Activo,
                _db.InventarioStock.Where(s => s.VarianteId == i.VarianteId).Sum(s => (int?)s.Cantidad) ?? 0))
            .ToListAsync();

        return new CarritoDto(items, items.Sum(x => x.Subtotal));
    }

    private static ProblemDetails Problema(string titulo, int status) =>
        new ProblemDetails { Title = titulo, Status = status };
}
