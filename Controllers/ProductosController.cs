using System.Text.Json;
using ComercioTecnologia.Api.Data;
using ComercioTecnologia.Api.Dtos;
using ComercioTecnologia.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComercioTecnologia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductosController(AppDbContext db) => _db = db;

    /// <summary>Catálogo paginado (una fila por variante) con filtros. Incluye la imagen a mostrar.</summary>
    [HttpGet]
    public async Task<ActionResult<PaginaDto<CatalogoItemDto>>> Listar([FromQuery] FiltroCatalogo f)
    {
        var pagina = Math.Max(f.Pagina, 1);
        var tamano = Math.Clamp(f.Tamano, 1, 50);

        var q = _db.Catalogo.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Q))
        {
            var patron = $"%{f.Q.Trim()}%";
            q = q.Where(x => EF.Functions.ILike(x.Producto, patron)
                          || EF.Functions.ILike(x.Marca, patron)
                          || EF.Functions.ILike(x.Sku, patron));
        }
        if (!string.IsNullOrWhiteSpace(f.Categoria))
        {
            var cat = f.Categoria.Trim().ToLower();
            q = q.Where(x => x.Categoria.ToLower() == cat);
        }
        if (!string.IsNullOrWhiteSpace(f.Marca))
        {
            var marca = f.Marca.Trim().ToLower();
            q = q.Where(x => x.Marca.ToLower() == marca);
        }
        if (f.PrecioMin.HasValue) q = q.Where(x => x.Precio >= f.PrecioMin.Value);
        if (f.PrecioMax.HasValue) q = q.Where(x => x.Precio <= f.PrecioMax.Value);
        if (f.SoloConStock) q = q.Where(x => x.StockTotal > 0);

        var total = await q.CountAsync();
        var items = await q
            .OrderBy(x => x.Producto).ThenBy(x => x.Precio)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .ToListAsync();

        // Imágenes solo de la página actual: la de la variante tiene prioridad sobre la del producto
        var varianteIds = items.Select(i => i.VarianteId).ToList();
        var productoIds = items.Select(i => i.ProductoId).Distinct().ToList();

        var imagenVariante = await _db.VariantesProducto.AsNoTracking()
            .Where(v => varianteIds.Contains(v.Id) && v.ImagenUrl != null)
            .ToDictionaryAsync(v => v.Id, v => v.ImagenUrl!);
        var imagenProducto = (await _db.Productos.AsNoTracking()
                .Where(p => productoIds.Contains(p.Id) && p.Imagenes != null)
                .Select(p => new { p.Id, p.Imagenes })
                .ToListAsync())
            .ToDictionary(p => p.Id, p => ImagenesProducto.Principal(p.Imagenes));

        var resultado = items.Select(x => new CatalogoItemDto(
                x.ProductoId, x.Producto, x.VarianteId, x.Sku, x.NombreVariante, x.Marca, x.Categoria,
                x.Precio, x.PrecioComparacion, x.StockTotal, x.GarantiaMeses,
                imagenVariante.GetValueOrDefault(x.VarianteId) ?? imagenProducto.GetValueOrDefault(x.ProductoId)))
            .ToList();

        return new PaginaDto<CatalogoItemDto>(resultado, total, pagina, tamano);
    }

    /// <summary>Detalle de un producto con especificaciones, imágenes y variantes.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDetalleDto>> Detalle(int id)
    {
        var p = await _db.Productos.AsNoTracking()
            .Where(x => x.Id == id && x.Activo)
            .Select(x => new
            {
                x.Id, x.Nombre, x.Slug, x.Descripcion, x.Marca,
                Categoria = x.Categoria.Nombre,
                x.GarantiaMeses, x.Especificaciones, x.Imagenes, x.Relacionados
            })
            .FirstOrDefaultAsync();

        if (p is null)
            return NotFound(new ProblemDetails { Title = "Producto no encontrado.", Status = 404 });

        var variantes = await _db.VariantesProducto.AsNoTracking()
            .Where(v => v.ProductoId == id && v.Activo)
            .OrderBy(v => v.Precio)
            .Select(v => new VarianteDto(
                v.Id, v.Sku, v.NombreVariante, v.Precio, v.PrecioComparacion,
                _db.InventarioStock.Where(s => s.VarianteId == v.Id).Sum(s => (int?)s.Cantidad) ?? 0,
                v.ImagenUrl))
            .ToListAsync();

        return new ProductoDetalleDto(
            p.Id, p.Nombre, p.Slug, p.Descripcion, p.Marca, p.Categoria, p.GarantiaMeses,
            LeerJson(p.Especificaciones), LeerJson(p.Imagenes), LeerJson(p.Relacionados),
            variantes);
    }

    /// <summary>Lista plana de categorías (usar CategoriaPadreId para armar el árbol).</summary>
    [HttpGet("/api/categorias")]
    public async Task<ActionResult<List<CategoriaDto>>> Categorias() =>
        await _db.Categorias.AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaDto(c.Id, c.Nombre, c.Slug, c.CategoriaPadreId))
            .ToListAsync();

    // ---------------- Imágenes ----------------

    /// <summary>Imágenes del producto en orden (la principal marcada).</summary>
    [HttpGet("{id:int}/imagenes")]
    public async Task<ActionResult<List<ImagenDto>>> Imagenes(int id)
    {
        var p = await _db.Productos.AsNoTracking().Where(x => x.Id == id).Select(x => new { x.Imagenes }).FirstOrDefaultAsync();
        if (p is null) return NotFound(Problema("Producto no encontrado.", 404));
        return ImagenesProducto.Leer(p.Imagenes);
    }

    /// <summary>Agrega una imagen por URL (solo Admin). La primera imagen queda como principal.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/imagenes")]
    public async Task<ActionResult<List<ImagenDto>>> AgregarImagen(int id, AgregarImagenDto dto)
    {
        var url = dto.Url.Trim();
        if (!ImagenesProducto.UrlValida(url))
            return BadRequest(Problema("La URL debe ser absoluta y empezar con http:// o https://.", 400));

        var producto = await _db.Productos.FirstOrDefaultAsync(x => x.Id == id);
        if (producto is null) return NotFound(Problema("Producto no encontrado.", 404));

        var lista = ImagenesProducto.Leer(producto.Imagenes);
        if (lista.Any(i => string.Equals(i.Url, url, StringComparison.OrdinalIgnoreCase)))
            return Conflict(Problema("Esa imagen ya está agregada.", 409));

        if (dto.Principal) lista = lista.Select(i => i with { Principal = false }).ToList();
        lista.Add(new ImagenDto(url, lista.Count + 1, dto.Principal || lista.Count == 0));

        return await Guardar(producto, lista);
    }

    /// <summary>Marca como principal la imagen en la posición indicada (0 = primera). Solo Admin.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/imagenes/{indice:int}/principal")]
    public async Task<ActionResult<List<ImagenDto>>> HacerPrincipal(int id, int indice)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(x => x.Id == id);
        if (producto is null) return NotFound(Problema("Producto no encontrado.", 404));

        var lista = ImagenesProducto.Leer(producto.Imagenes);
        if (indice < 0 || indice >= lista.Count) return NotFound(Problema("La imagen no existe.", 404));

        // La principal se mueve al primer lugar
        var elegida = lista[indice];
        lista.RemoveAt(indice);
        lista.Insert(0, elegida);
        lista = lista.Select((i, n) => i with { Principal = n == 0 }).ToList();

        return await Guardar(producto, lista);
    }

    /// <summary>Quita la imagen en la posición indicada (0 = primera). Solo Admin.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}/imagenes/{indice:int}")]
    public async Task<ActionResult<List<ImagenDto>>> QuitarImagen(int id, int indice)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(x => x.Id == id);
        if (producto is null) return NotFound(Problema("Producto no encontrado.", 404));

        var lista = ImagenesProducto.Leer(producto.Imagenes);
        if (indice < 0 || indice >= lista.Count) return NotFound(Problema("La imagen no existe.", 404));

        lista.RemoveAt(indice);
        return await Guardar(producto, lista);
    }

    // ---------------- helpers ----------------
    private async Task<List<ImagenDto>> Guardar(Models.Producto producto, List<ImagenDto> lista)
    {
        producto.Imagenes = ImagenesProducto.Escribir(lista);
        await _db.SaveChangesAsync();
        return ImagenesProducto.Leer(producto.Imagenes);
    }

    private static ProblemDetails Problema(string titulo, int status) =>
        new() { Title = titulo, Status = status };

    private static JsonElement? LeerJson(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : JsonDocument.Parse(texto).RootElement.Clone();
}
