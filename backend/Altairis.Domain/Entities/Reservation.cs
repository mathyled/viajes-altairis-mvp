namespace Altairis.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public int RoomTypeId { get; set; }
    public string HuespedNombre { get; set; } = string.Empty;
    public DateTime FechaEntrada { get; set; }
    public DateTime FechaSalida { get; set; }
    public string Estado { get; set; } = "Confirmada"; // Confirmada, Cancelada, Completada
    public decimal MontoTotal { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    // Relaciones
    public virtual Hotel Hotel { get; set; } = null!;
    public virtual RoomType RoomType { get; set; } = null!;
}
