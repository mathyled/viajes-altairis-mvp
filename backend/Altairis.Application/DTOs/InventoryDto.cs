namespace Altairis.Application.DTOs;

public class InventoryDto
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public int RoomTypeId { get; set; }
    public DateTime Fecha { get; set; }
    public int CantidadTotal { get; set; }
    public int CantidadReservada { get; set; }
    public int CantidadDisponible { get; set; }
    public string? HotelNombre { get; set; }
    public string? RoomTypeName { get; set; }
}

public class CreateInventoryDto
{
    public int HotelId { get; set; }
    public int RoomTypeId { get; set; }
    public DateTime Fecha { get; set; }
    public int CantidadTotal { get; set; }
}

public class UpdateInventoryDto
{
    public int CantidadTotal { get; set; }
    public int CantidadReservada { get; set; }
}

public class CheckAvailabilityDto
{
    public int HotelId { get; set; }
    public int RoomTypeId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int CantidadHabitaciones { get; set; }
}

public class AvailabilityResultDto
{
    public bool Available { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<InventoryDto>? InventoryDetails { get; set; }
}

public class BulkCreateInventoryDto
{
    public int HotelId { get; set; }
    public int RoomTypeId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int CantidadTotal { get; set; }
}
