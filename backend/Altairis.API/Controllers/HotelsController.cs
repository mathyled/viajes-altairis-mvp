using Altairis.Application.DTOs;
using Altairis.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Altairis.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotelsController : ControllerBase
{
    private readonly IHotelService _hotelService;
    private readonly ILogger<HotelsController> _logger;

    public HotelsController(IHotelService hotelService, ILogger<HotelsController> logger)
    {
        _hotelService = hotelService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los hoteles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HotelDto>>> GetAllHotels()
    {
        try
        {
            var hotels = await _hotelService.GetAllHotelsAsync();
            return Ok(hotels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener hoteles");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene hoteles con paginación y búsqueda
    /// </summary>
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResultDto<HotelDto>>> GetPagedHotels(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("El número de página y el tamaño de página deben ser mayores a 0");
            }

            var result = await _hotelService.GetPagedHotelsAsync(pageNumber, pageSize, searchTerm);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener hoteles paginados");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene solo los hoteles activos
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<HotelDto>>> GetActiveHotels()
    {
        try
        {
            var hotels = await _hotelService.GetActiveHotelsAsync();
            return Ok(hotels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener hoteles activos");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene un hotel por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<HotelDto>> GetHotelById(int id)
    {
        try
        {
            var hotel = await _hotelService.GetHotelByIdAsync(id);
            
            if (hotel == null)
            {
                return NotFound($"Hotel con ID {id} no encontrado");
            }

            return Ok(hotel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener hotel por ID");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea un nuevo hotel
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<HotelDto>> CreateHotel([FromBody] CreateHotelDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hotel = await _hotelService.CreateHotelAsync(dto);
            return CreatedAtAction(nameof(GetHotelById), new { id = hotel.Id }, hotel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear hotel");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza un hotel existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<HotelDto>> UpdateHotel(int id, [FromBody] UpdateHotelDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hotel = await _hotelService.UpdateHotelAsync(id, dto);
            
            if (hotel == null)
            {
                return NotFound($"Hotel con ID {id} no encontrado");
            }

            return Ok(hotel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar hotel");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Elimina un hotel
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHotel(int id)
    {
        try
        {
            var result = await _hotelService.DeleteHotelAsync(id);
            
            if (!result)
            {
                return NotFound($"Hotel con ID {id} no encontrado");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar hotel");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
