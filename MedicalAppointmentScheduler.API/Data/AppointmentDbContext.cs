using Microsoft.EntityFrameworkCore;
using MedicalAppointmentScheduler.API.Models;

namespace MedicalAppointmentScheduler.API.Data;

public class AppointmentDbContext : DbContext
{
    public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.HealthcareProfessionalName, a.AppointmentDate });
    }
} 