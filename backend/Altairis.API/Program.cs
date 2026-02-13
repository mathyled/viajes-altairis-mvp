using Altairis.Application.Services;
using Altairis.Domain.Entities;
using Altairis.Domain.Interfaces;
using Altairis.Infrastructure.Data;
using Altairis.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Altairis Hotel Reservation API", 
        Version = "v1",
        Description = "API para el sistema de reservas hoteleras Altairis",
        Contact = new()
        {
            Name = "Altairis Team"
        }
    });
    
    // Habilitar comentarios XML para documentación en Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<AltairisDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar Repositories
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IRepository<RoomType>, Repository<RoomType>>();
builder.Services.AddScoped<IRepository<Hotel>, Repository<Hotel>>();

// Registrar Services
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

// Configurar CORS: permitir solo el frontend en http://localhost:3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed de datos iniciales
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AltairisDbContext>();
        
        context.Database.Migrate();
        
        await DbInitializer.SeedAsync(context);
        
        Console.WriteLine("Base de datos inicializada correctamente con datos de prueba");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar la base de datos");
    }
}

// Configurar el pipeline HTTP
// Dejamos Swagger siempre habilitado en desarrollo local para facilitar pruebas.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Altairis API v1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz
});

// En este proyecto solo exponemos HTTP en localhost:5000, sin HTTPS,
// así evitamos problemas de redirección cuando no hay puerto HTTPS configurado.
// Si en un futuro se agrega HTTPS, se puede volver a habilitar.
// app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════╗
║                  ALTAIRIS API INICIADA                    ║
║                                                           ║
║  Swagger UI: http://localhost:5000                       ║
║  API Base:   http://localhost:5000/api                   ║
║                                                           ║
║  Endpoints disponibles:                                   ║
║  - GET    /api/hotels                                    ║
║  - GET    /api/hotels/paged?pageNumber=1&pageSize=10    ║
║  - POST   /api/reservations                              ║
║  - POST   /api/inventory/check-availability              ║
║                                                           ║
║  Base de datos inicializada con datos de prueba          ║
╚═══════════════════════════════════════════════════════════╝
");

app.Run();
