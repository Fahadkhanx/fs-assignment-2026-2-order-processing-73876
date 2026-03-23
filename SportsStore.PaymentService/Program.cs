using MassTransit;
using Serilog;
using SportsStore.PaymentService.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
    config.WriteTo.Console()
          .WriteTo.File("logs/payment-service-.log", rollingInterval: RollingInterval.Day)
          .Enrich.WithProperty("ServiceName", "PaymentService")
          .Enrich.FromLogContext();
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<InventoryConfirmedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQ");
        cfg.Host(rabbitMqSettings["Host"] ?? "localhost", "/", h =>
        {
            h.Username(rabbitMqSettings["Username"] ?? "guest");
            h.Password(rabbitMqSettings["Password"] ?? "guest");
        });

        cfg.ReceiveEndpoint("payment-service", e =>
        {
            e.ConfigureConsumer<InventoryConfirmedConsumer>(context);
        });
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
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("Payment Service starting. Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();
