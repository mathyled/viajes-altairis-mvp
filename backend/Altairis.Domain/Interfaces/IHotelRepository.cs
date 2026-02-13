using Altairis.Domain.Entities;

namespace Altairis.Domain.Interfaces;

public interface IHotelRepository : IRepository<Hotel>
{
    Task<(IEnumerable<Hotel> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm = null);
    
    Task<IEnumerable<Hotel>> GetActiveHotelsAsync();
}
