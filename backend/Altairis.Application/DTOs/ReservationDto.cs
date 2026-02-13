namespace Altairis.Application.DTOs;

public class ReservationDto
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public int RoomTypeId { get; set; }
    public string HuespedNombre { get; set; } = string.Empty;
    public DateTime FechaEntrada { get; set; }
    public DateTime FechaSalida { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? HotelNombre { get; set; }
    public string? RoomTypeName { get; set; }
    public int Noches { get; set; }
}

public class CreateReservationDto
{
    public int HotelId { get; set; }
    public int RoomTypeId { get; set; }
    public string HuespedNombre { get; set; } = string.Empty;
    public DateTime FechaEntrada { get; set; }
    public DateTime FechaSalida { get; set; }
    public int CantidadHabitaciones { get; set; } = 1;
}

public class UpdateReservationDto
{
    public string Estado { get; set; } = string.Empty;
}

public class ReservationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ReservationDto? Reservation { get; set; }
}
