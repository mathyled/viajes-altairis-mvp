using Altairis.Domain.Entities;
using Altairis.Domain.Interfaces;
using Altairis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Infrastructure.Repositories;

public class HotelRepository : Repository<Hotel>, IHotelRepository
{
    public HotelRepository(AltairisDbContext context) : base(context)
    {
    }

    public override async Task<Hotel?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(h => h.RoomTypes)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public override async Task<IEnumerable<Hotel>> GetAllAsync()
    {
        return await _dbSet
            .Include(h => h.RoomTypes)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Hotel> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm = null)
    {
        var query = _dbSet.Include(h => h.RoomTypes).AsQueryable();

        // Aplicar búsqueda si existe el término
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(h => 
                h.Nombre.ToLower().Contains(searchTerm) || 
                h.Direccion.ToLower().Contains(searchTerm));
        }

        // Obtener el total de registros
        var totalCount = await query.CountAsync();

        // Aplicar paginación
        var items = await query
            .OrderBy(h => h.Nombre)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Hotel>> GetActiveHotelsAsync()
    {
        return await _dbSet
            .Include(h => h.RoomTypes)
            .Where(h => h.Estado)
            .ToListAsync();
    }
}
