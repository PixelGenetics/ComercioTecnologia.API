using ComercioTecnologia.Api.Data;
using ComercioTecnologia.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ComercioTecnologia.Api.Services;

public class PedidoService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;

    public PedidoService(AppDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    /// <summary>
    /// Crea el pedido con la función atómica de la base de datos.
    /// Lanza PostgresException con SqlState 50001..50005 si una validación falla.
    /// </summary>
    public async Task<int> CrearDesdeCarritoAsync(int usuarioId, CrearPedidoDto d)
    {
        var tasa = _cfg.GetValue<decimal>("Negocio:TasaImpuesto", 0.15m);
        var pago = d.MetodoPago.Trim();
        var cupon = string.IsNullOrWhiteSpace(d.CodigoCupon) ? null : d.CodigoCupon.Trim();

        var resultado = await _db.Database
            .SqlQuery<int>($"SELECT fn_crear_pedido_desde_carrito({usuarioId}, {d.DireccionEnvioId}, {d.MetodoEnvioId}, {pago}::varchar, {cupon}::varchar, {tasa}::numeric) AS \"Value\"")
            .ToListAsync();

        return resultado[0];
    }

    public async Task<List<PedidoResumenDto>> ListarAsync(int usuarioId) =>
        await _db.Pedidos.AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.CreadoEn)
            .Select(p => new PedidoResumenDto(p.Id, p.Estado, p.Total, p.MetodoPago, p.CreadoEn))
            .ToListAsync();

    public async Task<PedidoDetalleDto?> ObtenerDetalleAsync(int pedidoId, int usuarioId)
    {
        var p = await _db.Pedidos.AsNoTracking()
            .Where(x => x.Id == pedidoId && x.UsuarioId == usuarioId)
            .Select(x => new
            {
                x.Id, x.Estado, x.Subtotal, x.Descuento, x.Impuesto,
                x.CostoEnvio, x.Total, x.MetodoPago, x.CreadoEn
            })
            .FirstOrDefaultAsync();

        if (p is null) return null;

        var lineas = await _db.DetallesPedido.AsNoTracking()
            .Where(l => l.PedidoId == pedidoId)
            .Select(l => new LineaPedidoDto(
                l.VarianteId, l.Variante.Sku, l.Variante.Producto.Nombre,
                l.Cantidad, l.PrecioUnitario, l.Cantidad * l.PrecioUnitario))
            .ToListAsync();

        return new PedidoDetalleDto(
            p.Id, p.Estado, p.Subtotal, p.Descuento, p.Impuesto,
            p.CostoEnvio, p.Total, p.MetodoPago, p.CreadoEn, lineas);
    }
}
