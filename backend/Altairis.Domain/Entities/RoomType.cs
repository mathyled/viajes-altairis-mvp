namespace Altairis.Domain.Entities;

public class RoomType
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty; // Simple, Doble, Suite, etc.
    public decimal PrecioBase { get; set; }
    public int HotelId { get; set; }
    
    // Relaciones
    public virtual Hotel Hotel { get; set; } = null!;
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
