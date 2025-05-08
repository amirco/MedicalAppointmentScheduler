using Microsoft.AspNetCore.Mvc;
using MedicalAppointmentScheduler.API.Models;
using MedicalAppointmentScheduler.API.Services;
using MedicalAppointmentScheduler.API.Exceptions;

namespace MedicalAppointmentScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentService _appointmentService;

    public AppointmentsController(AppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Appointment>> GetAppointment(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return NotFound($"Appointment with ID {id} not found.");
        }
        return Ok(appointment);
    }

    [HttpPost]
    public async Task<ActionResult<Appointment>> CreateAppointment(Appointment appointment)
    {
        try
        {
            var createdAppointment = await _appointmentService.CreateAppointmentAsync(appointment);
            return CreatedAtAction(nameof(GetAppointment), new { id = createdAppointment.Id }, createdAppointment);
        }
        catch (AppointmentConflictException ex)
        {
            return Conflict(new
            {
                Message = ex.Message,
                AlternativeTimes = ex.AlternativeTimes
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Appointment>> UpdateAppointment(int id, Appointment appointment)
    {
        try
        {
            var updatedAppointment = await _appointmentService.UpdateAppointmentAsync(id, appointment);
            if (updatedAppointment == null)
            {
                return NotFound($"Appointment with ID {id} not found.");
            }
            return Ok(updatedAppointment);
        }
        catch (AppointmentConflictException ex)
        {
            return Conflict(new
            {
                Message = ex.Message,
                AlternativeTimes = ex.AlternativeTimes
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAppointment(int id)
    {
        var result = await _appointmentService.DeleteAppointmentAsync(id);
        if (!result)
        {
            return NotFound($"Appointment with ID {id} not found.");
        }
        return NoContent();
    }
} 