using Altairis.Domain.Entities;

namespace Altairis.Domain.Interfaces;

public interface IInventoryRepository : IRepository<Inventory>
{
    Task<Inventory?> GetByHotelRoomAndDateAsync(int hotelId, int roomTypeId, DateTime fecha);
    Task<IEnumerable<Inventory>> GetInventoryRangeAsync(int hotelId, int roomTypeId, DateTime fechaInicio, DateTime fechaFin);
    Task<bool> CheckAvailabilityAsync(int hotelId, int roomTypeId, DateTime fechaInicio, DateTime fechaFin, int cantidadHabitaciones);
    Task ReserveRoomsAsync(int hotelId, int roomTypeId, DateTime fechaInicio, DateTime fechaFin, int cantidadHabitaciones);
}
