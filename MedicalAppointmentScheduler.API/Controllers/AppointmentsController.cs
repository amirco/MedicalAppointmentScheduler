using Microsoft.AspNetCore.Mvc;
using MedicalAppointmentScheduler.API.Models;
using MedicalAppointmentScheduler.API.Services;

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
        var res = await _appointmentService.CreateAppointmentAsync(appointment);
        if(res.alternativeTimes.Count == 0)
            return CreatedAtAction(nameof(GetAppointment), new { id = res.appointment.Id }, res.appointment);
        else
            return Conflict(new
            {
                Message = "Cannot create appointment due to scheduling conflict.",
                AlternativeTimes = res.alternativeTimes
            });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Appointment>> UpdateAppointment(int id, Appointment appointment)
    {
        var updatedAppointment = await _appointmentService.UpdateAppointmentAsync(id, appointment);
        if (updatedAppointment == null)
        {
            return NotFound($"Appointment with ID {id} not found.");
        }
        return Ok(updatedAppointment);
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