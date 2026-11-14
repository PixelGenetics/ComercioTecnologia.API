using ComercioTecnologia.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ComercioTecnologia.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Los nombres de DbSet se convierten a snake_case: InventarioStock -> inventario_stock
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Direccion> Direcciones => Set<Direccion>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<VarianteProducto> VariantesProducto => Set<VarianteProducto>();
    public DbSet<Stock> InventarioStock => Set<Stock>();
    public DbSet<ItemUsuario> ItemsUsuario => Set<ItemUsuario>();
    public DbSet<MetodoEnvio> MetodosEnvio => Set<MetodoEnvio>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();
    public DbSet<CatalogoItem> Catalogo => Set<CatalogoItem>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Stock>().HasKey(s => new { s.VarianteId, s.AlmacenId });
        mb.Entity<ItemUsuario>().HasKey(i => new { i.UsuarioId, i.Lista, i.VarianteId });

        mb.Entity<Producto>(e =>
        {
            e.Property(p => p.Especificaciones).HasColumnType("jsonb");
            e.Property(p => p.Imagenes).HasColumnType("jsonb");
            e.Property(p => p.Relacionados).HasColumnType("jsonb");
        });

        mb.Entity<Pedido>()
            .HasMany(p => p.Detalles)
            .WithOne()
            .HasForeignKey(d => d.PedidoId);

        mb.Entity<CatalogoItem>().HasNoKey().ToView("vw_catalogo");
    }
}
