using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace MedicalAppointmentScheduler.Console;

public class Program
{
    private static readonly HttpClient client = new HttpClient();
    private const string BaseUrl = "https://localhost:7142/api/appointments";

    public static async Task Main(string[] args)
    {
        try
        {
            while (true)
            {
                System.Console.WriteLine("\nMedical Appointment Scheduler Console");
                System.Console.WriteLine("1. List all appointments");
                System.Console.WriteLine("2. Get appointment by ID");
                System.Console.WriteLine("3. Create new appointment");
                System.Console.WriteLine("4. Update appointment");
                System.Console.WriteLine("5. Delete appointment");
                System.Console.WriteLine("6. Exit");
                System.Console.Write("\nSelect an option: ");

                var choice = System.Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await ListAllAppointments();
                        break;
                    case "2":
                        await GetAppointmentById();
                        break;
                    case "3":
                        await CreateAppointment();
                        break;
                    case "4":
                        await UpdateAppointment();
                        break;
                    case "5":
                        await DeleteAppointment();
                        break;
                    case "6":
                        return;
                    default:
                        System.Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static async Task ListAllAppointments()
    {
        var appointments = await client.GetFromJsonAsync<List<Appointment>>(BaseUrl);
        if (appointments != null)
        {
            foreach (var appointment in appointments)
            {
                DisplayAppointment(appointment);
            }
        }
    }

    private static async Task GetAppointmentById()
    {
        System.Console.Write("Enter appointment ID: ");
        if (int.TryParse(System.Console.ReadLine(), out int id))
        {
            var appointment = await client.GetFromJsonAsync<Appointment>($"{BaseUrl}/{id}");
            if (appointment != null)
            {
                DisplayAppointment(appointment);
            }
            else
            {
                System.Console.WriteLine("Appointment not found.");
            }
        }
    }

    private static async Task CreateAppointment()
    {
        var appointment = GetAppointmentDetails();
        var response = await client.PostAsJsonAsync(BaseUrl, appointment);
        
        if (response.IsSuccessStatusCode)
        {
            var createdAppointment = await response.Content.ReadFromJsonAsync<Appointment>();
            System.Console.WriteLine("Appointment created successfully:");
            DisplayAppointment(createdAppointment);
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var conflict = await response.Content.ReadFromJsonAsync<ConflictResponse>();
            System.Console.WriteLine("Cannot create appointment due to scheduling conflict.");
            System.Console.WriteLine("Alternative times available:");
            foreach (var time in conflict.AlternativeTimes)
            {
                System.Console.WriteLine($"- {time}");
            }
        }
        else
        {
            System.Console.WriteLine($"Error creating appointment: {response.StatusCode}");
        }
    }

    private static async Task UpdateAppointment()
    {
        System.Console.Write("Enter appointment ID to update: ");
        if (int.TryParse(System.Console.ReadLine(), out int id))
        {
            var appointment = GetAppointmentDetails();
            var response = await client.PutAsJsonAsync($"{BaseUrl}/{id}", appointment);
            
            if (response.IsSuccessStatusCode)
            {
                var updatedAppointment = await response.Content.ReadFromJsonAsync<Appointment>();
                System.Console.WriteLine("Appointment updated successfully:");
                DisplayAppointment(updatedAppointment);
            }
            else
            {
                System.Console.WriteLine($"Error updating appointment: {response.StatusCode}");
            }
        }
    }

    private static async Task DeleteAppointment()
    {
        System.Console.Write("Enter appointment ID to delete: ");
        if (int.TryParse(System.Console.ReadLine(), out int id))
        {
            var response = await client.DeleteAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                System.Console.WriteLine("Appointment deleted successfully.");
            }
            else
            {
                System.Console.WriteLine($"Error deleting appointment: {response.StatusCode}");
            }
        }
    }

    private static Appointment GetAppointmentDetails()
    {
        System.Console.Write("Enter patient name: ");
        var patientName = System.Console.ReadLine();

        System.Console.Write("Enter healthcare professional name: ");
        var healthcareProfessionalName = System.Console.ReadLine();

        System.Console.Write("Enter appointment date (yyyy-MM-dd HH:mm): ");
        var dateStr = System.Console.ReadLine();
        DateTime appointmentDate = DateTime.Parse(dateStr);

        System.Console.Write("Enter duration in minutes: ");
        var duration = int.Parse(System.Console.ReadLine());

        System.Console.Write("Enter description (optional): ");
        var description = System.Console.ReadLine();

        return new Appointment
        {
            PatientName = patientName,
            HealthcareProfessionalName = healthcareProfessionalName,
            AppointmentDate = appointmentDate,
            Duration = duration,
            Description = description
        };
    }

    private static void DisplayAppointment(Appointment appointment)
    {
        System.Console.WriteLine($"\nID: {appointment.Id}");
        System.Console.WriteLine($"Patient: {appointment.PatientName}");
        System.Console.WriteLine($"Healthcare Professional: {appointment.HealthcareProfessionalName}");
        System.Console.WriteLine($"Date: {appointment.AppointmentDate}");
        System.Console.WriteLine($"Duration: {appointment.Duration} minutes");
        if (!string.IsNullOrEmpty(appointment.Description))
        {
            System.Console.WriteLine($"Description: {appointment.Description}");
        }
        System.Console.WriteLine();
    }
}

public class Appointment
{
    public int Id { get; set; }
    public string PatientName { get; set; }
    public string HealthcareProfessionalName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int Duration { get; set; }
    public string Description { get; set; }
}

public class ConflictResponse
{
    public string Message { get; set; }
    public List<DateTime> AlternativeTimes { get; set; }
} 