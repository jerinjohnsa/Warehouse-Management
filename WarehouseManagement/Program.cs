
using WarehouseManagement.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<DbConnectionFactory>();

builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5042",
                "https://localhost:7293"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Temporarily disabled for local HTTP testing
// app.UseHttpsRedirection();

// CORS must be before authorization
app.UseCors("AllowMVC");

app.UseAuthorization();

app.MapControllers();

app.Run();
