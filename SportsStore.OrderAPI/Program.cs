using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SportsStore.OrderAPI.Consumers;
using SportsStore.OrderAPI.Data;
using SportsStore.OrderAPI.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
    config.WriteTo.Console()
          .WriteTo.File("logs/orderapi-.log", rollingInterval: RollingInterval.Day)
          .Enrich.WithProperty("ServiceName", "OrderAPI")
          .Enrich.FromLogContext();
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure SQLite Database
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("OrderDatabase") 
        ?? "Data Source=orderapi.db"));

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Configure MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<InventoryResultConsumer>();
    x.AddConsumer<PaymentResultConsumer>();
    x.AddConsumer<ShippingResultConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQ");
        cfg.Host(rabbitMqSettings["Host"] ?? "localhost", "/", h =>
        {
            h.Username(rabbitMqSettings["Username"] ?? "guest");
            h.Password(rabbitMqSettings["Password"] ?? "guest");
        });

        cfg.ReceiveEndpoint("orderapi-inventory-results", e =>
        {
            e.ConfigureConsumer<InventoryResultConsumer>(context);
        });

        cfg.ReceiveEndpoint("orderapi-payment-results", e =>
        {
            e.ConfigureConsumer<PaymentResultConsumer>(context);
        });

        cfg.ReceiveEndpoint("orderapi-shipping-results", e =>
        {
            e.ConfigureConsumer<ShippingResultConsumer>(context);
        });
    });
});

// Add CORS for frontend applications
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5001",  // Blazor
                "http://localhost:3000",  // React Admin
                "http://localhost:5173"   // Vite dev server
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestPath", httpContext.Request.Path);
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
    };
});

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.EnsureCreated();
    SeedData.EnsurePopulated(app);
}

app.Logger.LogInformation("OrderAPI starting. Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();
