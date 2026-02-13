using Altairis.Domain.Entities;
using Altairis.Domain.Interfaces;
using Altairis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Infrastructure.Repositories;

public class InventoryRepository : Repository<Inventory>, IInventoryRepository
{
    public InventoryRepository(AltairisDbContext context) : base(context)
    {
    }

    public async Task<Inventory?> GetByHotelRoomAndDateAsync(int hotelId, int roomTypeId, DateTime fecha)
    {
        fecha = fecha.Date; // Normalizar a fecha sin hora
        
        return await _dbSet
            .FirstOrDefaultAsync(i => 
                i.HotelId == hotelId && 
                i.RoomTypeId == roomTypeId && 
                i.Fecha.Date == fecha);
    }

    public async Task<IEnumerable<Inventory>> GetInventoryRangeAsync(
        int hotelId, 
        int roomTypeId, 
        DateTime fechaInicio, 
        DateTime fechaFin)
    {
        fechaInicio = fechaInicio.Date;
        fechaFin = fechaFin.Date;
        
        return await _dbSet
            .Where(i => 
                i.HotelId == hotelId && 
                i.RoomTypeId == roomTypeId && 
                i.Fecha.Date >= fechaInicio && 
                i.Fecha.Date <= fechaFin)
            .OrderBy(i => i.Fecha)
            .ToListAsync();
    }

    public async Task<bool> CheckAvailabilityAsync(
        int hotelId, 
        int roomTypeId, 
        DateTime fechaInicio, 
        DateTime fechaFin, 
        int cantidadHabitaciones)
    {
        fechaInicio = fechaInicio.Date;
        fechaFin = fechaFin.Date;

        // Obtener el inventario para el rango de fechas
        var inventarios = await GetInventoryRangeAsync(hotelId, roomTypeId, fechaInicio, fechaFin);
        
        // Calcular los días necesarios (excluyendo el día de salida)
        var diasNecesarios = (int)(fechaFin - fechaInicio).TotalDays;
        
        // Verificar que exista inventario para todos los días necesarios
        if (inventarios.Count() < diasNecesarios)
        {
            return false;
        }

        // Verificar que haya disponibilidad suficiente en cada día
        foreach (var inventario in inventarios.Take(diasNecesarios))
        {
            var disponible = inventario.CantidadTotal - inventario.CantidadReservada;
            if (disponible < cantidadHabitaciones)
            {
                return false;
            }
        }

        return true;
    }

    public async Task ReserveRoomsAsync(
        int hotelId, 
        int roomTypeId, 
        DateTime fechaInicio, 
        DateTime fechaFin, 
        int cantidadHabitaciones)
    {
        fechaInicio = fechaInicio.Date;
        fechaFin = fechaFin.Date;

        // Obtener el inventario para el rango de fechas
        var inventarios = await GetInventoryRangeAsync(hotelId, roomTypeId, fechaInicio, fechaFin);
        
        // Calcular los días necesarios (excluyendo el día de salida)
        var diasNecesarios = (int)(fechaFin - fechaInicio).TotalDays;

        // Actualizar la cantidad reservada en cada día
        foreach (var inventario in inventarios.Take(diasNecesarios))
        {
            inventario.CantidadReservada += cantidadHabitaciones;
            _dbSet.Update(inventario);
        }

        await _context.SaveChangesAsync();
    }
}
