using Altairis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Infrastructure.Data;

/// <summary>
/// Clase estática para inicializar la base de datos con datos de prueba (Seed Data).
/// Se ejecuta automáticamente al iniciar la aplicación si la base de datos está vacía.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Inicializa la base de datos con datos de prueba si está vacía.
    /// Crea 10 hoteles, tipos de habitación, inventario para el próximo mes y reservas de ejemplo.
    /// </summary>
    public static async Task SeedAsync(AltairisDbContext context)
    {
        // Verificar si ya hay datos en la tabla Hotels
        if (await context.Hotels.AnyAsync())
        {
            return; // La base de datos ya tiene datos, no se ejecuta el seed
        }

        // Seed de 10 Hoteles variados con nombres reales y direcciones
        var hotels = new List<Hotel>
        {
            new Hotel
            {
                Nombre = "Hotel Ritz Madrid",
                Direccion = "Plaza de la Lealtad, 5, 28014 Madrid, España",
                Categoria = 5,
                Estado = true
            },
            new Hotel
            {
                Nombre = "Hotel Marriott Barcelona",
                Direccion = "Avinguda Diagonal, 250, 08029 Barcelona, España",
                Categoria = 5,
                Estado = true
            },
            new Hotel
            {
                Nombre = "Hotel NH Collection Valencia",
                Direccion = "Calle de Xàtiva, 14, 46004 Valencia, España",
                Categoria = 4,
                Estado = true
            },
            new Hotel
            {
                Nombre = "Hotel AC Málaga",
                Direccion = "Calle Cortina del Muelle, 1, 29015 Málaga, España",
                Categoria = 4,
                Estado = true
            },
            new Hotel
            {
                Nombre = "Hotel Silken Gran Hotel Domine",
                Direccion = "Alameda de Mazarredo, 61, 48009 Bilbao, España",
                Categoria = 5,
                Estado = true
            },
            new Hotel
            {
                Nombre = "Hotel Tryp Sevilla",
                Direccion = "Av. de la Constitución, 13, 41004 Sevilla, España",
                Categoria = 4,
                Estado = true
            },
            new Hotel
            {
                Nombre = "Hotel Eurostars Las Letras",
                Direccion = "Calle de las Huertas, 11, 28012 Madrid, España",
                Categoria = 4,
                Estado = true
            },
            new Hotel
            {
                Nombre = "Hotel Boutique Casa del Poeta",
                Direccion = "Calle de los Reyes Católicos, 4, 18009 Granada, España",
                Categoria = 3,
                Estado = true
            },
            new Hotel
            {
                Nombre = "Hotel Occidental Bilbao",
                Direccion = "Calle de Rodríguez Arias, 66, 48013 Bilbao, España",
                Categoria = 4,
                Estado = true
            },
            new Hotel
            {
                Nombre = "Hotel Zenit Conde de Orgaz",
                Direccion = "Calle del Conde de Orgaz, 5, 28027 Madrid, España",
                Categoria = 3,
                Estado = false // Inactivo para pruebas
            }
        };

        await context.Hotels.AddRangeAsync(hotels);
        await context.SaveChangesAsync();

        // Seed Tipos de Habitación para cada hotel
        var roomTypes = new List<RoomType>();
        
        foreach (var hotel in hotels)
        {
            // Hoteles de 5 estrellas tienen más opciones de suites
            if (hotel.Categoria == 5)
            {
                roomTypes.AddRange(new[]
                {
                    new RoomType
                    {
                        Nombre = "Habitación Estándar",
                        PrecioBase = hotel.Categoria * 60m,
                        HotelId = hotel.Id
                    },
                    new RoomType
                    {
                        Nombre = "Habitación Superior",
                        PrecioBase = hotel.Categoria * 90m,
                        HotelId = hotel.Id
                    },
                    new RoomType
                    {
                        Nombre = "Suite Junior",
                        PrecioBase = hotel.Categoria * 180m,
                        HotelId = hotel.Id
                    },
                    new RoomType
                    {
                        Nombre = "Suite Ejecutiva",
                        PrecioBase = hotel.Categoria * 250m,
                        HotelId = hotel.Id
                    },
                    new RoomType
                    {
                        Nombre = "Suite Presidencial",
                        PrecioBase = hotel.Categoria * 400m,
                        HotelId = hotel.Id
                    }
                });
            }
            // Hoteles de 4 estrellas tienen opciones estándar
            else if (hotel.Categoria == 4)
            {
                roomTypes.AddRange(new[]
                {
                    new RoomType
                    {
                        Nombre = "Habitación Simple",
                        PrecioBase = hotel.Categoria * 50m,
                        HotelId = hotel.Id
                    },
                    new RoomType
                    {
                        Nombre = "Habitación Doble",
                        PrecioBase = hotel.Categoria * 75m,
                        HotelId = hotel.Id
                    },
                    new RoomType
                    {
                        Nombre = "Habitación Triple",
                        PrecioBase = hotel.Categoria * 100m,
                        HotelId = hotel.Id
                    },
                    new RoomType
                    {
                        Nombre = "Suite",
                        PrecioBase = hotel.Categoria * 150m,
                        HotelId = hotel.Id
                    }
                });
            }
            // Hoteles de 3 estrellas tienen opciones básicas
            else
            {
                roomTypes.AddRange(new[]
                {
                    new RoomType
                    {
                        Nombre = "Habitación Simple",
                        PrecioBase = hotel.Categoria * 40m,
                        HotelId = hotel.Id
                    },
                    new RoomType
                    {
                        Nombre = "Habitación Doble",
                        PrecioBase = hotel.Categoria * 60m,
                        HotelId = hotel.Id
                    },
                    new RoomType
                    {
                        Nombre = "Habitación Familiar",
                        PrecioBase = hotel.Categoria * 85m,
                        HotelId = hotel.Id
                    }
                });
            }
        }

        await context.RoomTypes.AddRangeAsync(roomTypes);
        await context.SaveChangesAsync();

        // Seed Inventario para el próximo mes (30 días)
        var inventories = new List<Inventory>();
        var startDate = DateTime.Today;
        var endDate = startDate.AddDays(30); // Próximo mes
        
        var random = new Random();
        
        foreach (var roomType in roomTypes)
        {
            for (var fecha = startDate; fecha < endDate; fecha = fecha.AddDays(1))
            {
                // Capacidad diferente según tipo de habitación y categoría del hotel
                int capacidadTotal = roomType.Nombre switch
                {
                    "Habitación Estándar" or "Habitación Simple" => 15,
                    "Habitación Superior" or "Habitación Doble" => 20,
                    "Habitación Triple" or "Habitación Familiar" => 12,
                    "Suite Junior" => 8,
                    "Suite Ejecutiva" => 5,
                    "Suite Presidencial" => 2,
                    "Suite" => 6,
                    _ => 10
                };
                
                // Simular algunas reservas aleatorias para hacer el inventario más realista
                // Los fines de semana y días cercanos tienen más ocupación
                int diasDesdeHoy = (fecha - startDate).Days;
                bool esFinDeSemana = fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday;
                bool esProximo = diasDesdeHoy < 7;
                
                int cantidadReservada = 0;
                if (esProximo && esFinDeSemana)
                {
                    // Fines de semana próximos: alta ocupación (60-80%)
                    cantidadReservada = random.Next(
                        (int)(capacidadTotal * 0.6), 
                        (int)(capacidadTotal * 0.8) + 1
                    );
                }
                else if (esProximo)
                {
                    // Días próximos entre semana: ocupación media (30-50%)
                    cantidadReservada = random.Next(
                        (int)(capacidadTotal * 0.3), 
                        (int)(capacidadTotal * 0.5) + 1
                    );
                }
                else if (esFinDeSemana)
                {
                    // Fines de semana futuros: ocupación media-baja (20-40%)
                    cantidadReservada = random.Next(
                        (int)(capacidadTotal * 0.2), 
                        (int)(capacidadTotal * 0.4) + 1
                    );
                }
                else
                {
                    // Días futuros entre semana: ocupación baja (5-20%)
                    cantidadReservada = random.Next(
                        (int)(capacidadTotal * 0.05), 
                        (int)(capacidadTotal * 0.2) + 1
                    );
                }

                // Asegurar que no exceda la capacidad total
                cantidadReservada = Math.Min(cantidadReservada, capacidadTotal);

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

        // Seed Reservas de prueba variadas
        var reservations = new List<Reservation>();
        var nombresHuespedes = new[]
        {
            "Juan Pérez García",
            "María González López",
            "Carlos Ramírez Martínez",
            "Ana Fernández Sánchez",
            "Luis Torres Díaz",
            "Laura Jiménez Ruiz",
            "Pedro Morales Castro",
            "Carmen Vázquez Romero",
            "Miguel Ángel Serrano",
            "Isabel Moreno Navarro"
        };

        var estados = new[] { "Confirmada", "Confirmada", "Confirmada", "Completada", "Cancelada" };
        var randomReservas = new Random(42); // Semilla fija para reproducibilidad

        // Crear reservas distribuidas en diferentes hoteles y tipos de habitación
        for (int i = 0; i < 15; i++)
        {
            var hotel = hotels[randomReservas.Next(hotels.Count)];
            var tiposDisponibles = roomTypes.Where(rt => rt.HotelId == hotel.Id).ToList();
            if (!tiposDisponibles.Any()) continue;

            var tipoHabitacion = tiposDisponibles[randomReservas.Next(tiposDisponibles.Count)];
            var diasEstancia = randomReservas.Next(2, 6); // Entre 2 y 5 noches
            var diasDesdeHoy = randomReservas.Next(0, 25); // Reservas en los próximos 25 días
            var fechaEntrada = startDate.AddDays(diasDesdeHoy);
            var fechaSalida = fechaEntrada.AddDays(diasEstancia);
            var estado = estados[randomReservas.Next(estados.Length)];

            // Calcular monto total basado en días de estancia
            var montoTotal = tipoHabitacion.PrecioBase * diasEstancia;

            // Si está cancelada, reducir el monto (penalización del 20%)
            if (estado == "Cancelada")
            {
                montoTotal = montoTotal * 0.2m;
            }

            reservations.Add(new Reservation
            {
                HotelId = hotel.Id,
                RoomTypeId = tipoHabitacion.Id,
                HuespedNombre = nombresHuespedes[i % nombresHuespedes.Length],
                FechaEntrada = fechaEntrada,
                FechaSalida = fechaSalida,
                Estado = estado,
                MontoTotal = montoTotal,
                FechaCreacion = DateTime.UtcNow.AddDays(-randomReservas.Next(1, 30))
            });
        }

        // Agregar algunas reservas pasadas (completadas)
        for (int i = 0; i < 5; i++)
        {
            var hotel = hotels[randomReservas.Next(hotels.Count)];
            var tiposDisponibles = roomTypes.Where(rt => rt.HotelId == hotel.Id).ToList();
            if (!tiposDisponibles.Any()) continue;

            var tipoHabitacion = tiposDisponibles[randomReservas.Next(tiposDisponibles.Count)];
            var diasEstancia = randomReservas.Next(2, 5);
            var diasAtras = randomReservas.Next(5, 30);
            var fechaSalida = startDate.AddDays(-diasAtras);
            var fechaEntrada = fechaSalida.AddDays(-diasEstancia);

            reservations.Add(new Reservation
            {
                HotelId = hotel.Id,
                RoomTypeId = tipoHabitacion.Id,
                HuespedNombre = nombresHuespedes[randomReservas.Next(nombresHuespedes.Length)],
                FechaEntrada = fechaEntrada,
                FechaSalida = fechaSalida,
                Estado = "Completada",
                MontoTotal = tipoHabitacion.PrecioBase * diasEstancia,
                FechaCreacion = fechaEntrada.AddDays(-randomReservas.Next(1, 10))
            });
        }

        await context.Reservations.AddRangeAsync(reservations);
        await context.SaveChangesAsync();
    }
}
