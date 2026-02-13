namespace Altairis.Application.DTOs;

public class HotelDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public int Categoria { get; set; }
    public bool Estado { get; set; }
    public List<RoomTypeDto>? RoomTypes { get; set; }
}

public class CreateHotelDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public int Categoria { get; set; }
    public bool Estado { get; set; } = true;
}

public class UpdateHotelDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public int Categoria { get; set; }
    public bool Estado { get; set; }
}

public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
