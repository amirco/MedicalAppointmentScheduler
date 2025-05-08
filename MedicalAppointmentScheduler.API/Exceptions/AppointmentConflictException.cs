using System;

namespace MedicalAppointmentScheduler.API.Exceptions;

public class AppointmentConflictException : InvalidOperationException
{
    public List<DateTime> AlternativeTimes { get; }

    public AppointmentConflictException(string message, List<DateTime> alternativeTimes) 
        : base(message)
    {
        AlternativeTimes = alternativeTimes;
    }
} 