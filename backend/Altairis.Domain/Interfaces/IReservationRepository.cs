using Altairis.Domain.Entities;

namespace Altairis.Domain.Interfaces;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<IEnumerable<Reservation>> GetByHotelAsync(int hotelId);
    Task<IEnumerable<Reservation>> GetByGuestNameAsync(string guestName);
}
