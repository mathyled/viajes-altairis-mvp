using Altairis.Domain.Entities;
using Altairis.Domain.Interfaces;
using Altairis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Infrastructure.Repositories;

public class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    public ReservationRepository(AltairisDbContext context) : base(context)
    {
    }

    public override async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(r => r.Hotel)
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public override async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        return await _dbSet
            .Include(r => r.Hotel)
            .Include(r => r.RoomType)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByHotelAsync(int hotelId)
    {
        return await _dbSet
            .Include(r => r.Hotel)
            .Include(r => r.RoomType)
            .Where(r => r.HotelId == hotelId)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByGuestNameAsync(string guestName)
    {
        return await _dbSet
            .Include(r => r.Hotel)
            .Include(r => r.RoomType)
            .Where(r => r.HuespedNombre.Contains(guestName))
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();
    }
}
