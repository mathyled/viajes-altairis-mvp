using Altairis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AltairisDbContext context)
    {
        // Verificar si ya hay datos
        if (await context.Hotels.AnyAsync())
        {
            return; // La base de datos ya tiene datos
        }

        // Seed Hoteles
        var hotels = new List<Hotel>
        {
            new Hotel
            {
                Nombre = "Hotel Plaza Central",
                Direccion = "Av. Principal 123, Ciudad Capital",
                Categoria = 5,
                Estado = true
            },
            new Hotel
            {
                Nombre = "Hotel Costanera",
                Direccion = "Paseo Marítimo 456, Costa Azul",
                Categoria = 4,
                Estado = true
            },
            new Hotel
            {
                Nombre = "Hotel Mountain View",
                Direccion = "Calle Sierra 789, Montaña Alta",
                Categoria = 3,
                Estado = true
            },
            new Hotel
            {
                Nombre = "Hotel Business Center",
                Direccion = "Zona Empresarial 321, Distrito Financiero",
                Categoria = 4,
                Estado = true
            },
            new Hotel
            {
                Nombre = "Hotel Boutique Aurora",
                Direccion = "Calle Colonial 555, Centro Histórico",
                Categoria = 5,
                Estado = false // Inactivo para pruebas
            }
        };

        await context.Hotels.AddRangeAsync(hotels);
        await context.SaveChangesAsync();

        // Seed Tipos de Habitación para cada hotel
        var roomTypes = new List<RoomType>();
        
        foreach (var hotel in hotels)
        {
            roomTypes.AddRange(new[]
            {
                new RoomType
                {
                    Nombre = "Simple",
                    PrecioBase = hotel.Categoria * 50m, // Precio base según categoría
                    HotelId = hotel.Id
                },
                new RoomType
                {
                    Nombre = "Doble",
                    PrecioBase = hotel.Categoria * 80m,
                    HotelId = hotel.Id
                },
                new RoomType
                {
                    Nombre = "Suite",
                    PrecioBase = hotel.Categoria * 150m,
                    HotelId = hotel.Id
                },
                new RoomType
                {
                    Nombre = "Suite Presidencial",
                    PrecioBase = hotel.Categoria * 300m,
                    HotelId = hotel.Id
                }
            });
        }

        await context.RoomTypes.AddRangeAsync(roomTypes);
        await context.SaveChangesAsync();

        // Seed Inventario para los próximos 60 días
        var inventories = new List<Inventory>();
        var startDate = DateTime.Today;
        
        foreach (var roomType in roomTypes)
        {
            for (int i = 0; i < 60; i++)
            {
                var fecha = startDate.AddDays(i);
                
                // Capacidad diferente según tipo de habitación
                int capacidadTotal = roomType.Nombre switch
                {
                    "Simple" => 10,
                    "Doble" => 15,
                    "Suite" => 5,
                    "Suite Presidencial" => 2,
                    _ => 10
                };
                
                // Algunas habitaciones con reservas iniciales (para demostración)
                int cantidadReservada = i < 7 && roomType.Nombre == "Doble" ? 5 : 0;

                inventories.Add(new Inventory
                {
                    HotelId = roomType.HotelId,
                    RoomTypeId = roomType.Id,
                    Fecha = fecha,
                    CantidadTotal = capacidadTotal,
                    CantidadReservada = cantidadReservada
                });
            }
        }

        await context.Inventories.AddRangeAsync(inventories);
        await context.SaveChangesAsync();

        // Seed Reservas de ejemplo
        var reservations = new List<Reservation>();
        var firstHotel = hotels.First();
        var dobleRoomType = roomTypes.First(rt => rt.Nombre == "Doble" && rt.HotelId == firstHotel.Id);

        reservations.AddRange(new[]
        {
            new Reservation
            {
                HotelId = firstHotel.Id,
                RoomTypeId = dobleRoomType.Id,
                HuespedNombre = "Juan Pérez",
                FechaEntrada = startDate.AddDays(2),
                FechaSalida = startDate.AddDays(5),
                Estado = "Confirmada",
                MontoTotal = dobleRoomType.PrecioBase * 3,
                FechaCreacion = DateTime.UtcNow
            },
            new Reservation
            {
                HotelId = firstHotel.Id,
                RoomTypeId = dobleRoomType.Id,
                HuespedNombre = "María González",
                FechaEntrada = startDate.AddDays(3),
                FechaSalida = startDate.AddDays(6),
                Estado = "Confirmada",
                MontoTotal = dobleRoomType.PrecioBase * 3,
                FechaCreacion = DateTime.UtcNow
            },
            new Reservation
            {
                HotelId = firstHotel.Id,
                RoomTypeId = dobleRoomType.Id,
                HuespedNombre = "Carlos Ramírez",
                FechaEntrada = startDate.AddDays(-5),
                FechaSalida = startDate.AddDays(-2),
                Estado = "Completada",
                MontoTotal = dobleRoomType.PrecioBase * 3,
                FechaCreacion = DateTime.UtcNow.AddDays(-10)
            }
        });

        await context.Reservations.AddRangeAsync(reservations);
        await context.SaveChangesAsync();
    }
}
