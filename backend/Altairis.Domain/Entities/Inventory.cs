namespace Altairis.Domain.Entities;

public class Inventory
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public int RoomTypeId { get; set; }
    public DateTime Fecha { get; set; }
    public int CantidadTotal { get; set; }
    public int CantidadReservada { get; set; }
    
    // Propiedad calculada
    public int CantidadDisponible => CantidadTotal - CantidadReservada;
    
    // Relaciones
    public virtual Hotel Hotel { get; set; } = null!;
    public virtual RoomType RoomType { get; set; } = null!;
}
