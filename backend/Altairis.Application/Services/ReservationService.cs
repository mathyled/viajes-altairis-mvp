using Altairis.Application.DTOs;
using Altairis.Domain.Entities;
using Altairis.Domain.Interfaces;

namespace Altairis.Application.Services;

public interface IReservationService
{
    Task<IEnumerable<ReservationDto>> GetAllReservationsAsync();
    Task<ReservationDto?> GetReservationByIdAsync(int id);
    Task<IEnumerable<ReservationDto>> GetReservationsByHotelAsync(int hotelId);
    Task<ReservationResultDto> CreateReservationAsync(CreateReservationDto dto);
    Task<ReservationDto?> UpdateReservationStatusAsync(int id, UpdateReservationDto dto);
    Task<bool> CancelReservationAsync(int id);
}

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IRepository<RoomType> _roomTypeRepository;
    private readonly IRepository<Hotel> _hotelRepository;

    public ReservationService(
        IReservationRepository reservationRepository,
        IInventoryRepository inventoryRepository,
        IRepository<RoomType> roomTypeRepository,
        IRepository<Hotel> hotelRepository)
    {
        _reservationRepository = reservationRepository;
        _inventoryRepository = inventoryRepository;
        _roomTypeRepository = roomTypeRepository;
        _hotelRepository = hotelRepository;
    }

    public async Task<IEnumerable<ReservationDto>> GetAllReservationsAsync()
    {
        var reservations = await _reservationRepository.GetAllAsync();
        return reservations.Select(MapToDto);
    }

    public async Task<ReservationDto?> GetReservationByIdAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        return reservation != null ? MapToDto(reservation) : null;
    }

    public async Task<IEnumerable<ReservationDto>> GetReservationsByHotelAsync(int hotelId)
    {
        var reservations = await _reservationRepository.GetByHotelAsync(hotelId);
        return reservations.Select(MapToDto);
    }

    public async Task<ReservationResultDto> CreateReservationAsync(CreateReservationDto dto)
    {
        // Validaciones básicas
        if (dto.FechaEntrada < DateTime.Today)
        {
            return new ReservationResultDto
            {
                Success = false,
                Message = "The entry date cannot be in the past"
            };
        }

        if (dto.FechaSalida <= dto.FechaEntrada)
        {
            return new ReservationResultDto
            {
                Success = false,
                Message = "The exit date must be after the entry date"
            };
        }

        if (dto.CantidadHabitaciones <= 0)
        {
            return new ReservationResultDto
            {
                Success = false,
                Message = "The number of rooms must be greater than 0"
            };
        }

        // Verificar que el hotel y tipo de habitación existan
        var hotel = await _hotelRepository.GetByIdAsync(dto.HotelId);
        if (hotel == null)
        {
            return new ReservationResultDto
            {
                Success = false,
                Message = "Hotel not found"
            };
        }

        if (!hotel.Estado)
        {
            return new ReservationResultDto
            {
                Success = false,
                Message = "The hotel is not active"
            };
        }

        var roomType = await _roomTypeRepository.GetByIdAsync(dto.RoomTypeId);
        if (roomType == null)
        {
            return new ReservationResultDto
            {
                Success = false,
                Message = "Room type not found"
            };
        }

        // Verificar disponibilidad
        var isAvailable = await _inventoryRepository.CheckAvailabilityAsync(
            dto.HotelId,
            dto.RoomTypeId,
            dto.FechaEntrada,
            dto.FechaSalida,
            dto.CantidadHabitaciones);

        if (!isAvailable)
        {
            return new ReservationResultDto
            {
                Success = false,
                Message = "No availability for the selected dates"
            };
        }

        // Calcular el monto total
        var noches = (dto.FechaSalida - dto.FechaEntrada).Days;
        var montoTotal = roomType.PrecioBase * noches * dto.CantidadHabitaciones;

        // Crear la reserva
        var reservation = new Reservation
        {
            HotelId = dto.HotelId,
            RoomTypeId = dto.RoomTypeId,
            HuespedNombre = dto.HuespedNombre,
            FechaEntrada = dto.FechaEntrada,
            FechaSalida = dto.FechaSalida,
            Estado = "Confirmed",
            MontoTotal = montoTotal,
            FechaCreacion = DateTime.UtcNow
        };

        var createdReservation = await _reservationRepository.AddAsync(reservation);

        // Actualizar el inventario
        await _inventoryRepository.ReserveRoomsAsync(
            dto.HotelId,
            dto.RoomTypeId,
            dto.FechaEntrada,
            dto.FechaSalida,
            dto.CantidadHabitaciones);

        // Obtener la reserva completa con relaciones
        var fullReservation = await _reservationRepository.GetByIdAsync(createdReservation.Id);

        return new ReservationResultDto
        {
            Success = true,
            Message = "Reservation created successfully",
            Reservation = fullReservation != null ? MapToDto(fullReservation) : null
        };
    }

    public async Task<ReservationDto?> UpdateReservationStatusAsync(int id, UpdateReservationDto dto)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null) return null;

        reservation.Estado = dto.Estado;
        await _reservationRepository.UpdateAsync(reservation);

        return MapToDto(reservation);
    }

    public async Task<bool> CancelReservationAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null) return false;

        reservation.Estado = "Cancelled";
        await _reservationRepository.UpdateAsync(reservation);

        // TODO: Liberar el inventario cuando se cancela una reserva
        // Esto requeriría un método adicional en el repository

        return true;
    }

    private static ReservationDto MapToDto(Reservation reservation)
    {
        return new ReservationDto
        {
            Id = reservation.Id,
            HotelId = reservation.HotelId,
            RoomTypeId = reservation.RoomTypeId,
            HuespedNombre = reservation.HuespedNombre,
            FechaEntrada = reservation.FechaEntrada,
            FechaSalida = reservation.FechaSalida,
            Estado = reservation.Estado,
            MontoTotal = reservation.MontoTotal,
            FechaCreacion = reservation.FechaCreacion,
            HotelNombre = reservation.Hotel?.Nombre,
            RoomTypeName = reservation.RoomType?.Nombre,
            Noches = (reservation.FechaSalida - reservation.FechaEntrada).Days
        };
    }
}
