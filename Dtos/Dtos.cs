using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ComercioTecnologia.Api.Dtos;

// ---------- Entrada ----------
public class RegistroDto
{
    [Required, StringLength(100)] public string Nombre { get; set; } = "";
    [Required, EmailAddress, StringLength(150)] public string Correo { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 8)] public string Password { get; set; } = "";
    [StringLength(25)] public string? Telefono { get; set; }
}

public class LoginDto
{
    [Required, EmailAddress] public string Correo { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public class FiltroCatalogo
{
    public string? Q { get; set; }
    public string? Categoria { get; set; }
    public string? Marca { get; set; }
    public decimal? PrecioMin { get; set; }
    public decimal? PrecioMax { get; set; }
    public bool SoloConStock { get; set; }
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 12;
}

public class ItemCarritoDto
{
    [Range(1, int.MaxValue)] public int VarianteId { get; set; }
    [Range(1, 100)] public int Cantidad { get; set; } = 1;
}

public class CantidadDto
{
    [Range(1, 100)] public int Cantidad { get; set; }
}

public class CrearPedidoDto
{
    [Range(1, int.MaxValue)] public int DireccionEnvioId { get; set; }
    [Range(1, int.MaxValue)] public int MetodoEnvioId { get; set; }
    [Required, StringLength(30)] public string MetodoPago { get; set; } = "";
    [StringLength(40)] public string? CodigoCupon { get; set; }
}

public class AgregarImagenDto
{
    [Required, StringLength(500)] public string Url { get; set; } = "";
    /// <summary>Si es true pasa a ser la imagen principal (la primera imagen siempre lo es).</summary>
    public bool Principal { get; set; }
}

public class DireccionDto
{
    [StringLength(50)] public string? Alias { get; set; }
    [Required, StringLength(200)] public string Calle { get; set; } = "";
    [Required, StringLength(100)] public string Ciudad { get; set; } = "";
    [StringLength(100)] public string? Estado { get; set; }
    [StringLength(20)] public string? CodigoPostal { get; set; }
    [Required, StringLength(100)] public string Pais { get; set; } = "";
    public bool EsPrincipal { get; set; }
}

// ---------- Salida ----------
public record UsuarioDto(int Id, string Nombre, string Correo, string[] Roles);
public record AuthRespuesta(string Token, DateTime Expira, UsuarioDto Usuario);
public record PaginaDto<T>(IReadOnlyList<T> Items, int Total, int Pagina, int Tamano);

public record CategoriaDto(int Id, string Nombre, string Slug, int? CategoriaPadreId);
public record ImagenDto(string Url, int Orden, bool Principal);

/// <summary>Fila del catálogo (vw_catalogo) más la imagen a mostrar: la de la variante o la principal del producto.</summary>
public record CatalogoItemDto(
    int ProductoId, string Producto, int VarianteId, string Sku, string? NombreVariante,
    string Marca, string Categoria, decimal Precio, decimal? PrecioComparacion,
    int StockTotal, short GarantiaMeses, string? Imagen);

public record VarianteDto(int Id, string Sku, string? Nombre, decimal Precio, decimal? PrecioComparacion, int Stock, string? ImagenUrl);
public record ProductoDetalleDto(
    int Id, string Nombre, string Slug, string? Descripcion, string Marca, string Categoria,
    short GarantiaMeses, JsonElement? Especificaciones, JsonElement? Imagenes, JsonElement? Relacionados,
    IReadOnlyList<VarianteDto> Variantes);

public record CarritoItemDto(
    int VarianteId, string Sku, string Producto, string? Variante,
    decimal PrecioUnitario, int Cantidad, decimal Subtotal, bool Activo, int Stock);
public record CarritoDto(IReadOnlyList<CarritoItemDto> Items, decimal Subtotal);

public record PedidoResumenDto(int Id, string Estado, decimal Total, string MetodoPago, DateTime CreadoEn);
public record LineaPedidoDto(int VarianteId, string Sku, string Producto, int Cantidad, decimal PrecioUnitario, decimal Subtotal);
public record PedidoDetalleDto(
    int Id, string Estado, decimal Subtotal, decimal Descuento, decimal Impuesto, decimal CostoEnvio,
    decimal Total, string MetodoPago, DateTime CreadoEn, IReadOnlyList<LineaPedidoDto> Lineas);

public record DireccionRespuesta(int Id, string? Alias, string Calle, string Ciudad, string? Estado, string? CodigoPostal, string Pais, bool EsPrincipal);
public record MetodoEnvioDto(int Id, string Nombre, decimal CostoBase, short TiempoEstimadoDias);
