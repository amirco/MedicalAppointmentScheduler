using Microsoft.EntityFrameworkCore;
using MedicalAppointmentScheduler.API.Data;
using MedicalAppointmentScheduler.API.Models;
using MedicalAppointmentScheduler.API.Services;
using Xunit;

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
        Assert.NotNull(result.appointment);
        Assert.NotNull(result.alternativeTimes);
        Assert.Empty(result.alternativeTimes);
        Assert.Equal(appointment.PatientName, result.appointment.PatientName);
        Assert.Equal(appointment.HealthcareProfessionalName, result.appointment.HealthcareProfessionalName);
        Assert.Equal(appointment.Duration, result.appointment.Duration);
        Assert.Equal(appointment.AppointmentDate, result.appointment.AppointmentDate);
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

        var createdFirst = await _service.CreateAppointmentAsync(existingAppointment);
        Assert.NotNull(createdFirst.alternativeTimes);
        Assert.Empty(createdFirst.alternativeTimes);
        await _context.SaveChangesAsync();

        var newAppointment = new Appointment
        {
            PatientName = "Iam Sick",
            HealthcareProfessionalName = "Dr. Smith",
            AppointmentDate = newAppointmentDate,
            Duration = newAppointmentDuration,
            Description = "New appointment"
        };

        // Act
        var createdSecond = await _service.CreateAppointmentAsync(newAppointment);

        // Assert
        Assert.NotNull(createdSecond.alternativeTimes);
        Assert.NotEmpty(createdSecond.alternativeTimes);
        Assert.Equal(3, createdSecond.alternativeTimes.Count);
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
    public async Task UpdateAppointment_Valid_Id_ShouldUpdate()
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
        created.appointment.Description = "Updated appointment";
        var update = await _service.UpdateAppointmentAsync(created.appointment.Id, created.appointment);

        // Assert
        Assert.NotNull(update);
        Assert.Equal("Updated appointment", update.Description);
    }

    [Fact]
    public async Task UpdateAppointment_Valid_Id_ShouldNotUpdate()
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
        created.appointment.Description = "Updated appointment";
        var update = await _service.UpdateAppointmentAsync(9999, created.appointment);

        // Assert
        Assert.Null(update);
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
        var result = await _service.DeleteAppointmentAsync(created.appointment.Id);

        // Assert
        Assert.True(result);
        var deleted = await _service.GetAppointmentByIdAsync(created.appointment.Id);
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

    [Fact]
    public async Task UpdateAppointment_AlternativeTimes_NextDay()
    {
        // Arrange
        var appointment = new Appointment
        {
            PatientName = "Iam Sick",
            HealthcareProfessionalName = "Dr. Smith",
            AppointmentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0).AddDays(1),
            Duration = 45,
            Description = "Original appointment"
        };

        for (int i = 0; i < 23; i++)
        {
            var res = await _service.CreateAppointmentAsync(appointment);
            Assert.NotNull(res.alternativeTimes);
            Assert.Empty(res.alternativeTimes);
            appointment.AppointmentDate = appointment.AppointmentDate.AddHours(1);
        }

        var newAppointment = new Appointment
        {
            PatientName = "Iam Sick",
            HealthcareProfessionalName = "Dr. Smith",
            AppointmentDate = DateTime.Now.AddDays(1),
            Duration = 60,
            Description = "Original appointment"
        };

        // Act
        var created = await _service.CreateAppointmentAsync(newAppointment);
        Assert.NotNull(created.alternativeTimes);
        Assert.Equal(3, created.alternativeTimes.Count);

        // Assert

        foreach (var alternativeTime in created.alternativeTimes)
            Assert.True(alternativeTime.Date != newAppointment.AppointmentDate.Date);
    }

}