using Altairis.Application.DTOs;
using Altairis.Domain.Entities;
using Altairis.Domain.Interfaces;

namespace Altairis.Application.Services;

public interface IHotelService
{
    Task<IEnumerable<HotelDto>> GetAllHotelsAsync();
    Task<HotelDto?> GetHotelByIdAsync(int id);
    Task<PagedResultDto<HotelDto>> GetPagedHotelsAsync(int pageNumber, int pageSize, string? searchTerm);
    Task<IEnumerable<HotelDto>> GetActiveHotelsAsync();
    Task<HotelDto> CreateHotelAsync(CreateHotelDto dto);
    Task<HotelDto?> UpdateHotelAsync(int id, UpdateHotelDto dto);
    Task<bool> DeleteHotelAsync(int id);
}

public class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepository;

    public HotelService(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<IEnumerable<HotelDto>> GetAllHotelsAsync()
    {
        var hotels = await _hotelRepository.GetAllAsync();
        return hotels.Select(MapToDto);
    }

    public async Task<HotelDto?> GetHotelByIdAsync(int id)
    {
        var hotel = await _hotelRepository.GetByIdAsync(id);
        return hotel != null ? MapToDto(hotel) : null;
    }

    public async Task<PagedResultDto<HotelDto>> GetPagedHotelsAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm)
    {
        var (items, totalCount) = await _hotelRepository.GetPagedAsync(pageNumber, pageSize, searchTerm);
        
        return new PagedResultDto<HotelDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<HotelDto>> GetActiveHotelsAsync()
    {
        var hotels = await _hotelRepository.GetActiveHotelsAsync();
        return hotels.Select(MapToDto);
    }

    public async Task<HotelDto> CreateHotelAsync(CreateHotelDto dto)
    {
        var hotel = new Hotel
        {
            Nombre = dto.Nombre,
            Direccion = dto.Direccion,
            Categoria = dto.Categoria,
            Estado = dto.Estado
        };

        var createdHotel = await _hotelRepository.AddAsync(hotel);
        return MapToDto(createdHotel);
    }

    public async Task<HotelDto?> UpdateHotelAsync(int id, UpdateHotelDto dto)
    {
        var hotel = await _hotelRepository.GetByIdAsync(id);
        if (hotel == null) return null;

        hotel.Nombre = dto.Nombre;
        hotel.Direccion = dto.Direccion;
        hotel.Categoria = dto.Categoria;
        hotel.Estado = dto.Estado;

        await _hotelRepository.UpdateAsync(hotel);
        return MapToDto(hotel);
    }

    public async Task<bool> DeleteHotelAsync(int id)
    {
        var hotel = await _hotelRepository.GetByIdAsync(id);
        if (hotel == null) return false;

        await _hotelRepository.DeleteAsync(hotel);
        return true;
    }

    private static HotelDto MapToDto(Hotel hotel)
    {
        return new HotelDto
        {
            Id = hotel.Id,
            Nombre = hotel.Nombre,
            Direccion = hotel.Direccion,
            Categoria = hotel.Categoria,
            Estado = hotel.Estado,
            RoomTypes = hotel.RoomTypes?.Select(rt => new RoomTypeDto
            {
                Id = rt.Id,
                Nombre = rt.Nombre,
                PrecioBase = rt.PrecioBase,
                HotelId = rt.HotelId
            }).ToList()
        };
    }
}
