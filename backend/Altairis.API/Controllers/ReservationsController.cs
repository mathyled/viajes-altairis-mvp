using Altairis.Application.DTOs;
using Altairis.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Altairis.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(IReservationService reservationService, ILogger<ReservationsController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las reservas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllReservations()
    {
        try
        {
            var reservations = await _reservationService.GetAllReservationsAsync();
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reservas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene una reserva por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ReservationDto>> GetReservationById(int id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            
            if (reservation == null)
            {
                return NotFound($"Reserva con ID {id} no encontrada");
            }

            return Ok(reservation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reserva");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene reservas por hotel
    /// </summary>
    [HttpGet("hotel/{hotelId}")]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservationsByHotel(int hotelId)
    {
        try
        {
            var reservations = await _reservationService.GetReservationsByHotelAsync(hotelId);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reservas por hotel");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea una nueva reserva
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ReservationResultDto>> CreateReservation([FromBody] CreateReservationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _reservationService.CreateReservationAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(
                nameof(GetReservationById), 
                new { id = result.Reservation?.Id }, 
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear reserva");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza el estado de una reserva
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ReservationDto>> UpdateReservationStatus(
        int id, 
        [FromBody] UpdateReservationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reservation = await _reservationService.UpdateReservationStatusAsync(id, dto);
            
            if (reservation == null)
            {
                return NotFound($"Reserva con ID {id} no encontrada");
            }

            return Ok(reservation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado de reserva");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Cancela una reserva
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> CancelReservation(int id)
    {
        try
        {
            var result = await _reservationService.CancelReservationAsync(id);
            
            if (!result)
            {
                return NotFound($"Reserva con ID {id} no encontrada");
            }

            return Ok(new { message = "Reserva cancelada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar reserva");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
