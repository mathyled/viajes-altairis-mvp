namespace Altairis.Domain.Entities;

public class Hotel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public int Categoria { get; set; } // Número de estrellas (1-5)
    public bool Estado { get; set; } // true = activo, false = inactivo
    
    // Relaciones
    public virtual ICollection<RoomType> RoomTypes { get; set; } = new List<RoomType>();
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
