using Altairis.Application.DTOs;
using Altairis.Domain.Entities;
using Altairis.Domain.Interfaces;

namespace Altairis.Application.Services;

public interface IInventoryService
{
    Task<IEnumerable<InventoryDto>> GetAllInventoryAsync();
    Task<InventoryDto?> GetInventoryByIdAsync(int id);
    Task<IEnumerable<InventoryDto>> GetInventoryByHotelAndRoomAsync(int hotelId, int roomTypeId, DateTime fechaInicio, DateTime fechaFin);
    Task<AvailabilityResultDto> CheckAvailabilityAsync(CheckAvailabilityDto dto);
    Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto dto);
    Task<InventoryDto?> UpdateInventoryAsync(int id, UpdateInventoryDto dto);
    Task<IEnumerable<InventoryDto>> BulkCreateInventoryAsync(BulkCreateInventoryDto dto);
}

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IRepository<Hotel> _hotelRepository;
    private readonly IRepository<RoomType> _roomTypeRepository;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IRepository<Hotel> hotelRepository,
        IRepository<RoomType> roomTypeRepository)
    {
        _inventoryRepository = inventoryRepository;
        _hotelRepository = hotelRepository;
        _roomTypeRepository = roomTypeRepository;
    }

    public async Task<IEnumerable<InventoryDto>> GetAllInventoryAsync()
    {
        var inventories = await _inventoryRepository.GetAllAsync();
        return inventories.Select(MapToDto);
    }

    public async Task<InventoryDto?> GetInventoryByIdAsync(int id)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        return inventory != null ? MapToDto(inventory) : null;
    }

    public async Task<IEnumerable<InventoryDto>> GetInventoryByHotelAndRoomAsync(
        int hotelId, 
        int roomTypeId, 
        DateTime fechaInicio, 
        DateTime fechaFin)
    {
        var inventories = await _inventoryRepository.GetInventoryRangeAsync(
            hotelId, roomTypeId, fechaInicio, fechaFin);
        
        return inventories.Select(MapToDto);
    }

    public async Task<AvailabilityResultDto> CheckAvailabilityAsync(CheckAvailabilityDto dto)
    {
        if (dto.FechaInicio >= dto.FechaFin)
        {
            return new AvailabilityResultDto
            {
                Available = false,
                Message = "The start date must be before the end date"
            };
        }

        if (dto.CantidadHabitaciones <= 0)
        {
            return new AvailabilityResultDto
            {
                Available = false,
                Message = "The number of rooms must be greater than 0"
            };
        }

        var isAvailable = await _inventoryRepository.CheckAvailabilityAsync(
            dto.HotelId,
            dto.RoomTypeId,
            dto.FechaInicio,
            dto.FechaFin,
            dto.CantidadHabitaciones);

        if (isAvailable)
        {
            var inventories = await _inventoryRepository.GetInventoryRangeAsync(
                dto.HotelId, dto.RoomTypeId, dto.FechaInicio, dto.FechaFin);

            return new AvailabilityResultDto
            {
                Available = true,
                Message = "There is availability for the selected dates",
                InventoryDetails = inventories.Select(MapToDto).ToList()
            };
        }

        return new AvailabilityResultDto
        {
            Available = false,
            Message = "No availability sufficient for the selected dates"
        };
    }

    public async Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto dto)
    {
        // Verificar si ya existe inventario para esa fecha
        var existingInventory = await _inventoryRepository.GetByHotelRoomAndDateAsync(
            dto.HotelId, dto.RoomTypeId, dto.Fecha);

        if (existingInventory != null)
        {
            throw new InvalidOperationException("Inventory already exists for this date");
        }

        var inventory = new Inventory
        {
            HotelId = dto.HotelId,
            RoomTypeId = dto.RoomTypeId,
            Fecha = dto.Fecha.Date,
            CantidadTotal = dto.CantidadTotal,
            CantidadReservada = 0
        };

        var createdInventory = await _inventoryRepository.AddAsync(inventory);
        return MapToDto(createdInventory);
    }

    public async Task<InventoryDto?> UpdateInventoryAsync(int id, UpdateInventoryDto dto)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null) return null;

        if (dto.CantidadReservada > dto.CantidadTotal)
        {
            throw new InvalidOperationException("The reserved quantity cannot be greater than the total quantity");
        }

        inventory.CantidadTotal = dto.CantidadTotal;
        inventory.CantidadReservada = dto.CantidadReservada;

        await _inventoryRepository.UpdateAsync(inventory);
        return MapToDto(inventory);
    }

    public async Task<IEnumerable<InventoryDto>> BulkCreateInventoryAsync(BulkCreateInventoryDto dto)
    {
        if (dto.FechaInicio >= dto.FechaFin)
        {
            throw new InvalidOperationException("The start date must be before the end date");
        }

        if (dto.CantidadTotal <= 0)
        {
            throw new InvalidOperationException("The total quantity must be greater than 0");
        }

        var createdInventories = new List<InventoryDto>();

        var startDate = dto.FechaInicio.Date;
        var endDate = dto.FechaFin.Date;

        // Número de noches (excluye el día de salida), consistente con la lógica de reservas
        var days = (endDate - startDate).Days;

        if (days <= 0)
        {
            throw new InvalidOperationException("The date range must contain at least one night");
        }

        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);

            // Si ya existe inventario para esa fecha, lo dejamos tal cual (no sobreescribimos)
            var existing = await _inventoryRepository.GetByHotelRoomAndDateAsync(
                dto.HotelId,
                dto.RoomTypeId,
                date);

            if (existing != null)
            {
                continue;
            }

            var inventory = new Inventory
            {
                HotelId = dto.HotelId,
                RoomTypeId = dto.RoomTypeId,
                Fecha = date,
                CantidadTotal = dto.CantidadTotal,
                CantidadReservada = 0
            };

            var created = await _inventoryRepository.AddAsync(inventory);
            createdInventories.Add(MapToDto(created));
        }

        return createdInventories;
    }

    private static InventoryDto MapToDto(Inventory inventory)
    {
        return new InventoryDto
        {
            Id = inventory.Id,
            HotelId = inventory.HotelId,
            RoomTypeId = inventory.RoomTypeId,
            Fecha = inventory.Fecha,
            CantidadTotal = inventory.CantidadTotal,
            CantidadReservada = inventory.CantidadReservada,
            CantidadDisponible = inventory.CantidadDisponible,
            HotelNombre = inventory.Hotel?.Nombre,
            RoomTypeName = inventory.RoomType?.Nombre
        };
    }
}
