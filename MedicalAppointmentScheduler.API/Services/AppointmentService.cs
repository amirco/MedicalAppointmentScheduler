using MedicalAppointmentScheduler.API.Data;
using MedicalAppointmentScheduler.API.Models;
using MedicalAppointmentScheduler.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointmentScheduler.API.Services;

public class AppointmentService
{
    private readonly AppointmentDbContext _context;
    private static readonly TimeSpan GENERATE_ALTERNATIVE_WORK_START = new TimeSpan(8, 0, 0);  // 8:00 AM
    private static readonly TimeSpan GENERATE_ALTERNATIVE_WORK_END = new TimeSpan(17, 0, 0);   // 5:00 PM
    private static readonly DayOfWeek[] GENERATE_ALTERNATIVE_WORK_DAYS = new[] 
    {
        DayOfWeek.Sunday,
        DayOfWeek.Monday, 
        DayOfWeek.Tuesday, 
        DayOfWeek.Wednesday, 
        DayOfWeek.Thursday, 
    };
    private static readonly int MAX_DAYS_TO_FIND_ALTERNATIVE = 14;
    private static readonly int NUMBER_OF_ALTERNATIVES = 3;
    private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    public AppointmentService(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return await _context.Appointments.ToListAsync();
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int id)
    {
        return await _context.Appointments.FindAsync(id);
    }

    private async Task<List<DateTime>?> CheckForConflictsAsync(Appointment appointment)
    {

        var appointmentEnd = appointment.AppointmentDate.AddMinutes(appointment.Duration);
            
        var existingAppointments = await _context.Appointments
            .Where(a => a.AppointmentDate <= appointmentEnd && 
                            a.AppointmentDate.AddMinutes(a.Duration) >= appointment.AppointmentDate)
            .ToListAsync();

        if (existingAppointments.Any())
        {
            return GenerateAlternativeTimes(appointment, existingAppointments);
        }

        return null;
    }

    public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
    {
        await _lock.WaitAsync();
        try
        {
            var alternativeTimes = await CheckForConflictsAsync(appointment);
            if (alternativeTimes != null)
            {
                throw new AppointmentConflictException(
                    "Cannot create appointment due to scheduling conflict.",
                    alternativeTimes);
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Appointment?> UpdateAppointmentAsync(int id, Appointment appointment)
    {
        await _lock.WaitAsync();
        try
        {
            var existingAppointment = await _context.Appointments.FindAsync(id);
            if (existingAppointment == null)
                return null;

            existingAppointment.PatientName = appointment.PatientName;
            existingAppointment.HealthcareProfessionalName = appointment.HealthcareProfessionalName;
            existingAppointment.AppointmentDate = appointment.AppointmentDate;
            existingAppointment.Duration = appointment.Duration;
            existingAppointment.Description = appointment.Description;

            await _context.SaveChangesAsync();
            return existingAppointment;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteAppointmentAsync(int id)
    {
        await _lock.WaitAsync();
        try
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private List<DateTime> GenerateAlternativeTimes(Appointment appointment, List<Appointment> existingAppointments)
    {
        var alternatives = new List<DateTime>();
        var currentDate = appointment.AppointmentDate.Date;
        var busyTimes = existingAppointments
            .Select(a => (Start: a.AppointmentDate, End: a.AppointmentDate.AddMinutes(a.Duration)))
            .ToList();

        for (int day = 0; day < MAX_DAYS_TO_FIND_ALTERNATIVE && alternatives.Count < NUMBER_OF_ALTERNATIVES; day++)
        {
            var checkDate = currentDate.AddDays(day);
            
            // Skip if not a working day
            if (!GENERATE_ALTERNATIVE_WORK_DAYS.Contains(checkDate.DayOfWeek))
                continue;

            // Start from work start time or current time if it's today
            var startTime = checkDate == appointment.AppointmentDate.Date 
                ? appointment.AppointmentDate.TimeOfDay 
                : GENERATE_ALTERNATIVE_WORK_START;

            // End at work end time
            var endTime = GENERATE_ALTERNATIVE_WORK_END;

            // Convert to DateTime for the current day
            var currentTime = checkDate.Add(startTime);
            var dayEnd = checkDate.Add(endTime).AddMinutes(-appointment.Duration);

            while (currentTime < dayEnd && alternatives.Count < NUMBER_OF_ALTERNATIVES)
            {
                var potentialEnd = currentTime.AddMinutes(appointment.Duration);

                if (!busyTimes.Any(b => currentTime < b.End && potentialEnd > b.Start))
                {
                    alternatives.Add(currentTime);
                }

                currentTime = currentTime.AddMinutes(30);
            }
        }

        return alternatives;
    }
} 