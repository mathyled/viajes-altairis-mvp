using Altairis.Application.DTOs;
using Altairis.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Altairis.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todo el inventario
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetAllInventory()
    {
        try
        {
            var inventory = await _inventoryService.GetAllInventoryAsync();
            return Ok(inventory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener inventario");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene inventario por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryDto>> GetInventoryById(int id)
    {
        try
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            
            if (inventory == null)
            {
                return NotFound($"Inventario con ID {id} no encontrado");
            }

            return Ok(inventory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener inventario");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene inventario por hotel y tipo de habitación en un rango de fechas
    /// </summary>
    [HttpGet("hotel/{hotelId}/roomtype/{roomTypeId}")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventoryByHotelAndRoom(
        int hotelId,
        int roomTypeId,
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin)
    {
        try
        {
            if (fechaInicio >= fechaFin)
            {
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin");
            }

            var inventory = await _inventoryService.GetInventoryByHotelAndRoomAsync(
                hotelId, roomTypeId, fechaInicio, fechaFin);
            
            return Ok(inventory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener inventario por hotel y habitación");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Verifica disponibilidad para una reserva
    /// </summary>
    [HttpPost("check-availability")]
    public async Task<ActionResult<AvailabilityResultDto>> CheckAvailability([FromBody] CheckAvailabilityDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _inventoryService.CheckAvailabilityAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea un nuevo registro de inventario
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<InventoryDto>> CreateInventory([FromBody] CreateInventoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var inventory = await _inventoryService.CreateInventoryAsync(dto);
            return CreatedAtAction(nameof(GetInventoryById), new { id = inventory.Id }, inventory);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear inventario");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea inventario masivo para un rango de fechas
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> BulkCreateInventory([FromBody] BulkCreateInventoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdInventories = await _inventoryService.BulkCreateInventoryAsync(dto);
            return Ok(createdInventories);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear inventario masivo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza un registro de inventario
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<InventoryDto>> UpdateInventory(int id, [FromBody] UpdateInventoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var inventory = await _inventoryService.UpdateInventoryAsync(id, dto);
            
            if (inventory == null)
            {
                return NotFound($"Inventario con ID {id} no encontrado");
            }

            return Ok(inventory);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar inventario");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
