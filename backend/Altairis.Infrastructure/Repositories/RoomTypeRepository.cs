using Altairis.Domain.Entities;
using Altairis.Domain.Interfaces;
using Altairis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Infrastructure.Repositories;

public class RoomTypeRepository : Repository<RoomType>
{
    public RoomTypeRepository(AltairisDbContext context) : base(context)
    {
    }

    public override async Task<RoomType?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(rt => rt.Hotel)
            .FirstOrDefaultAsync(rt => rt.Id == id);
    }

    public override async Task<IEnumerable<RoomType>> GetAllAsync()
    {
        return await _dbSet
            .Include(rt => rt.Hotel)
            .ToListAsync();
    }

    public async Task<IEnumerable<RoomType>> GetByHotelAsync(int hotelId)
    {
        return await _dbSet
            .Include(rt => rt.Hotel)
            .Where(rt => rt.HotelId == hotelId)
            .ToListAsync();
    }
}
