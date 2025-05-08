using Microsoft.EntityFrameworkCore;
using MedicalAppointmentScheduler.API.Data;
using MedicalAppointmentScheduler.API.Models;
using MedicalAppointmentScheduler.API.Services;
using Xunit;
using MedicalAppointmentScheduler.API.Exceptions;

namespace MedicalAppointmentScheduler.Tests;

public class AppointmentServiceTests
{
    private readonly AppointmentDbContext _context;
    private readonly AppointmentService _service;

    public AppointmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppointmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppointmentDbContext(options);
        _service = new AppointmentService(_context);
    }

    [Fact]
    public async Task CreateAppointment_NoConflict_ShouldSucceed()
    {
        // Arrange
        var appointment = new Appointment
        {
            PatientName = "Iam Sick",
            HealthcareProfessionalName = "Dr. Smith",
            AppointmentDate = DateTime.Now.AddHours(1),
            Duration = 30,
            Description = "Regular checkup"
        };

        // Act
        var result = await _service.CreateAppointmentAsync(appointment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(appointment.PatientName, result.PatientName);
        Assert.Equal(appointment.HealthcareProfessionalName, result.HealthcareProfessionalName);
        Assert.Equal(appointment.Duration, result.Duration);
        Assert.Equal(appointment.AppointmentDate, result.AppointmentDate);
    }

    public async Task CreateAppointment_WithConflict(DateTime existingAppointmentDate, int existingAppointmentDuration, DateTime newAppointmentDate, int newAppointmentDuration)
    {
        // Arrange
        var existingAppointment = new Appointment
        {
            PatientName = "Iam Sick",
            HealthcareProfessionalName = "Dr. Smith",
            AppointmentDate = existingAppointmentDate,
            Duration = existingAppointmentDuration,
            Description = "Existing appointment"
        };

        await _service.CreateAppointmentAsync(existingAppointment);
        await _context.SaveChangesAsync(); // Ensure the appointment is saved

        var newAppointment = new Appointment
        {
            PatientName = "Iam Sick",
            HealthcareProfessionalName = "Dr. Smith",
            AppointmentDate = newAppointmentDate,
            Duration = newAppointmentDuration,
            Description = "New appointment"
        };

        // Act
        List<DateTime> alternativeTimes = null;
        try
        {
            await _service.CreateAppointmentAsync(newAppointment);
        }
        catch (AppointmentConflictException ex)
        {
            alternativeTimes = ex.AlternativeTimes;
        }

        // Assert
        Assert.NotNull(alternativeTimes);
        Assert.NotEmpty(alternativeTimes);
        Assert.True(alternativeTimes.Count <= 3);
    }

    [Fact]
    public async Task CreateAppointment_WithConflict_Overlap_Start_ShouldDetectConflict()
    {
        await CreateAppointment_WithConflict(DateTime.Now.AddHours(1),30, DateTime.Now.AddHours(1).AddMinutes(15),30);
    }

    [Fact]
    public async Task CreateAppointment_WithConflict_Overlap_End_ShouldDetectConflict()
    {
        await CreateAppointment_WithConflict(DateTime.Now.AddHours(1).AddMinutes(15), 30, DateTime.Now.AddHours(1), 30);
    }

    [Fact]
    public async Task CreateAppointment_WithConflict_Overlap_Inside_ShouldDetectConflict()
    {
        await CreateAppointment_WithConflict(DateTime.Now.AddHours(1), 60, DateTime.Now.AddHours(1).AddMinutes(15), 30);
    }

    [Fact]
    public async Task CreateAppointment_WithConflict_Overlap_End_And_Start_ShouldDetectConflict()
    {
        await CreateAppointment_WithConflict(DateTime.Now.AddHours(1).AddMinutes(15), 30, DateTime.Now.AddHours(1), 60);
    }

    [Fact]
    public async Task UpdateAppointment_ValidId_ShouldUpdate()
    {
        // Arrange
        var appointment = new Appointment
        {
            PatientName = "Iam Sick",
            HealthcareProfessionalName = "Dr. Smith",
            AppointmentDate = DateTime.Now.AddHours(1),
            Duration = 30,
            Description = "Original appointment"
        };

        var created = await _service.CreateAppointmentAsync(appointment);

        // Act
        created.Description = "Updated appointment";
        var updated = await _service.UpdateAppointmentAsync(created.Id, created);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("Updated appointment", updated.Description);
    }

    [Fact]
    public async Task DeleteAppointment_ValidId_ShouldDelete()
    {
        // Arrange
        var appointment = new Appointment
        {
            PatientName = "Iam Sick",
            HealthcareProfessionalName = "Dr. Smith",
            AppointmentDate = DateTime.Now.AddHours(1),
            Duration = 30,
            Description = "Appointment to delete"
        };

        var created = await _service.CreateAppointmentAsync(appointment);

        // Act
        var result = await _service.DeleteAppointmentAsync(created.Id);

        // Assert
        Assert.True(result);
        var deleted = await _service.GetAppointmentByIdAsync(created.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task GetAppointmentById_InvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetAppointmentByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAppointments_ShouldReturnAllAppointments()
    {
        // Arrange
        var appointments = new List<Appointment>
        {
            new Appointment
            {
                PatientName = "Iam Sick",
                HealthcareProfessionalName = "Dr. Smith",   
                AppointmentDate = DateTime.Now.AddHours(1),
                Duration = 30,
                Description = "First appointment"
            },
            new Appointment
            {
                PatientName = "Iam PartialSick",
                HealthcareProfessionalName = "Dr. Smith",
                AppointmentDate = DateTime.Now.AddHours(2),
                Duration = 45,
                Description = "Second appointment"
            }
        };

        foreach (var appointment in appointments)
        {
            await _service.CreateAppointmentAsync(appointment);
        }

        // Act
        var result = await _service.GetAllAppointmentsAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }
} 