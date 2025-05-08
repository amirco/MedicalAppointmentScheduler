using System.ComponentModel.DataAnnotations;

namespace MedicalAppointmentScheduler.API.Models;

public class Appointment
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string PatientName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string HealthcareProfessionalName { get; set; } = string.Empty;

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    [Range(15, 240)] // Appointments between 15 minutes and 4 hours
    public int Duration { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
} 