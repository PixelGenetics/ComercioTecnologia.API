using ComercioTecnologia.Api.Dtos;
using ComercioTecnologia.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ComercioTecnologia.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly PedidoService _pedidos;

    public PedidosController(PedidoService pedidos) => _pedidos = pedidos;

    /// <summary>Checkout: crea el pedido con los ítems del carrito.</summary>
    [HttpPost]
    public async Task<ActionResult<PedidoDetalleDto>> Crear(CrearPedidoDto dto)
    {
        var uid = User.GetUserId();
        try
        {
            var id = await _pedidos.CrearDesdeCarritoAsync(uid, dto);
            var detalle = await _pedidos.ObtenerDetalleAsync(id, uid);
            return CreatedAtAction(nameof(Detalle), new { id }, detalle);
        }
        catch (PostgresException ex) when (ex.SqlState is "50001" or "50002" or "50003" or "50004" or "50005")
        {
            // Códigos definidos en fn_crear_pedido_desde_carrito
            var status = ex.SqlState switch
            {
                "50002" => StatusCodes.Status403Forbidden,
                "50003" => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };
            return StatusCode(status, new ProblemDetails
            {
                Title = ex.MessageText,
                Status = status,
                Detail = $"Código {ex.SqlState}"
            });
        }
    }

    /// <summary>Pedidos del usuario autenticado.</summary>
    [HttpGet]
    public async Task<ActionResult<List<PedidoResumenDto>>> Listar() =>
        await _pedidos.ListarAsync(User.GetUserId());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoDetalleDto>> Detalle(int id)
    {
        var detalle = await _pedidos.ObtenerDetalleAsync(id, User.GetUserId());
        if (detalle is null)
            return NotFound(new ProblemDetails { Title = "Pedido no encontrado.", Status = 404 });
        return detalle;
    }
}
