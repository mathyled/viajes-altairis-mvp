namespace Altairis.Application.DTOs;

public class RoomTypeDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public int HotelId { get; set; }
    public string? HotelNombre { get; set; }
}

public class CreateRoomTypeDto
{
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public int HotelId { get; set; }
}

public class UpdateRoomTypeDto
{
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
}

public class AssignRoomTypesToHotelDto
{
    public int HotelId { get; set; }
    public List<int> RoomTypeIds { get; set; } = new();
}
