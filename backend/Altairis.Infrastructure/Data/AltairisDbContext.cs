using Altairis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Infrastructure.Data;

public class AltairisDbContext : DbContext
{
    public AltairisDbContext(DbContextOptions<AltairisDbContext> options) : base(options)
    {
    }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Hotel
        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Direccion).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Categoria).IsRequired();
            entity.Property(e => e.Estado).IsRequired();
        });

        // Configuración de RoomType
        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PrecioBase).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Hotel)
                .WithMany(h => h.RoomTypes)
                .HasForeignKey(e => e.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de Inventory
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Fecha).IsRequired();
            entity.Property(e => e.CantidadTotal).IsRequired();
            entity.Property(e => e.CantidadReservada).IsRequired();
            
            // Índice único para evitar duplicados de inventario por fecha
            entity.HasIndex(e => new { e.HotelId, e.RoomTypeId, e.Fecha }).IsUnique();
            
            entity.HasOne(e => e.Hotel)
                .WithMany(h => h.Inventories)
                .HasForeignKey(e => e.HotelId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.RoomType)
                .WithMany(rt => rt.Inventories)
                .HasForeignKey(e => e.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de Reservation
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HuespedNombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FechaEntrada).IsRequired();
            entity.Property(e => e.FechaSalida).IsRequired();
            entity.Property(e => e.Estado).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MontoTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.FechaCreacion).IsRequired();
            
            entity.HasOne(e => e.Hotel)
                .WithMany(h => h.Reservations)
                .HasForeignKey(e => e.HotelId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.RoomType)
                .WithMany(rt => rt.Reservations)
                .HasForeignKey(e => e.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
