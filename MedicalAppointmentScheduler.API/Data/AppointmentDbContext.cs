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

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.PatientName).IsRequired();
            entity.Property(e => e.HealthcareProfessionalName).IsRequired();
            entity.Property(e => e.AppointmentDate).IsRequired();
            entity.Property(e => e.Duration).IsRequired();
        });
    }
} 