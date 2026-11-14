namespace ComercioTecnologia.Api.Models;

/// <summary>Roles como máscara de bits (columna usuarios.roles).</summary>
public static class Rol
{
    public const short Admin = 1;
    public const short Cliente = 2;
    public const short Vendedor = 4;
    public const short Soporte = 8;

    public static IEnumerable<string> Nombres(short roles)
    {
        if ((roles & Admin) != 0) yield return "Admin";
        if ((roles & Cliente) != 0) yield return "Cliente";
        if ((roles & Vendedor) != 0) yield return "Vendedor";
        if ((roles & Soporte) != 0) yield return "Soporte";
    }
}

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string? Telefono { get; set; }
    public short Roles { get; set; } = Rol.Cliente;
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoAcceso { get; set; }
}

public class Direccion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string? Alias { get; set; }
    public string Calle { get; set; } = "";
    public string Ciudad { get; set; } = "";
    public string? Estado { get; set; }
    public string? CodigoPostal { get; set; }
    public string Pais { get; set; } = "";
    public bool EsPrincipal { get; set; }
}

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Slug { get; set; } = "";
    public int? CategoriaPadreId { get; set; }
    public bool Activo { get; set; } = true;
}

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Descripcion { get; set; }
    public string Marca { get; set; } = "";
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
    public short GarantiaMeses { get; set; } = 12;
    // Columnas JSONB: se leen como texto JSON
    public string? Especificaciones { get; set; }
    public string? Imagenes { get; set; }
    public string? Relacionados { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}

public class VarianteProducto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public string Sku { get; set; } = "";
    public string? NombreVariante { get; set; }
    public decimal Precio { get; set; }
    public decimal? PrecioComparacion { get; set; }
    public decimal? Costo { get; set; }
    public decimal? PesoKg { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>Tabla inventario_stock (clave compuesta variante + almacén).</summary>
public class Stock
{
    public int VarianteId { get; set; }
    public int AlmacenId { get; set; }
    public int Cantidad { get; set; }
    public int StockMinimo { get; set; } = 5;
}

/// <summary>Carrito y lista de deseos (campo Lista = "Carrito" | "Deseos").</summary>
public class ItemUsuario
{
    public int UsuarioId { get; set; }
    public string Lista { get; set; } = "Carrito";
    public int VarianteId { get; set; }
    public VarianteProducto Variante { get; set; } = null!;
    public int Cantidad { get; set; } = 1;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
}

public class MetodoEnvio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public decimal CostoBase { get; set; }
    public short TiempoEstimadoDias { get; set; }
    public bool Activo { get; set; } = true;
}

public class Pedido
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int DireccionEnvioId { get; set; }
    public int MetodoEnvioId { get; set; }
    public int? CuponId { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuesto { get; set; }
    public decimal CostoEnvio { get; set; }
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = "";
    public string? NumeroFactura { get; set; }
    public DateTime? FechaFactura { get; set; }
    public DateTime CreadoEn { get; set; }
    public List<DetallePedido> Detalles { get; set; } = new();
}

public class DetallePedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public int VarianteId { get; set; }
    public VarianteProducto Variante { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

/// <summary>Fila de la vista vw_catalogo (solo lectura).</summary>
public class CatalogoItem
{
    public int ProductoId { get; set; }
    public string Producto { get; set; } = "";
    public int VarianteId { get; set; }
    public string Sku { get; set; } = "";
    public string? NombreVariante { get; set; }
    public string Marca { get; set; } = "";
    public string Categoria { get; set; } = "";
    public decimal Precio { get; set; }
    public decimal? PrecioComparacion { get; set; }
    public int StockTotal { get; set; }
    public short GarantiaMeses { get; set; }
}
