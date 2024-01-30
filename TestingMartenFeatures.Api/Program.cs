using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Options;
using TestingMartenFeatures.Api;
using TestingMartenFeatures.Api.Features;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg =>
{
    cfg.Lifetime = ServiceLifetime.Scoped;
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});
builder.Services.Configure<MartenOptions>(builder.Configuration.GetSection(nameof(MartenOptions)));
builder.Services.AddMarten(sp =>
{
    var martenOptions = sp.GetRequiredService<IOptions<MartenOptions>>().Value;
    if (martenOptions is null) throw new ArgumentException("MartenOptions is null");
    var options = new StoreOptions();
    options.Connection(martenOptions.ConnectionString);

    options.Projections.Add<TodoItemProjection>(ProjectionLifecycle.Inline);

    options.AutoCreateSchemaObjects = AutoCreate.All;
    //Maybe incorrect use of tenants but it works for auto create the postgres database
    options.CreateDatabasesForTenants(c => { c.ForTenant("TestingMartenFeatures"); });
    return options;
}).ApplyAllDatabaseChangesOnStartup();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();

public partial class Program;