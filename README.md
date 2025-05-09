# Medical Appointment Scheduler

A medical appointment scheduling system with a REST API, web interface, and console client.

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code
- SQL Server (for the database)

## Project Structure

- `MedicalAppointmentScheduler.API` - The REST API project
- `MedicalAppointmentScheduler.Console` - Console client application
- `MedicalAppointmentScheduler.Web` - Web interface
- `MedicalAppointmentScheduler.Tests` - Unit tests project

## Running the Application

### 1. Start the API

1. Open a terminal in the solution directory
2. Navigate to the API project:
   ```bash
   cd MedicalAppointmentScheduler.API
   ```
3. Run the API:
   ```bash
   dotnet run
   ```
4. The API will start on `https://localhost:7142`

### 2. Run the Web Interface

1. Open a new terminal in the solution directory
2. Navigate to the Web project:
   ```bash
   cd MedicalAppointmentScheduler.Web
   ```
3. Run the web application:
   ```bash
   dotnet run
   ```
4. Open your browser and navigate to `https://localhost:5001`

The web interface provides a calendar view where you can:
- View all appointments in a calendar format
- Create new appointments by selecting time slots
- Edit existing appointments
- Delete appointments
- Handle scheduling conflicts with alternative time suggestions

### 3. Run the Console Application

1. Open a new terminal in the solution directory
2. Navigate to the Console project:
   ```bash
   cd MedicalAppointmentScheduler.Console
   ```
3. Run the console application:
   ```bash
   dotnet run
   ```

## Using the Console Application

The console application provides a menu-driven interface to interact with the API:

1. List all appointments
2. Get appointment by ID
3. Create new appointment
4. Update appointment
5. Delete appointment
6. Exit

### Example: Creating an Appointment

1. Select option 3 from the menu
2. Enter the requested information:
   ```
   Enter patient name: John Doe
   Enter healthcare professional name: Dr. Smith
   Enter appointment date (yyyy-MM-dd HH:mm): 2024-05-20 14:30
   Enter duration in minutes: 30
   Enter description (optional): Regular checkup
   ```

### Example: Viewing Appointments

1. Select option 1 to list all appointments
2. Each appointment will be displayed with its details:
   ```
   ID: 1
   Patient: John Doe
   Healthcare Professional: Dr. Smith
   Date: 3/20/2024 2:30:00 PM
   Duration: 30 minutes
   Description: Regular checkup
   ```

## Running Unit Tests

The project includes unit tests for both the API and business logic. To run the tests:

1. Open a terminal in the solution directory
2. Navigate to the Tests project:
   ```bash
   cd MedicalAppointmentScheduler.Tests
   ```
3. Run the tests:
   ```bash
   dotnet test
   ```

The test suite includes:
- Appointment creation and validation tests
- Scheduling conflict detection tests
- Alternative time suggestion tests
- API endpoint tests
- Service layer tests

## Notes

- Make sure the API is running before starting the console application or web interface
- The console application and web interface connect to the API at `https://localhost:7142`
- If you get scheduling conflicts when creating appointments, the system will suggest alternative times
- All times are handled in the local timezone of the server
- The web interface requires a modern browser with JavaScript enabled
- Unit tests use an in-memory database for testing
