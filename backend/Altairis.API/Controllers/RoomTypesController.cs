using Altairis.Application.DTOs;
using Altairis.Domain.Entities;
using Altairis.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Altairis.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomTypesController : ControllerBase
{
    private readonly IRepository<RoomType> _roomTypeRepository;
    private readonly IRepository<Hotel> _hotelRepository;
    private readonly ILogger<RoomTypesController> _logger;

    public RoomTypesController(
        IRepository<RoomType> roomTypeRepository,
        IRepository<Hotel> hotelRepository,
        ILogger<RoomTypesController> logger)
    {
        _roomTypeRepository = roomTypeRepository;
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los tipos de habitación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomTypeDto>>> GetAllRoomTypes()
    {
        try
        {
            var roomTypes = await _roomTypeRepository.GetAllAsync();
            var dtos = roomTypes.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de habitación");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene un tipo de habitación por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RoomTypeDto>> GetRoomTypeById(int id)
    {
        try
        {
            var roomType = await _roomTypeRepository.GetByIdAsync(id);
            
            if (roomType == null)
            {
                return NotFound($"Tipo de habitación con ID {id} no encontrado");
            }

            return Ok(MapToDto(roomType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipo de habitación");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene tipos de habitación por hotel
    /// </summary>
    [HttpGet("hotel/{hotelId}")]
    public async Task<ActionResult<IEnumerable<RoomTypeDto>>> GetRoomTypesByHotel(int hotelId)
    {
        try
        {
            var roomTypes = await _roomTypeRepository.FindAsync(rt => rt.HotelId == hotelId);
            var dtos = roomTypes.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de habitación por hotel");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea un nuevo tipo de habitación
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RoomTypeDto>> CreateRoomType([FromBody] CreateRoomTypeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var roomType = new RoomType
            {
                Nombre = dto.Nombre,
                PrecioBase = dto.PrecioBase,
                HotelId = dto.HotelId
            };

            var created = await _roomTypeRepository.AddAsync(roomType);
            return CreatedAtAction(nameof(GetRoomTypeById), new { id = created.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tipo de habitación");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza un tipo de habitación
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<RoomTypeDto>> UpdateRoomType(int id, [FromBody] UpdateRoomTypeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var roomType = await _roomTypeRepository.GetByIdAsync(id);
            
            if (roomType == null)
            {
                return NotFound($"Tipo de habitación con ID {id} no encontrado");
            }

            roomType.Nombre = dto.Nombre;
            roomType.PrecioBase = dto.PrecioBase;

            await _roomTypeRepository.UpdateAsync(roomType);
            return Ok(MapToDto(roomType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tipo de habitación");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Elimina un tipo de habitación
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRoomType(int id)
    {
        try
        {
            var roomType = await _roomTypeRepository.GetByIdAsync(id);
            
            if (roomType == null)
            {
                return NotFound($"Tipo de habitación con ID {id} no encontrado");
            }

            await _roomTypeRepository.DeleteAsync(roomType);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar tipo de habitación");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Asigna una lista de tipos de habitación a un hotel
    /// </summary>
    [HttpPost("assign-to-hotel")]
    public async Task<ActionResult> AssignRoomTypesToHotel([FromBody] AssignRoomTypesToHotelDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.RoomTypeIds == null || dto.RoomTypeIds.Count == 0)
            {
                return BadRequest("Debe especificar al menos un tipo de habitación");
            }

            // Verificar que el hotel exista
            var hotel = await _hotelRepository.GetByIdAsync(dto.HotelId);
            if (hotel == null)
            {
                return NotFound($"Hotel con ID {dto.HotelId} no encontrado");
            }

            // Obtener los tipos de habitación a asignar
            var roomTypes = await _roomTypeRepository.FindAsync(rt => dto.RoomTypeIds.Contains(rt.Id));
            var roomTypesList = roomTypes.ToList();

            if (roomTypesList.Count == 0)
            {
                return NotFound("No se encontraron tipos de habitación para los IDs especificados");
            }

            foreach (var roomType in roomTypesList)
            {
                roomType.HotelId = dto.HotelId;
                await _roomTypeRepository.UpdateAsync(roomType);
            }

            return Ok(new
            {
                Message = "Tipos de habitación asignados correctamente",
                HotelId = dto.HotelId,
                AssignedRoomTypeIds = roomTypesList.Select(rt => rt.Id).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar tipos de habitación a un hotel");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    private static RoomTypeDto MapToDto(RoomType roomType)
    {
        return new RoomTypeDto
        {
            Id = roomType.Id,
            Nombre = roomType.Nombre,
            PrecioBase = roomType.PrecioBase,
            HotelId = roomType.HotelId,
            HotelNombre = roomType.Hotel?.Nombre
        };
    }
}
