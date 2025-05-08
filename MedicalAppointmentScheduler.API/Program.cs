using Microsoft.EntityFrameworkCore;
using MedicalAppointmentScheduler.API.Data;
using MedicalAppointmentScheduler.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseInMemoryDatabase("AppointmentsDb"));

// Add Services
builder.Services.AddScoped<AppointmentService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enforce HTTPS
app.UseHttpsRedirection();

// Remove HTTP
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Set default page
app.MapFallbackToFile("index.html");

app.Run();
